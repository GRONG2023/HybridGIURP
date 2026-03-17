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

namespace HTraceWSGI.Scripts.Passes.HDRP
{
	internal class GBufferPassHDRP : ScriptableRenderPass
	{
		private enum HDepthPyramidKernel
		{
			GenerateDepthPyramid_1 = 0,
			GenerateDepthPyramid_2 = 1,
		}

		// DepthPyramid
		private static readonly int _DepthIntermediate        = Shader.PropertyToID("_DepthIntermediate");
		private static readonly int _DepthIntermediate_Output = Shader.PropertyToID("_DepthIntermediate_Output");
		private static readonly int _DepthPyramid_OutputMIP0  = Shader.PropertyToID("_DepthPyramid_OutputMIP0");
		private static readonly int _DepthPyramid_OutputMIP1  = Shader.PropertyToID("_DepthPyramid_OutputMIP1");
		private static readonly int _DepthPyramid_OutputMIP2  = Shader.PropertyToID("_DepthPyramid_OutputMIP2");
		private static readonly int _DepthPyramid_OutputMIP3  = Shader.PropertyToID("_DepthPyramid_OutputMIP3");
		private static readonly int _DepthPyramid_OutputMIP4  = Shader.PropertyToID("_DepthPyramid_OutputMIP4");
		private static readonly int _DepthPyramid_OutputMIP5  = Shader.PropertyToID("_DepthPyramid_OutputMIP5");
		private static readonly int _DepthPyramid_OutputMIP6  = Shader.PropertyToID("_DepthPyramid_OutputMIP6");
		private static readonly int _DepthPyramid_OutputMIP7  = Shader.PropertyToID("_DepthPyramid_OutputMIP7");
		private static readonly int _DepthPyramid_OutputMIP8  = Shader.PropertyToID("_DepthPyramid_OutputMIP8");

		// Materials & Computes
		internal static Material      ColorCompose_BIRP;
		internal static ComputeShader HDepthPyramid = null;

		// Samplers
		internal static readonly ProfilingSamplerHTrace GBufferProfilingSampler                = new ProfilingSamplerHTrace("GBuffer", parentName: HNames.HTRACE_PRE_PASS_NAME, priority: 1);
		private static readonly  ProfilingSamplerHTrace DepthPyramidGenerationProfilingSampler = new ProfilingSamplerHTrace("Depth Pyramid Generation", parentName: HNames.HTRACE_PRE_PASS_NAME, priority: 2);

		// Textures
		internal static RTWrapper Dummy                     = new RTWrapper();
		internal static RTWrapper DepthPyramidRT            = new RTWrapper();
		internal static RTWrapper DepthIntermediate_Pyramid = new RTWrapper();

		// MRT Arrays
		internal static RenderTargetIdentifier[] GBufferMRT = null;

		// Misc
		internal static RenderStateBlock ForwardGBufferRenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

		private bool _initialized = false;

		public GBufferPassHDRP()
		{
			renderPassEvent = RenderPassEvent.AfterRenderingGbuffer+1;
		}

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
            if (!_initialized)
            {
                Debug.Log("OnCameraSetup");
                if (HDepthPyramid == null)
                    HDepthPyramid = HExtensions.LoadComputeShader("HDepthPyramid");

                Dummy?.HRelease();
                DepthPyramidRT?.HRelease();
                DepthIntermediate_Pyramid?.HRelease();

                Dummy.HTextureAlloc("_Dummy", 4, 4, GraphicsFormat.R8G8B8A8_UNorm);
                DepthPyramidRT.HTextureAlloc("_DepthPyramid", Vector2.one, GraphicsFormat.R16_SFloat, useMipMap: true);
                DepthIntermediate_Pyramid.HTextureAlloc("_DepthIntermediate_Pyramid", Vector2.one / 16, GraphicsFormat.R16_SFloat);


                if (HDepthPyramid != null)
                {
                    _initialized = true;
                }
            }

            
            
        }

