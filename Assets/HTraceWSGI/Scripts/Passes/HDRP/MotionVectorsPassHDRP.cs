//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Wrappers;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HTraceWSGI.Scripts.Passes.HDRP
{
    internal class MotionVectorsPassHDRP : ScriptableRenderPass
    {
        // Shader properties
        public static readonly int StencilRef = Shader.PropertyToID("_StencilRef");
        public static readonly int StencilMask = Shader.PropertyToID("_StencilMask");

        // Samplers
        internal static readonly ProfilingSamplerHTrace MotionVectorsProfilingSampler = new("Motion Vectors", HNames.HTRACE_MV_PASS_NAME, priority: 0);
        internal static readonly ProfilingSamplerHTrace CopyingStencilMovingObjectProfilingSampler = new("Copying stencil moving object", HNames.HTRACE_MV_PASS_NAME, priority: 1);
        internal static readonly ProfilingSamplerHTrace ComposeMotionVectorsProfilingSampler = new("Compose Motion Vectors", HNames.HTRACE_MV_PASS_NAME, priority: 2);

        // Materials
        internal static Material CameraMotionVectorsMaterial_HDRP;

        // Textures
        internal static RTWrapper CustomCameraMotionVectors = new RTWrapper();

        private bool _initialized = false;

        public MotionVectorsPassHDRP()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingGbuffer+2;
            //// 请求MotionVectors Buffer
            //ConfigureInput(ScriptableRenderPassInput.Motion | ScriptableRenderPassInput.Depth);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!_initialized)
            {
                // URP中没有HDRP的CameraMotionVectors shader，用URP自己的
                if (CameraMotionVectorsMaterial_HDRP == null)
                    CameraMotionVectorsMaterial_HDRP = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Universal Render Pipeline/CameraMotionVectors"));

                CustomCameraMotionVectors?.HRelease();
                CustomCameraMotionVectors.HTextureAlloc("_CustomCameraMotionVectors", Vector2.one, GraphicsFormat.R16G16_SFloat);

                _initialized = true;
            }
        }

        public override void Execute(ScriptableRenderContext renderContext, ref RenderingData renderingData)
        {

            var camera = renderingData.cameraData.camera;

            if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
                return;

            var cmd = CommandBufferPool.Get(HNames.HTRACE_MV_PASS_NAME);

            try
            {
                var renderer = renderingData.cameraData.renderer as UniversalRenderer;

                using (new HTraceProfilingScope(cmd, MotionVectorsProfilingSampler))
                {
                    CameraMotionVectors(cmd, renderer);
                }

                renderContext.ExecuteCommandBuffer(cmd);
            }
            finally
            {
                CommandBufferPool.Release(cmd);
            }
        }


        private static void CameraMotionVectors(CommandBuffer cmd, UniversalRenderer renderer)
        {
            using (new HTraceProfilingScope(cmd, ComposeMotionVectorsProfilingSampler))
            {
                //CameraMotionVectorsMaterial_HDRP.SetInt(StencilRef, 32);
                //CameraMotionVectorsMaterial_HDRP.SetInt(StencilMask, 32);

                //// 拷贝MotionVectors到自定义RT
                //Blitter.BlitCameraTexture(cmd, cameraMotionVectorsBuffer, CustomCameraMotionVectors.rt);

                // 叠加Camera MotionVectors（背景部分）
                cmd.Blit(renderer.cameraDepthTargetHandle, CustomCameraMotionVectors.rt, CameraMotionVectorsMaterial_HDRP, 0);
                //CoreUtils.SetRenderTarget(cmd, CustomCameraMotionVectors.rt, ClearFlag.Color);
                //CoreUtils.DrawFullScreen(cmd, CameraMotionVectorsMaterial_HDRP, CustomCameraMotionVectors.rt, renderer.cameraDepthTargetHandle, shaderPassId: 0);

                cmd.SetGlobalTexture(HShaderParams.g_HTraceMotionVectors, CustomCameraMotionVectors.rt);
            }
        }

        public void Cleanup()
        {

            CustomCameraMotionVectors?.HRelease();
            _initialized = false;
        }
    }
}