//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Wrappers;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using HTraceWSGI.Scripts.Passes.Shared;

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
        // internal static RTWrapper CustomCameraMotionVectors = new RTWrapper();

        private bool _initialized = false;

        public MotionVectorsPassHDRP()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingGbuffer+2;
            //// ����MotionVectors Buffer
            //ConfigureInput(ScriptableRenderPassInput.Motion | ScriptableRenderPassInput.Depth);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!_initialized)
            {
                // URP��û��HDRP��CameraMotionVectors shader����URP�Լ���
                if (CameraMotionVectorsMaterial_HDRP == null)
                    CameraMotionVectorsMaterial_HDRP = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Universal Render Pipeline/CameraMotionVectors"));

                SoftwareTracingShared.CustomCameraMotionVectors?.HRelease();
                SoftwareTracingShared.CustomCameraMotionVectors.HTextureAlloc("_CustomCameraMotionVectors", Vector2.one, GraphicsFormat.R16G16_SFloat);

                _initialized = true;
            }
        }

        Matrix4x4 _NonJitteredViewProjMatrix;
        Matrix4x4 _PrevViewProjMatrix;

        public Matrix4x4 prevInvViewProjMatrix;
		
		Vector3 prevWorldSpaceCameraPos;
        bool isFirst = true;

        public override void Execute(ScriptableRenderContext renderContext, ref RenderingData renderingData)
        {

            var camera = renderingData.cameraData.camera;

            if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
                return;

            var cmd = CommandBufferPool.Get(HNames.HTRACE_MV_PASS_NAME);

		    // Matrix4x4 currentViewMatrix = camera.worldToCameraMatrix;
            // // Debug.Log("currentViewMatrix = "+currentViewMatrix);
			// currentViewMatrix.SetColumn(3, new Vector4(0, 0, 0, 1));
            // // Debug.Log("currentViewMatrix2 = "+currentViewMatrix);
			// Matrix4x4 currentProjMatrix = GL.GetGPUProjectionMatrix(camera.nonJitteredProjectionMatrix, true); // Had to change this from 'false'

            // _NonJitteredViewProjMatrix = currentProjMatrix * currentViewMatrix;
            // cmd.SetGlobalMatrix(HShaderParams._NonJitteredViewProjMatrix, _NonJitteredViewProjMatrix);

            // if (isFirst)
            // {
            //     _PrevViewProjMatrix = _NonJitteredViewProjMatrix;
            //     isFirst = false;
            // }
            // cmd.SetGlobalMatrix(HShaderParams._PrevViewProjMatrix, _PrevViewProjMatrix);
            // _PrevViewProjMatrix = _NonJitteredViewProjMatrix;


            Matrix4x4 currentViewMatrix = camera.worldToCameraMatrix;
            // Debug.Log("currentViewMatrix = "+currentViewMatrix);
			currentViewMatrix.SetColumn(3, new Vector4(0, 0, 0, 1));
            // Debug.Log("currentViewMatrix2 = "+currentViewMatrix);
			Matrix4x4 currentProjMatrix = GL.GetGPUProjectionMatrix(camera.nonJitteredProjectionMatrix, true); // Had to change this from 'false'

			Vector3 cameraPosition = camera.transform.position;

            _NonJitteredViewProjMatrix = currentProjMatrix * currentViewMatrix;
   
            cmd.SetGlobalMatrix(HShaderParams._NonJitteredViewProjMatrix, _NonJitteredViewProjMatrix);
            cmd.SetGlobalMatrix(HShaderParams._NonJitteredViewProjMatrix2, _NonJitteredViewProjMatrix);
            cmd.SetGlobalMatrix(HShaderParams._ViewMatrix2, currentViewMatrix);
            cmd.SetGlobalMatrix(HShaderParams._ViewProjMatrix2, _NonJitteredViewProjMatrix);

            Matrix4x4 _InvViewMatrix = currentViewMatrix.inverse;
            cmd.SetGlobalMatrix(HShaderParams._InvViewMatrix2, _InvViewMatrix);

 			// cmd.SetGlobalMatrix(HShaderParams.unity_MatrixV2, currentViewMatrix);
 			cmd.SetGlobalMatrix(HShaderParams.unity_MatrixInvV2, _InvViewMatrix);
 			// cmd.SetGlobalMatrix(HShaderParams.unity_MatrixVP2, _NonJitteredViewProjMatrix);


            if (isFirst)
            {
				prevWorldSpaceCameraPos = cameraPosition;
                _PrevViewProjMatrix = _NonJitteredViewProjMatrix;
                isFirst = false;
            }

			Vector3 cameraDisplacement = cameraPosition - prevWorldSpaceCameraPos;

			_PrevViewProjMatrix *= Matrix4x4.Translate(cameraDisplacement);
			prevInvViewProjMatrix = _PrevViewProjMatrix.inverse;
			
            cmd.SetGlobalMatrix(HShaderParams._PrevViewProjMatrix, _PrevViewProjMatrix);
            cmd.SetGlobalMatrix(HShaderParams._PrevViewProjMatrix2, _PrevViewProjMatrix);
            

            Matrix4x4 _NonJitteredViewProjMatrixInverse = _NonJitteredViewProjMatrix.inverse;
			cmd.SetGlobalMatrix(HShaderParams._InvViewProjMatrix2, _NonJitteredViewProjMatrixInverse);
 			cmd.SetGlobalMatrix(HShaderParams._PrevInvViewProjMatrix2, prevInvViewProjMatrix);
 			cmd.SetGlobalMatrix(HShaderParams.unity_MatrixInvVP2, _NonJitteredViewProjMatrixInverse);


			_PrevViewProjMatrix = _NonJitteredViewProjMatrix;

			prevWorldSpaceCameraPos = cameraPosition;

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

                //// ����MotionVectors���Զ���RT
                //Blitter.BlitCameraTexture(cmd, cameraMotionVectorsBuffer, CustomCameraMotionVectors.rt);

                // ����Camera MotionVectors���������֣�
                // cmd.Blit(renderer.cameraDepthTargetHandle, CustomCameraMotionVectors.rt, CameraMotionVectorsMaterial_HDRP, 0);
                //CoreUtils.SetRenderTarget(cmd, CustomCameraMotionVectors.rt, ClearFlag.Color);
                //CoreUtils.DrawFullScreen(cmd, CameraMotionVectorsMaterial_HDRP, CustomCameraMotionVectors.rt, renderer.cameraDepthTargetHandle, shaderPassId: 0);

                
                
                CoreUtils.SetRenderTarget(cmd, SoftwareTracingShared.CustomCameraMotionVectors.rt, ClearFlag.Color);
				CoreUtils.DrawFullScreen(cmd, CameraMotionVectorsMaterial_HDRP, SoftwareTracingShared.CustomCameraMotionVectors.rt, renderer.cameraDepthTargetHandle, shaderPassId: 0);


                cmd.SetGlobalTexture(HShaderParams.g_HTraceMotionVectors, SoftwareTracingShared.CustomCameraMotionVectors.rt);
            }
        }

        public void Cleanup()
        {

            SoftwareTracingShared.CustomCameraMotionVectors?.HRelease();
            _initialized = false;
        }
    }
}