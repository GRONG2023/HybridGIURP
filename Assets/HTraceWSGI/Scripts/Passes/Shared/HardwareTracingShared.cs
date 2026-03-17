//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Wrappers;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_2021 || UNITY_2022
using UnityEngine.Experimental.Rendering;
#endif

namespace HTraceWSGI.Scripts.Passes.Shared
{
	internal static class HardwareTracingShared
	{
		// Shader properties
		private static readonly int _ShaderPropertysample = Shader.PropertyToID("_ShaderPropertysample");
		
		// Samplers
		private static readonly ProfilingSamplerHTrace SampleProfilingSampler = new ProfilingSamplerHTrace("Sample Profiling Sampler", parentName: HNames.HTRACE_HARDWARE_TRACING_PASS_NAME, priority: 0);
		internal static readonly ProfilingSamplerHTrace s_LightEvaluationProfilingSampler              = new ProfilingSamplerHTrace("Light Evaluation",               parentName: HNames.HTRACE_HARDWARE_TRACING_PASS_NAME, priority: 11);

		
		// Materials
		internal static Material VoxelVisualizationMaterial = null;
		
		internal static ComputeBuffer    PrevBuffer  = null;
		internal static ComputeShader    TestCompute = null;
		internal static RayTracingShader HRayTracing = null;
		
		internal static RTWrapper TestBuffer = new RTWrapper();
		internal static RTWrapper RayTracedGBufferPayload = new RTWrapper();
		internal static RTWrapper VoxelVisualizationRayDirections = new RTWrapper();

		internal struct HistoryData : IHistoryData
		{
			public Vector3 CameraPosition;

			public void Update(Camera camera)
			{
				CameraPosition = camera.transform.position;
			}
		}

		internal static HistoryData History = new HistoryData()
		{
			CameraPosition = Vector3.zero
		};

		internal static void Execute(CommandBuffer cmd, Camera camera, int cameraWidth, int cameraHeight)
		{
			// Raytrace
			cmd.SetRayTracingShaderPass(HRayTracing, "GBufferDXR");
			cmd.SetRayTracingTextureParam(HRayTracing, "_RayDirection", VoxelVisualizationRayDirections.rt);
			//cmd.SetRayTracingTextureParam(HRayTracing, "_Output", TestBuffer.rt);
			cmd.SetRayTracingTextureParam(HRayTracing, "_Output", RayTracedGBufferPayload.rt);
			cmd.DispatchRays(HRayTracing, "TraceAmbientOcclusion", (uint)Mathf.CeilToInt(cameraWidth), (uint)Mathf.CeilToInt(cameraHeight), (uint)HRenderer.TextureXrSlices);

			cmd.SetGlobalBuffer("_PrevBuffer", PrevBuffer);
			
			using (new HTraceProfilingScope(cmd, s_LightEvaluationProfilingSampler))
			{
				int test_kernel = TestCompute.FindKernel("TestKernel");
				cmd.SetComputeTextureParam(TestCompute, test_kernel, "_Output",                  TestBuffer.rt);
				cmd.SetComputeTextureParam(TestCompute, test_kernel, "_RayTracedGBufferPayload", RayTracedGBufferPayload.rt);
				cmd.SetComputeTextureParam(TestCompute, test_kernel, "_Output",                  TestBuffer.rt);
				cmd.SetComputeVectorParam(TestCompute, "_PreviousCameraPosition", History.CameraPosition);
				cmd.DispatchCompute(TestCompute, test_kernel, Mathf.CeilToInt(cameraWidth / 8.0f), Mathf.CeilToInt(cameraHeight / 8.0f), HRenderer.TextureXrSlices);

			}
			
			int copy_test_kernel = TestCompute.FindKernel("CopyTestKernel");
			cmd.DispatchCompute(TestCompute, copy_test_kernel, 1,1,1);

			cmd.SetGlobalTexture(HShaderParams.g_HTraceBufferGI, TestBuffer.rt);
		}
	}
}
