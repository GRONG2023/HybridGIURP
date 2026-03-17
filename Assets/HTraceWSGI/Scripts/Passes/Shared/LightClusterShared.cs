//pipelinedefine
#define H_HDRP

using System.Linq;
using HTraceWSGI.Scripts.Data.Private;
using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Services.LightsCluster;
using HTraceWSGI.Scripts.Wrappers;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace HTraceWSGI.Scripts.Passes.Shared
{
	internal static class LightClusterShared
	{
		private enum LightClusterKernels
		{
			ClearBuffer = 0,
			TransferPreviousPosition = 1,
			LightDataCompaction = 2,
			FillLightCluster = 3,
			DebugLightCluster = 4,
			FillLightClusterDebugBuffer = 5,
		}

		// Properties
		private static readonly int _LightClusterDebug_Output = Shader.PropertyToID("_LightClusterDebug_Output");

		private static readonly int _SceneLightsBuffer         = Shader.PropertyToID("_SceneLightsBuffer");
		private static readonly int _LightDatasCompactedBuffer = Shader.PropertyToID("_LightDatasCompactedBuffer");
		private static readonly int _LightClusterIndexesBuffer = Shader.PropertyToID("_LightClusterIndexesBuffer");
		private static readonly int _LightClusterCounterBuffer = Shader.PropertyToID("_LightClusterCounterBuffer");
		private static readonly int _LightClusterDebugBuffer   = Shader.PropertyToID("_LightClusterDebugBuffer");

		private static readonly int g_HeatmapDebug            = Shader.PropertyToID("g_HeatmapDebug");
		private static readonly int g_MaxLightsPerCell        = Shader.PropertyToID("g_MaxLightsPerCell");
		private static readonly int g_SceneLightsBufferSize   = Shader.PropertyToID("g_SceneLightsBufferSize");
		private static readonly int g_LightClusterDimensions  = Shader.PropertyToID("g_LightClusterDimensions");
		private static readonly int g_MinLightClusterPosition = Shader.PropertyToID("g_MinLightClusterPosition");
		private static readonly int g_MaxLightClusterPosition = Shader.PropertyToID("g_MaxLightClusterPosition");
		private static readonly int g_LightCluterCellSize     = Shader.PropertyToID("g_LightCluterCellSize");

		private static readonly int _LightClusterDebugColor = Shader.PropertyToID("_LightClusterDebugColor");
		private static readonly int _LightClusterDebugDepth = Shader.PropertyToID("_LightClusterDebugDepth");
		
		// Profiler Samplers
		private static readonly ProfilingSamplerHTrace LightClusterBuildProfilingSampler = new ProfilingSamplerHTrace("Light Cluster Build", parentName: HNames.HTRACE_FINAL_PASS_NAME, priority: 0);
		private static readonly ProfilingSamplerHTrace DebugLightClusterProfilingSampler  = new ProfilingSamplerHTrace("Debug Light Cluster",  parentName: HNames.HTRACE_FINAL_PASS_NAME, priority: 0);

		// Buffers
		internal static ComputeShader HLightCluster = null;

		internal static ComputeBuffer PrevBuffer                = null; // TODO: delete if not needed
		internal static  ComputeBuffer SceneLightsBuffer         = null; // Scene lights with HTrace punctual light script
		internal static  ComputeBuffer LightClusterDebugBuffer   = null; // Marked edge cells of light cluster (debug purposes only)
		internal static  ComputeBuffer LightClusterIndexesBuffer = null; // Indirection buffer used to index into _LightDatas
		internal static  ComputeBuffer LightClusterCounterBuffer = null; // Light count for each cell in light cluster
		internal static  ComputeBuffer LightDatasCompactedBuffer = null; // Lights from unity's _LightDatas that match lights in SceneLightsBuffer
		
		// Materials
		internal static Material LightClusterVisualizationMaterial = null;

		// RTHandles
		internal static RTWrapper LightClusterDebugColor = new RTWrapper();
		internal static RTWrapper LightClusterDebugDepth = new RTWrapper();


		internal struct HistoryData : IHistoryData
		{
			public void Update()
			{
			}
		}

		internal static HistoryData History = new HistoryData();
		
		internal static void Execute(CommandBuffer cmd, Camera camera, int cameraWidth, int cameraHeight)
		{
			//using (new HTraceProfilingScope(cmd, LightClusterBuildProfilingSampler))
			//{
			//	var filteredLights = LightsService.Instance.GetFilteredLights(camera).ToArray();
			//	SceneLightsBuffer.SetData(filteredLights);

			//	Vector3 lightClusterRangeVector3 = new Vector3(HSettings.LightingSettings.LightClusterRange, HSettings.LightingSettings.LightClusterRange, HSettings.LightingSettings.LightClusterRange);
			//	Vector3 lightClusterCellSizeSW = HSettings.VoxelizationSettings.ExactData.Bounds / HSettings.LightingSettings.LightClusterCellDensity;
				
			//	Vector3 lightClusterCellSizeHW = lightClusterRangeVector3 * 2 / HSettings.LightingSettings.LightClusterCellDensity;
			//	Vector3 lightClusterCellSize = HSettings.GeneralSettings.TracingMode == Globals.TracingMode.SoftwareTracing ? lightClusterCellSizeSW : lightClusterCellSizeHW;
				
			//	Vector3 lighClusterCenter = Vector3.zero;
			//	switch (HSettings.GeneralSettings.TracingMode)
			//	{
			//		case Globals.TracingMode.SoftwareTracing:
			//			lighClusterCenter = new Vector3(
			//				Mathf.Round(VoxelizationRuntimeData.VoxelCamera.transform.position.x / lightClusterCellSize.x) * lightClusterCellSize.x,
			//				Mathf.Round(VoxelizationRuntimeData.VoxelCamera.transform.position.y / lightClusterCellSize.y) * lightClusterCellSize.y,
			//				Mathf.Round(VoxelizationRuntimeData.VoxelCamera.transform.position.z / lightClusterCellSize.z) * lightClusterCellSize.z);
			//			break;
			//		case Globals.TracingMode.HardwareTracing:
			//			lighClusterCenter = new Vector3(
			//				Mathf.Round(camera.transform.position.x / lightClusterCellSize.x) * lightClusterCellSize.x,
			//				Mathf.Round(camera.transform.position.y / lightClusterCellSize.y) * lightClusterCellSize.y,
			//				Mathf.Round(camera.transform.position.z / lightClusterCellSize.z) * lightClusterCellSize.z);
			//			break;
			//	}

			//	Bounds  softwareLightClusterBounds = new Bounds(lighClusterCenter, HSettings.VoxelizationSettings.ExactData.Bounds);
			//	Bounds  hardwareLightClusterBounds = new Bounds(lighClusterCenter, lightClusterCellSizeHW * HSettings.LightingSettings.LightClusterCellDensity);
			//	Vector3 lightClusterBoundMin = HSettings.GeneralSettings.TracingMode == Globals.TracingMode.SoftwareTracing ? softwareLightClusterBounds.min : hardwareLightClusterBounds.min;
			//	Vector3 lightClusterBoundMax = HSettings.GeneralSettings.TracingMode == Globals.TracingMode.SoftwareTracing ? softwareLightClusterBounds.max : hardwareLightClusterBounds.max;
				
			//	// Set buffers
			//	cmd.SetGlobalBuffer(_LightClusterDebugBuffer, LightClusterDebugBuffer);
			//	cmd.SetGlobalBuffer(_LightDatasCompactedBuffer, LightDatasCompactedBuffer);
			//	cmd.SetGlobalBuffer(_LightClusterIndexesBuffer, LightClusterIndexesBuffer);
			//	cmd.SetGlobalBuffer(_LightClusterCounterBuffer, LightClusterCounterBuffer);
				
			//	// Set parameters
			//	cmd.SetGlobalVector(g_LightClusterDimensions, new Vector3(HSettings.LightingSettings.LightClusterCellDensity, HSettings.LightingSettings.LightClusterCellDensity, HSettings.LightingSettings.LightClusterCellDensity));
			//	cmd.SetGlobalVector(g_MinLightClusterPosition, lightClusterBoundMin);
			//	cmd.SetGlobalVector(g_MaxLightClusterPosition, lightClusterBoundMax);
			//	cmd.SetGlobalVector(g_LightCluterCellSize, lightClusterCellSize);
			//	cmd.SetGlobalInt(g_SceneLightsBufferSize, filteredLights.Length);
			//	cmd.SetGlobalInt(g_HeatmapDebug, HSettings.GeneralSettings.DebugModeWS == DebugModeWS.LightClusterHeatmap ? 1 : 0);
			//	cmd.SetGlobalInt(g_MaxLightsPerCell, HSettings.LightingSettings.LightClusterCellLightCount);
			//	// Clear Counter buffer
			//	cmd.DispatchCompute(HLightCluster, (int)LightClusterKernels.ClearBuffer, Mathf.CeilToInt(64 * 64 * 64 / 64), 1, 1);
				
			//	// Compact Unity's _LightData through our Scene Light Buffer
			//	cmd.SetComputeBufferParam(HLightCluster, (int)LightClusterKernels.LightDataCompaction, _SceneLightsBuffer, SceneLightsBuffer);
			//	cmd.DispatchCompute(HLightCluster, (int)LightClusterKernels.LightDataCompaction,  HRenderer.HdrpAsset.currentPlatformRenderPipelineSettings.lightLoopSettings.maxPunctualLightsOnScreen / 64, 1, 1);
				
			//	// Fill Light Cluster
			//	cmd.DispatchCompute(HLightCluster, (int)LightClusterKernels.FillLightCluster, HSettings.LightingSettings.LightClusterCellDensity / 4, HSettings.LightingSettings.LightClusterCellDensity / 4, HSettings.LightingSettings.LightClusterCellDensity / 4);
			//}
		}
			
		internal static void DebugLightCluster(CommandBuffer cmd, Camera camera, int cameraWidth, int cameraHeight)
		{
			using (new HTraceProfilingScope(cmd, DebugLightClusterProfilingSampler))
			{
				if (HSettings.LightingSettings.EvaluatePunctualLights && (HSettings.GeneralSettings.DebugModeWS == DebugModeWS.LightClusterColor || HSettings.GeneralSettings.DebugModeWS == DebugModeWS.LightClusterHeatmap))
				{
					// Draw 2D debug light cluster view on visible surfaces
					cmd.SetComputeTextureParam(HLightCluster, (int)LightClusterKernels.DebugLightCluster, _LightClusterDebug_Output, LightClusterDebugColor.rt);
					cmd.DispatchCompute(HLightCluster, (int)LightClusterKernels.DebugLightCluster, Mathf.CeilToInt(cameraWidth / 8.0f), Mathf.CeilToInt(cameraHeight / 8.0f), HRenderer.TextureXrSlices);

					if (HSettings.GeneralSettings.VolumetricDebug)
					{
						// Find all edge cells relative to each light's radius
						cmd.DispatchCompute(HLightCluster, (int)LightClusterKernels.FillLightClusterDebugBuffer, HSettings.LightingSettings.LightClusterCellDensity / 4, HSettings.LightingSettings.LightClusterCellDensity / 4, HSettings.LightingSettings.LightClusterCellDensity / 4);
					
						LightClusterVisualizationMaterial.SetTexture(_LightClusterDebugColor, LightClusterDebugColor.rt);
						LightClusterVisualizationMaterial.SetTexture(_LightClusterDebugDepth, LightClusterDebugDepth.rt);
					
						// Set our own Color and Depth render targets
						CoreUtils.SetRenderTarget(cmd, LightClusterDebugColor.rt, LightClusterDebugDepth.rt, ClearFlag.Depth, Color.clear, 0, CubemapFace.Unknown, -1);
					
						// Draw cubes first to Color and then to Depth
						cmd.DrawProcedural(Matrix4x4.identity, LightClusterVisualizationMaterial, 0, MeshTopology.Triangles, 36, HSettings.LightingSettings.LightClusterCellDensity * HSettings.LightingSettings.LightClusterCellDensity * HSettings.LightingSettings.LightClusterCellDensity);
						cmd.DrawProcedural(Matrix4x4.identity, LightClusterVisualizationMaterial, 1, MeshTopology.Triangles, 36, HSettings.LightingSettings.LightClusterCellDensity * HSettings.LightingSettings.LightClusterCellDensity * HSettings.LightingSettings.LightClusterCellDensity);
					
						// Set only Color
						CoreUtils.SetRenderTarget(cmd, LightClusterDebugColor.rt, ClearFlag.None);
					
						// Draw lines testing against the Depth filled by Cubes
						cmd.DrawProcedural(Matrix4x4.identity, LightClusterVisualizationMaterial, 2, MeshTopology.Lines, 48, HSettings.LightingSettings.LightClusterCellDensity * HSettings.LightingSettings.LightClusterCellDensity * HSettings.LightingSettings.LightClusterCellDensity);
					}
				}
			}

			cmd.SetGlobalTexture(HShaderParams.g_LightClusterDebug, LightClusterDebugColor.rt);
		}
	}
}
