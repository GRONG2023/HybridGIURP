//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Data.Private;
using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Wrappers;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using HTraceWSGI.Scripts.Editor;
#endif

namespace HTraceWSGI.Scripts.Passes.HDRP
{
    internal class FinalPassHDRP : ScriptableRenderPass
    {
        private enum HDebugKernel
        {
            Debug = 0,
        }

        // Shader properties
        private static readonly int Debug_Output = Shader.PropertyToID("_Debug_Output");
        private static readonly int DebugSwitch = Shader.PropertyToID("_DebugSwitch");
        private static readonly int BuffersSwitch = Shader.PropertyToID("_BuffersSwitch");
        private static readonly int DepthPyramidLod = Shader.PropertyToID("_DepthPyramidLod");

        // Samplers
        private readonly ProfilingSamplerHTrace DebugProfilingSampler = new ProfilingSamplerHTrace("Debug Output", parentName: HNames.HTRACE_FINAL_PASS_NAME, priority: 0);

        // Buffers & etc
        internal static ComputeShader HDebug = null;
        internal static ComputeShader HReflectionProbeCompose = null;

        // Textures
        internal static RTWrapper OutputTarget = new RTWrapper();

        internal struct HistoryData : IHistoryData
        {
            public GraphicsFormat GraphicsFormat;

            public void Update()
            {
                // URP�й̶�ʹ��R16G16B16A16_SFloat��û��HDRP��colorBufferFormat����
                GraphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
            }
        }

        internal static HistoryData History = new HistoryData() { };

        private bool _initialized = false;

        public FinalPassHDRP()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!_initialized)
            {
                if (HDebug == null) HDebug = HExtensions.LoadComputeShader("HDebug");
                if (HReflectionProbeCompose == null) HReflectionProbeCompose = HExtensions.LoadComputeShader("HReflectionProbeCompose");

                OutputTarget?.HRelease();
                // URP�й̶�ʹ��R16G16B16A16_SFloat
                OutputTarget.HTextureAlloc("_OutputTarget", Vector2.one, GraphicsFormat.R16G16B16A16_SFloat);

                _initialized = true;
            }
        }

        public override void Execute(ScriptableRenderContext renderContext, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            int width = renderingData.cameraData.cameraTargetDescriptor.width;
            int height = renderingData.cameraData.cameraTargetDescriptor.height;

            if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
                return;

            var cmd = CommandBufferPool.Get(HNames.HTRACE_FINAL_PASS_NAME);

            try
            {
#if UNITY_EDITOR
                if (SceneViewDrawModeTracker.IsUnlit && camera.cameraType == CameraType.SceneView)
                    return;
#endif
                var renderer = renderingData.cameraData.renderer as UniversalRenderer;
                RTHandle cameraColorBuffer = renderer.cameraColorTargetHandle;

                VoxelizationPassHDRP.VisualizeVoxels(cmd, camera, width, height, renderContext);
                // Shared.LightClusterShared.DebugLightCluster(cmd, camera, width, height);

                if (HSettings.GeneralSettings.DebugModeWS == DebugModeWS.None)
                    return;

                ReallocateConditions(width, height);

                using (new HTraceProfilingScope(cmd, DebugProfilingSampler))
                {
                    // Render debug
                    cmd.SetComputeTextureParam(HDebug, (int)HDebugKernel.Debug, Debug_Output, OutputTarget.rt, 0);
                    cmd.SetComputeIntParam(HDebug, DebugSwitch, (int)HSettings.GeneralSettings.DebugModeWS);
                    cmd.SetComputeIntParam(HDebug, BuffersSwitch, (int)HSettings.GeneralSettings.HBuffer);
                    // URP��TextureXrSlices�̶�Ϊ1
                    cmd.DispatchCompute(HDebug, (int)HDebugKernel.Debug, Mathf.CeilToInt(width / 8.0f), Mathf.CeilToInt(height / 8.0f), 1);

                    Blitter.BlitCameraTexture(cmd, OutputTarget.rt, cameraColorBuffer);
                }

                History.Update();

                renderContext.ExecuteCommandBuffer(cmd);
            }
            finally
            {
                CommandBufferPool.Release(cmd);
            }
        }

        private void ReallocateConditions(int width, int height)
        {
            // URP�й̶�ʹ��R16G16B16A16_SFloat������Ҫ���colorBufferFormat
            var graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
            if (History.GraphicsFormat != graphicsFormat)
            {
                OutputTarget.HRelease();
                OutputTarget.HTextureAlloc("_OutputTarget", Vector2.one, graphicsFormat);
            }
        }

        public void Cleanup()
        {

            OutputTarget?.HRelease();
            _initialized = false;
        }
    }
}