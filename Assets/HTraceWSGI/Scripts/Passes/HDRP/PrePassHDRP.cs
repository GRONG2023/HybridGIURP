//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Data.Private;
using HTraceWSGI.Scripts.Extensions.CameraHistorySystem;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Passes.Shared;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HTraceWSGI.Scripts.Passes.HDRP
{
    internal class PrePassHDRP : ScriptableRenderPass
    {
        private RenderTexture _dummyRT1;
        private RenderTexture _dummyRT2;
        private bool _initialized = false;

        private struct HistoryCameraData : ICameraHistoryData
        {
            private int hash;
            public Matrix4x4 previousViewProjMatrix;
            public Matrix4x4 previousInvViewProjMatrix;
            public Vector4 previousHRenderScale;

            public int GetHash() => hash;
            public void SetHash(int hashIn) => this.hash = hashIn;
        }

        private static readonly CameraHistorySystem<HistoryCameraData> CameraHistorySystem = new CameraHistorySystem<HistoryCameraData>();

        public PrePassHDRP()
        {
            renderPassEvent = RenderPassEvent.BeforeRendering;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!_initialized)
            {
                _dummyRT1 = RenderTexture.GetTemporary(4, 4, 0, GraphicsFormat.R8_SNorm);
                _dummyRT1.dimension = TextureDimension.Tex2D;
                _initialized = true;
            }
        }

        public override void Execute(ScriptableRenderContext renderContext, ref RenderingData renderingData)
        {

            var camera = renderingData.cameraData.camera;

            if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
                return;

            var cmd = CommandBufferPool.Get(HNames.HTRACE_PRE_PASS_NAME);

            try
            {
                CameraHistorySystem.UpdateCameraHistoryIndex(camera.GetHashCode());
                CameraHistorySystem.UpdateCameraHistoryData();

                // -------------- HRenderScale -----------------
                ref var previousHRenderScale = ref CameraHistorySystem.GetCameraData().previousHRenderScale;
                cmd.SetGlobalVector(HShaderParams.HRenderScalePrevious, previousHRenderScale);

                // URP中RTHandles.rtHandleProperties获取方式和HDRP一致
                previousHRenderScale = new Vector4(
                    RTHandles.rtHandleProperties.rtHandleScale.x,
                    RTHandles.rtHandleProperties.rtHandleScale.y,
                    1 / RTHandles.rtHandleProperties.rtHandleScale.x,
                    1 / RTHandles.rtHandleProperties.rtHandleScale.y);
                cmd.SetGlobalVector(HShaderParams.HRenderScale, previousHRenderScale);

                // -------------- Matrix -----------------
                Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
                Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
                Matrix4x4 viewProjMatrix = projMatrix * viewMatrix;
                Matrix4x4 invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix);
                {
                    cmd.SetGlobalMatrix(HShaderParams.H_MATRIX_VP, viewProjMatrix);
                    cmd.SetGlobalMatrix(HShaderParams.H_MATRIX_I_VP, invViewProjMatrix);

                    ref var previousViewProjMatrix = ref CameraHistorySystem.GetCameraData().previousViewProjMatrix;
                    ref var previousInvViewProjMatrix = ref CameraHistorySystem.GetCameraData().previousInvViewProjMatrix;

                    cmd.SetGlobalMatrix(HShaderParams.H_MATRIX_PREV_VP, previousViewProjMatrix);
                    cmd.SetGlobalMatrix(HShaderParams.H_MATRIX_PREV_I_VP, previousInvViewProjMatrix);

                    previousViewProjMatrix = viewProjMatrix;
                    previousInvViewProjMatrix = invViewProjMatrix;
                }
                CameraHistorySystem.GetCameraData().SetHash(camera.GetHashCode());

                // Unity's blue noise is unreliable, so we'll use ours in all pipelines
                HBlueNoiseShared.SetTextures(cmd);

                // Dummy buffers for First frame
                cmd.SetGlobalTexture(HShaderParams.g_HTraceBufferGI, _dummyRT1);
                cmd.SetGlobalTexture(HShaderParams.g_GeometryNormal, _dummyRT1);

                // Dummy buffers for Debug
                if (HSettings.GeneralSettings.DebugModeWS != DebugModeWS.None)
                {
                    if (_dummyRT2 == null)
                    {
                        _dummyRT2 = RenderTexture.GetTemporary(4, 4, 0, GraphicsFormat.R16G16B16A16_SFloat);
                        _dummyRT2.dimension = TextureDimension.Tex2D;
                    }
                    cmd.SetGlobalTexture(HShaderParams.g_LightClusterDebug, _dummyRT2);
                }
                else
                {
                    RenderTexture.ReleaseTemporary(_dummyRT2);
                    _dummyRT2 = null;
                }

                renderContext.ExecuteCommandBuffer(cmd);
            }
            finally
            {
                CommandBufferPool.Release(cmd);
            }
        }

        public void Cleanup()
        {
            CameraHistorySystem.Cleanup();
            RenderTexture.ReleaseTemporary(_dummyRT1);
            RenderTexture.ReleaseTemporary(_dummyRT2);
            _dummyRT1 = null;
            _dummyRT2 = null;
            _initialized = false;
        }
    }
}