        public override void Execute(ScriptableRenderContext renderContext, ref RenderingData renderingData)
		{
			var camera = renderingData.cameraData.camera;
			int width  = renderingData.cameraData.cameraTargetDescriptor.width;
			int height = renderingData.cameraData.cameraTargetDescriptor.height;

			if (HDepthPyramid == null)
			{
				return;
			}

			if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
				return;

			var cmd = CommandBufferPool.Get(HNames.HTRACE_GBUFFER_PASS_NAME);

			try
			{
				GBufferGeneration(cmd, renderingData);


                using (new HTraceProfilingScope(cmd, DepthPyramidGenerationProfilingSampler))
				{
					// Generate 0-4 mip levels
					cmd.SetComputeTextureParam(HDepthPyramid, (int)HDepthPyramidKernel.GenerateDepthPyramid_1, _DepthPyramid_OutputMIP0,  DepthPyramidRT.rt, 0);
					cmd.SetComputeTextureParam(HDepthPyramid, (int)HDepthPyramidKernel.GenerateDepthPyramid_1, _DepthPyramid_OutputMIP1,  DepthPyramidRT.rt, 1);
					cmd.SetComputeTextureParam(HDepthPyramid, (int)HDepthPyramidKernel.GenerateDepthPyramid_1, _DepthPyramid_OutputMIP2,  DepthPyramidRT.rt, 2);
					cmd.SetComputeTextureParam(HDepthPyramid, (int)HDepthPyramidKernel.GenerateDepthPyramid_1, _DepthPyramid_OutputMIP3,  DepthPyramidRT.rt, 3);
					cmd.SetComputeTextureParam(HDepthPyramid, (int)HDepthPyramidKernel.GenerateDepthPyramid_1, _DepthPyramid_OutputMIP4,  DepthPyramidRT.rt, 4);
					cmd.SetComputeTextureParam(HDepthPyramid, (int)HDepthPyramidKernel.GenerateDepthPyramid_1, _DepthIntermediate_Output, DepthIntermediate_Pyramid.rt);
					cmd.DispatchCompute(HDepthPyramid, (int)HDepthPyramidKernel.GenerateDepthPyramid_1, Mathf.CeilToInt(width / 16.0f), Mathf.CeilToInt(height / 16.0f), 1);

					// Generate 5-7 mip levels
					cmd.SetComputeTextureParam(HDepthPyramid, (int)HDepthPyramidKernel.GenerateDepthPyramid_2, _DepthIntermediate,       DepthIntermediate_Pyramid.rt);
					cmd.SetComputeTextureParam(HDepthPyramid, (int)HDepthPyramidKernel.GenerateDepthPyramid_2, _DepthPyramid_OutputMIP5, DepthPyramidRT.rt, 5);
					cmd.SetComputeTextureParam(HDepthPyramid, (int)HDepthPyramidKernel.GenerateDepthPyramid_2, _DepthPyramid_OutputMIP6, DepthPyramidRT.rt, 6);
					cmd.SetComputeTextureParam(HDepthPyramid, (int)HDepthPyramidKernel.GenerateDepthPyramid_2, _DepthPyramid_OutputMIP7, DepthPyramidRT.rt, 7);
					cmd.SetComputeTextureParam(HDepthPyramid, (int)HDepthPyramidKernel.GenerateDepthPyramid_2, _DepthPyramid_OutputMIP8, DepthPyramidRT.rt, 8);
					cmd.DispatchCompute(HDepthPyramid, (int)HDepthPyramidKernel.GenerateDepthPyramid_2, Mathf.CeilToInt(width / 16.0f / 8.0f), Mathf.CeilToInt(height / 16.0f / 8.0f), 1);

					cmd.SetGlobalTexture(HShaderParams.g_HTraceDepthPyramidWSGI, DepthPyramidRT.rt);
				}

				renderContext.ExecuteCommandBuffer(cmd);
			}
			finally
			{
				CommandBufferPool.Release(cmd);
			}
		}

		private static void GBufferGeneration(CommandBuffer cmd, RenderingData renderingData)
		{
			using (new HTraceProfilingScope(cmd, GBufferProfilingSampler))
			{
				var NativeGBuffer0  = Shader.GetGlobalTexture(HShaderParams._GBuffer0);

                // URP中没有litShaderMode，Forward模式下GBuffer0为null时用Dummy替代
                if (NativeGBuffer0 == null)
					NativeGBuffer0 = Dummy.rt;

				var cameraData = renderingData.cameraData;

                // URP中通过RTHandle获取相机的各个Buffer
                // _CameraDepthTexture      -> 深度
                // _CameraNormalsTexture    -> 法线
                // _CameraOpaqueTexture     -> 颜色
				cmd.SetGlobalTexture(HShaderParams.g_HTraceGBuffer0,        NativeGBuffer0);
                var NormalGBuffer0 = Shader.GetGlobalTexture(HShaderParams._GBuffer2);

                if (NormalGBuffer0 == null)
				{
                    NormalGBuffer0 = Dummy.rt;
                }
				cmd.SetGlobalTexture(HShaderParams.g_HTraceGBuffer2, NormalGBuffer0);
				cmd.SetGlobalTexture(HShaderParams.g_HTraceColor,           cameraData.renderer.cameraColorTargetHandle);
				cmd.SetGlobalTexture(HShaderParams.g_HTraceDepth, cameraData.renderer.cameraDepthTargetHandle);

			}
		}

        public override void OnCameraCleanup(CommandBuffer cmd)
        {

        }

        public void Cleanup()
		{
            Dummy?.HRelease();
            DepthPyramidRT?.HRelease();
            DepthIntermediate_Pyramid?.HRelease();
            _initialized = false;
        }
    }
}