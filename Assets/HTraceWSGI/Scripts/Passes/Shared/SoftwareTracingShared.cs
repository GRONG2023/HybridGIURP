//pipelinedefine
#define H_HDRP

using System;
using HTraceWSGI.Scripts.Data.Private;
using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Wrappers;
using UnityEngine;
using UnityEngine.Rendering;

namespace HTraceWSGI.Scripts.Passes.Shared
{
	internal static class SoftwareTracingShared
	{
		#region ---------------------------------- KERNELS ----------------------------------
		
		private enum HRenderAOKernel
		{
			HorizonTracing = 0,
			OcclusionInterpolation = 1,
			OcclusionAccumulation = 2,
		}

		private enum HSpatialPrepassKernel
		{
			SpatialPrepass = 0,
			GeometryNormals = 1,
			GBufferDownsample = 2,
			PointDistributionFill = 3,
			SpatialOffsetsBufferFill = 4,
			GeometryNormalsSmoothing = 5,
		}

		private enum HTemporalReprojectionKernel
		{
			ProbeReprojection = 0,
			HistoryIndirectionScroll = 1,
			HistoryIndirectionUpdate = 2,
			HistoryProbeBuffersUpdate = 3,
			CopyHistory = 4,
		}

		private enum HRayGenerationKernel
		{
			RayGeneration = 0,
			RayCompaction = 1,
			IndirectArguments = 2,
		}

		private enum HTracingScreenSpaceKernel
		{
			LightEvaluation = 0,
			ScreenSpaceTracing = 1,
		}

		private enum HTracingWorldSpaceKernel
		{
			WorldSpaceTracing = 0,
			LightEvaluation = 1,
		}

		private enum HRadianceCacheKernel
		{
			CacheDataUpdate = 0,
			CachePrimarySpawn = 1,
			CacheTracingUpdate = 2,
			CacheLightEvaluation = 3,
			CacheDataClear = 4,
		}

		private enum HProbeAmbientOcclusionKernel
		{
			ProbeAmbientOcclusion = 0,
			ProbeAmbientOcclusionSpatialFilter = 1,
			ProbeAmbientOcclusionHistoryUpdate = 2,
		}
		
		private enum HReSTIRKernel
		{
			ProbeAtlasTemporalReuse = 0,
			ProbeAtlasSpatialReuse = 1,
			ProbeAtlasSpatialReuseDisocclusion = 2,
			ReservoirHistoryUpdate = 3,
		}
		
		private enum HReservoirValidationKernel
		{
			OcclusionValidation = 0,
			OcclusionReprojection = 1,
			OcclusionSpatialFilter = 2,
			OcclusionTemporalFilter = 3,
		}

		private enum HInterpolationKernel
		{
			GatherSH = 0,
			Interpolation = 1,
		}

		private enum HTemporalDenoiserKernel
		{
			TemporalDenoising = 0,
			SpatialCleanup = 1,
		}

		private enum HCopyKernel
		{
			CopyProbeAtlases = 0,
			CopyProbeBuffers = 1,
			CopyFullResBuffers = 2,
		}

		private enum HDebugPassthroughKernel
		{
			DebugPassthrough = 0,
		}

		#endregion ---------------------------------- KERNELS ----------------------------------
		
		internal static ComputeShader HReservoirValidation    = null;
		internal static ComputeShader HTracingScreenSpace     = null;
		internal static ComputeShader HTracingWorldSpace      = null;
		internal static ComputeShader HRadianceCache          = null;
		internal static ComputeShader HTemporalReprojection   = null;
		internal static ComputeShader HReSTIR                 = null;
		internal static ComputeShader HCopy                   = null;
		internal static ComputeShader HSpatialPrepass         = null;
		internal static ComputeShader HProbeAmbientOcclusion  = null;
		internal static ComputeShader HProbeAtlasAccumulation = null;
		internal static ComputeShader HRayGeneration          = null;
		internal static ComputeShader HRenderAO               = null;
		internal static ComputeShader HPrefilterTemporal      = null;
		internal static ComputeShader HPrefilterSpatial       = null;
		internal static ComputeShader HDebugPassthrough       = null;
		internal static ComputeShader HInterpolation          = null;
		internal static ComputeShader HTemporalDenoiser       = null;
		internal static ComputeShader TestCompute             = null;
		
		// Indirection dispatch buffers
		internal static ComputeBuffer  RayCounter;
		internal static ComputeBuffer  RayCounterWS;
		internal static ComputeBuffer  IndirectArgumentsSS;
		internal static ComputeBuffer  IndirectArgumentsWS;
		internal static ComputeBuffer  IndirectArgumentsOV;
		internal static ComputeBuffer  IndirectArgumentsSF;
		internal static HDynamicBuffer IndirectCoordsSS;
		internal static HDynamicBuffer IndirectCoordsWS;
		internal static HDynamicBuffer IndirectCoordsOV;
		internal static HDynamicBuffer IndirectCoordsSF;
		
		// Spatial offsets buffers
		internal static ComputeBuffer PointDistributionBuffer;
		internal static ComputeBuffer SpatialOffsetsBuffer;
        
		// Hash buffers
		internal static ComputeBuffer HashBuffer_Key;
		internal static ComputeBuffer HashBuffer_Payload;
		internal static ComputeBuffer HashBuffer_Counter;
		internal static ComputeBuffer HashBuffer_Radiance;
		internal static ComputeBuffer HashBuffer_Position;
		
		// Materials
		internal static Material ColorCompose_BIRP;
		
		#region RT HADNLES ------------------------------------>
		
		internal static RTWrapper ColorPreviousFrame            = new RTWrapper();
		
		// SSAO RT
		internal static RTWrapper ProbeSSAO                        = new RTWrapper();
		internal static RTWrapper NormalDepthHalf                  = new RTWrapper();
		internal static RTWrapper BentNormalsAO                    = new RTWrapper();
		internal static RTWrapper BentNormalsAO_Interpolated       = new RTWrapper();
		internal static RTWrapper BentNormalsAO_History            = new RTWrapper();
		internal static RTWrapper BentNormalsAO_Accumulated        = new RTWrapper();
		internal static RTWrapper BentNormalsAO_Samplecount        = new RTWrapper();
		internal static RTWrapper BentNormalsAO_SamplecountHistory = new RTWrapper();

		// TRACING RT
		internal static RTWrapper VoxelPayload           = new RTWrapper();
		internal static RTWrapper RayDirections          = new RTWrapper();
		internal static RTWrapper HitRadiance            = new RTWrapper();
		internal static RTWrapper HitDistanceScreenSpace = new RTWrapper();
		internal static RTWrapper HitDistanceWorldSpace  = new RTWrapper();
		internal static RTWrapper HitCoordScreenSpace    = new RTWrapper();

		// PROBE AO RT
		internal static RTWrapper ProbeAmbientOcclusion          = new RTWrapper();
		internal static RTWrapper ProbeAmbientOcclusion_History  = new RTWrapper();
		internal static RTWrapper ProbeAmbientOcclusion_Filtered = new RTWrapper();

		// GBUFFER RT
		internal static RTWrapper GeometryNormal                = new RTWrapper();
		internal static RTWrapper NormalDepth_History           = new RTWrapper();
		internal static RTWrapper ProbeNormalDepth              = new RTWrapper();
		internal static RTWrapper ProbeNormalDepth_History      = new RTWrapper();
		internal static RTWrapper ProbeWorldPosNormal_History   = new RTWrapper();
		internal static RTWrapper ProbeNormalDepth_Intermediate = new RTWrapper();
		internal static RTWrapper ProbeDiffuse                  = new RTWrapper();

		// REPROJECTION RT
		internal static RTWrapper HistoryIndirection            = new RTWrapper();
		internal static RTWrapper ReprojectionCoord             = new RTWrapper();
		internal static RTWrapper PersistentReprojectionCoord   = new RTWrapper();
		internal static RTWrapper ReprojectionWeights           = new RTWrapper();
		internal static RTWrapper PersistentReprojectionWeights = new RTWrapper();

		// SPATIAL PREPASS RT
		internal static RTWrapper SpatialOffsetsPacked = new RTWrapper();
		internal static RTWrapper SpatialWeightsPacked = new RTWrapper();

		// RESERVOIR RT
		internal static RTWrapper ReservoirAtlas               = new RTWrapper();
		internal static RTWrapper ReservoirAtlas_History       = new RTWrapper();
		internal static RTWrapper ReservoirAtlasRadianceData_A = new RTWrapper();
		internal static RTWrapper ReservoirAtlasRadianceData_B = new RTWrapper();
		internal static RTWrapper ReservoirAtlasRadianceData_C = new RTWrapper();
		internal static RTWrapper ReservoirAtlasRayData_A      = new RTWrapper();
		internal static RTWrapper ReservoirAtlasRayData_B      = new RTWrapper();
		internal static RTWrapper ReservoirAtlasRayData_C      = new RTWrapper();

		// SHADOW GUIDANCE MASK RT
		internal static RTWrapper ShadowGuidanceMask                     = new RTWrapper();
		internal static RTWrapper ShadowGuidanceMask_Accumulated         = new RTWrapper();
		internal static RTWrapper ShadowGuidanceMask_Filtered            = new RTWrapper();
		internal static RTWrapper ShadowGuidanceMask_History             = new RTWrapper();
		internal static RTWrapper ShadowGuidanceMask_CheckerboardHistory = new RTWrapper();
		internal static RTWrapper ShadowGuidanceMask_Samplecount         = new RTWrapper();
		internal static RTWrapper ShadowGuidanceMask_SamplecountHistory  = new RTWrapper();

		// INTERPOLATION RT
		internal static RTWrapper PackedSH_A            = new RTWrapper();
		internal static RTWrapper PackedSH_B            = new RTWrapper();
		internal static RTWrapper Radiance_Interpolated = new RTWrapper();

		// TEMPORAL DENOISER RT
		internal static RTWrapper RadianceAccumulated         = new RTWrapper();
		internal static RTWrapper RadianceAccumulated_History = new RTWrapper();
		internal static RTWrapper LuminanceDelta              = new RTWrapper();
		internal static RTWrapper LuminanceDelta_History      = new RTWrapper();

		// DEBUG RT
		internal static RTWrapper DebugOutput                     = new RTWrapper();

		internal static RTWrapper RadianceCacheFiltered = new RTWrapper();

		#endregion
		
		internal static readonly ProfilingSamplerHTrace s_RenderAmbientOcclsuionProfilingSampler       = new ProfilingSamplerHTrace("Render Ambient Occlsuion",       parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 2);
		internal static readonly ProfilingSamplerHTrace s_ProbeGBufferDownsamplingProfilingSampler     = new ProfilingSamplerHTrace("Probe GBuffer Downsampling",     parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 3);
		internal static readonly ProfilingSamplerHTrace s_ProbeTemporalReprojectionProfilingSampler    = new ProfilingSamplerHTrace("Probe Temporal Reprojection",    parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 4);
		internal static readonly ProfilingSamplerHTrace s_RayGenerationProfilingSampler                = new ProfilingSamplerHTrace("Ray Generation",                 parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 5);
		internal static readonly ProfilingSamplerHTrace s_ClearTargetsProfilingSampler                 = new ProfilingSamplerHTrace("Clear Targets",                  parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 6);
		internal static readonly ProfilingSamplerHTrace s_ScreenSpaceLightingProfilingSampler          = new ProfilingSamplerHTrace("Screen Space Lighting",          parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 7);
		internal static readonly ProfilingSamplerHTrace s_RayCompactionProfilingSampler                = new ProfilingSamplerHTrace("Ray Compaction",                 parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 8);
		internal static readonly ProfilingSamplerHTrace s_WorldSpaceLightingProfilingSampler           = new ProfilingSamplerHTrace("World Space Lighting",           parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 9);
		internal static readonly ProfilingSamplerHTrace s_WorldSpaceTracingProfilingSampler            = new ProfilingSamplerHTrace("World Space Tracing",            parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 10);
		internal static readonly ProfilingSamplerHTrace s_LightEvaluationProfilingSampler              = new ProfilingSamplerHTrace("Light Evaluation",               parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 11);
		internal static readonly ProfilingSamplerHTrace s_RadianceCachingProfilingSampler              = new ProfilingSamplerHTrace("Radiance Caching",               parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 12);
		internal static readonly ProfilingSamplerHTrace s_CacheTracingUpdateProfilingSampler           = new ProfilingSamplerHTrace("Cache Tracing Update",           parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 13);
		internal static readonly ProfilingSamplerHTrace s_CacheLightEvaluationProfilingSampler         = new ProfilingSamplerHTrace("Cache Light Evaluation",         parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 14);
		internal static readonly ProfilingSamplerHTrace s_PrimaryCacheSpawnProfilingSampler            = new ProfilingSamplerHTrace("Primary Cache Spawn",            parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 15);
		internal static readonly ProfilingSamplerHTrace s_CacheDataUpdateProfilingSampler              = new ProfilingSamplerHTrace("Cache Data Update",              parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 16);
		internal static readonly ProfilingSamplerHTrace s_SpatialPrepassProfilingSampler               = new ProfilingSamplerHTrace("Spatial Prepass",                parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 17);
		internal static readonly ProfilingSamplerHTrace s_ReSTIRTemporalReuseProfilingSampler          = new ProfilingSamplerHTrace("ReSTIR Temporal Reuse",          parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 18);
		internal static readonly ProfilingSamplerHTrace s_ReservoirOcclusionValidationProfilingSampler = new ProfilingSamplerHTrace("Reservoir Occlusion Validation", parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 19);
		internal static readonly ProfilingSamplerHTrace s_ReSTIRSpatialReuseProfilingSampler           = new ProfilingSamplerHTrace("ReSTIR Spatial Reuse",           parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 20);
		internal static readonly ProfilingSamplerHTrace s_PersistentHistoryUpdateProfilingSampler      = new ProfilingSamplerHTrace("Persistent History Update",      parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 20);
		internal static readonly ProfilingSamplerHTrace s_InterpolationProfilingSampler                = new ProfilingSamplerHTrace("Interpolation",                  parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 0);
		internal static readonly ProfilingSamplerHTrace s_TemporalDenoisingProfilingSampler            = new ProfilingSamplerHTrace("Temporal Denoising",             parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 0);
		internal static readonly ProfilingSamplerHTrace s_SpatialCleanupProfilingSampler               = new ProfilingSamplerHTrace("Spatial Cleanup",                parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 0);
		internal static readonly ProfilingSamplerHTrace s_CopyBuffersProfilingSampler                  = new ProfilingSamplerHTrace("Copy Buffers",                   parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 0);
		internal static readonly ProfilingSamplerHTrace s_DebugPassthroughProfilingSampler             = new ProfilingSamplerHTrace("Debug Passthrough",              parentName: HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME, priority: 0);

		private const string MULTIBOUNCE_CACHE = "MULTIBOUNCE_CACHE";
		private const string MULTIBOUNCE_OFF = "MULTIBOUNCE_OFF";
		private const string MULTIBOUNCE_APV = "MULTIBOUNCE_APV";
		private const string GI_TRACING_IN_REFLECTIONS = "GI_TRACING_IN_REFLECTIONS";
		private const string GI_APPROXIMATION_IN_REFLECTIONS = "GI_APPROXIMATION_IN_REFLECTIONS";
		private const string GI_APPROXIMATION_IN_REFLECTIONS_OCCLUSION_CHECK = "GI_APPROXIMATION_IN_REFLECTIONS_OCCLUSION_CHECK";
		private const string USE_DIRECTIONAL_OCCLUSION = "USE_DIRECTIONAL_OCCLUSION";
		private const string DIFFUSE_BUFFER_UNAVAILABLE = "DIFFUSE_BUFFER_UNAVAILABLE";
		private const string HIT_SCREEN_SPACE_LIGHTING = "HIT_SCREEN_SPACE_LIGHTING";
		private const string EVALUATE_PUNCTUAL_LIGHTS = "EVALUATE_PUNCTUAL_LIGHTS";

		private static int _hFrameIndex = 0;
		private static int _hashUpdateFrameIndex = 0;
		private static int _startFrameCounter = 0;

		internal static bool SkipFirstFrame = true;
		internal static bool ClearRadianceCache = false;

		internal static bool UseDirectionalOcclusion => HSettings.ScreenSpaceLightingSettings.DirectionalOcclusion && HSettings.ScreenSpaceLightingSettings.OcclusionIntensity > Single.Epsilon;

        internal struct HistoryData : IHistoryData
        {
	        public bool DebugModeEnabled;
	        public bool DirectionalOcclusion;
	        public RayCountMode RayCountMode;
	        public TracingMode TracingMode;

	        public void Update()
	        {
		        DebugModeEnabled = HSettings.GeneralSettings.DebugModeWS != DebugModeWS.None;
		        DirectionalOcclusion  = UseDirectionalOcclusion;
		        RayCountMode          = HSettings.GeneralSettings.RayCountMode;
		        TracingMode           = HSettings.GeneralSettings.TracingMode;
	        }
        }

        internal static HistoryData History = new HistoryData();

        static bool hasBindDitheredBlueNoiseTexture = false;
        static int HFrameCount = 0;
        static Cubemap skyCubemap = null;

        internal static void Execute(CommandBuffer cmd, Camera camera, int cameraWidth, int cameraHeight, Texture cameraColorBuffer = null, Texture previousColorBuffer = null, Texture diffuseBuffer = null)
		{

            HFrameCount++;
            cmd.SetGlobalInt(HShaderParams.g_TestCheckbox, HSettings.DebugSettings.TestCheckbox ? 1 : 0);
            cmd.SetGlobalInt(HShaderParams.HFrameCount, HFrameCount);
            
            _hFrameIndex = _hFrameIndex > 15 ? 0 : _hFrameIndex;
            _hashUpdateFrameIndex = _hashUpdateFrameIndex > HConstants.HASH_UPDATE_FRACTION ? 0 : _hashUpdateFrameIndex;
            
            cmd.SetGlobalInt(HShaderParams.g_ProbeSize,                HSettings.GeneralSettings.RayCountMode.ParseToProbeSize());
            cmd.SetGlobalInt(HShaderParams.g_OctahedralSize,           HConstants.OCTAHEDRAL_SIZE);
            cmd.SetGlobalInt(HShaderParams.g_HFrameIndex,              _hFrameIndex);
            cmd.SetGlobalInt(HShaderParams.g_ReprojectSkippedFrame,    Time.frameCount % 8 == 0 ? 1 : 0);
            cmd.SetGlobalInt(HShaderParams.g_PersistentHistorySamples, HConstants.PERSISTENT_HISTORY_SAMPLES);
            
            // Constants set
            cmd.SetGlobalFloat(HShaderParams.g_SkyOcclusionCone,          HConfig.SkyOcclusionCone);
            cmd.SetGlobalFloat(HShaderParams.g_DirectionalLightIntensity, HConfig.DirectionalLightIntensity);
            cmd.SetGlobalFloat(HShaderParams.g_SurfaceDiffuseIntensity,   HConfig.SurfaceDiffuseIntensity);
            cmd.SetGlobalFloat(HShaderParams.g_SkyLightIntensity,         HConfig.SkyLightIntensity);

            if (skyCubemap == null)
            {
                skyCubemap = UnityEngine.Resources.Load<Cubemap>("HTraceWSGI/skyCubemap");
            }

            cmd.SetGlobalTexture(HShaderParams.unity_SpecCube2, skyCubemap);

            if (!hasBindDitheredBlueNoiseTexture)
            {

                hasBindDitheredBlueNoiseTexture = true;

                cmd.SetGlobalTexture(HShaderParams._OwenScrambledTexture, HBlueNoiseShared.OwenScrambledTexture);
                cmd.SetGlobalTexture(HShaderParams._ScramblingTileXSPP, HBlueNoiseShared.ScramblingTileXSPP);
                cmd.SetGlobalTexture(HShaderParams._RankingTileXSPP, HBlueNoiseShared.RankingTileXSPP);
                cmd.SetGlobalTexture(HShaderParams._ScramblingTexture, HBlueNoiseShared.ScramblingTexture);

                Debug.Log("BindDitheredTexture BlueNoise");

            }

            //cmdList.SetGlobalInt("_TestCheckbox", DebugData.TestCheckbox == true ? 1 : 0);

            if (HSettings.LightingSettings.EvaluatePunctualLights) Shader.EnableKeyword(EVALUATE_PUNCTUAL_LIGHTS);
            else Shader.DisableKeyword(EVALUATE_PUNCTUAL_LIGHTS);
            
            cmd.SetGlobalInt(HShaderParams.g_ReprojectSkippedFrame, 0);
             
            int screenResX = cameraWidth;
            int screenResY = cameraHeight;

            // Calculate Resolution for Compute Shaders
            Vector2Int runningRes = new Vector2Int(screenResX, screenResY);
            Vector2 probeSize = runningRes * Vector2.one / HSettings.GeneralSettings.RayCountMode.ParseToProbeSize();
            Vector2 probeAtlasRes = probeSize * HConstants.OCTAHEDRAL_SIZE;
            //Debug.Log("probeSize = "+ probeSize + "  probeAtlasRes = " + probeAtlasRes);
            //Dispatch resolutions
            int fullResX_8       = Mathf.CeilToInt((float)runningRes.x / 8);
            int fullResY_8       = Mathf.CeilToInt((float)runningRes.y / 8);
            int probeResX_8      = Mathf.CeilToInt(((float)runningRes.x / (float)HSettings.GeneralSettings.RayCountMode.ParseToProbeSize() / 8.0f));
            int probeResY_8      = Mathf.CeilToInt(((float)runningRes.y / (float)HSettings.GeneralSettings.RayCountMode.ParseToProbeSize() / 8.0f));
            int probeAtlasResX_8 = Mathf.CeilToInt((Mathf.CeilToInt((float)runningRes.x / (float)HSettings.GeneralSettings.RayCountMode.ParseToProbeSize()) * (float)HConstants.OCTAHEDRAL_SIZE) / 8);
            int probeAtlasResY_8 = Mathf.CeilToInt((Mathf.CeilToInt((float)runningRes.y / (float)HSettings.GeneralSettings.RayCountMode.ParseToProbeSize()) * (float)HConstants.OCTAHEDRAL_SIZE) / 8);
            //Debug.Log("probeResX_8 = " + probeResX_8 + "  probeResY_8 = " + probeResY_8);
            bool diffuseBufferUnavailable = diffuseBuffer == null;
            
            cmd.SetGlobalBuffer(HShaderParams.g_HashBuffer_Key,      HashBuffer_Key);
            cmd.SetGlobalBuffer(HShaderParams.g_HashBuffer_Payload,  HashBuffer_Payload);
            cmd.SetGlobalBuffer(HShaderParams.g_HashBuffer_Counter,  HashBuffer_Counter);
            cmd.SetGlobalBuffer(HShaderParams.g_HashBuffer_Radiance, HashBuffer_Radiance);
            cmd.SetGlobalBuffer(HShaderParams.g_HashBuffer_Position, HashBuffer_Position);
            
            cmd.SetGlobalInt(HShaderParams.g_HashStorageSize,    HConstants.HASH_STORAGE_SIZE);
            cmd.SetGlobalInt(HShaderParams.g_HashUpdateFraction, HConstants.HASH_UPDATE_FRACTION);
            
            cmd.SetGlobalTexture(HShaderParams.g_HTraceBufferGI, RadianceAccumulated.rt);
            
            if (camera.cameraType == CameraType.Reflection)
                return;
            
            if (_startFrameCounter < 2) { _startFrameCounter++; return; }
            
            
            // ---------------------------------------- RENDER AMBIENT OCCLUSION ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_RenderAmbientOcclsuionProfilingSampler))
            {
                int computeResX_GI = (runningRes.x / 2  + 8 - 1) / 8;
                int computeResY_GI = (runningRes.y / 2  + 8 - 1) / 8;
                
                if (UseDirectionalOcclusion)
                {
                    HInterpolation.EnableKeyword(USE_DIRECTIONAL_OCCLUSION);
                    HSpatialPrepass.EnableKeyword(USE_DIRECTIONAL_OCCLUSION);

                    cmd.SetComputeTextureParam(HRenderAO, (int)HRenderAOKernel.HorizonTracing, HShaderParams._BentNormalAmbientOcclusion_Output, BentNormalsAO.rt);
                    cmd.SetComputeTextureParam(HRenderAO, (int)HRenderAOKernel.HorizonTracing, HShaderParams._NormalDepthHalf_Output,            NormalDepthHalf.rt);
                    cmd.SetComputeFloatParam(HRenderAO, HShaderParams._Camera_FOV, camera.fieldOfView);
                    cmd.DispatchCompute(HRenderAO, (int)HRenderAOKernel.HorizonTracing, computeResX_GI, computeResY_GI, HRenderer.TextureXrSlices);

                    cmd.SetComputeTextureParam(HRenderAO, (int)HRenderAOKernel.OcclusionInterpolation, HShaderParams._AmbientOcclusion,      BentNormalsAO.rt);
                    cmd.SetComputeTextureParam(HRenderAO, (int)HRenderAOKernel.OcclusionInterpolation, HShaderParams._NormalDepthHalf,       NormalDepthHalf.rt);
                    cmd.SetComputeTextureParam(HRenderAO, (int)HRenderAOKernel.OcclusionInterpolation, HShaderParams._BentNormalAO_Output,   BentNormalsAO_Interpolated.rt);
                    cmd.SetComputeTextureParam(HRenderAO, (int)HRenderAOKernel.OcclusionInterpolation, HShaderParams._GeometryNormal_Output, GeometryNormal.rt);
                    cmd.DispatchCompute(HRenderAO, (int)HRenderAOKernel.OcclusionInterpolation, fullResX_8, fullResY_8, HRenderer.TextureXrSlices);
                }
                else
                {
                    HInterpolation.DisableKeyword(USE_DIRECTIONAL_OCCLUSION);
                    HSpatialPrepass.DisableKeyword(USE_DIRECTIONAL_OCCLUSION);
                }
            }

            // ---------------------------------------- PROBE GBUFFER DOWNSAMPLING ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_ProbeGBufferDownsamplingProfilingSampler))
            {
                // Fill sample offsets for disk filters
                cmd.SetComputeBufferParam(HSpatialPrepass, (int)HSpatialPrepassKernel.PointDistributionFill, HShaderParams._PointDistribution_Output, PointDistributionBuffer);
                cmd.DispatchCompute(HSpatialPrepass, (int)HSpatialPrepassKernel.PointDistributionFill, 1, 1, 1);
                
                // Fill 4x4 spatial offset buffer
                cmd.SetComputeBufferParam(HSpatialPrepass, (int)HSpatialPrepassKernel.SpatialOffsetsBufferFill, HShaderParams._SpatialOffsetsBuffer_Output, SpatialOffsetsBuffer);
                cmd.DispatchCompute(HSpatialPrepass, (int)HSpatialPrepassKernel.SpatialOffsetsBufferFill, 1, 1, 1);
                
                // Calculate geometry normals 
                if (!UseDirectionalOcclusion)
                {
                    cmd.SetComputeTextureParam(HSpatialPrepass, (int)HSpatialPrepassKernel.GeometryNormals, HShaderParams._GeometryNormal_Output, GeometryNormal.rt);
                    cmd.DispatchCompute(HSpatialPrepass, (int)HSpatialPrepassKernel.GeometryNormals, fullResX_8, fullResY_8, HRenderer.TextureXrSlices);
                }
                
                if (diffuseBufferUnavailable) HSpatialPrepass.EnableKeyword(DIFFUSE_BUFFER_UNAVAILABLE);
                if (!diffuseBufferUnavailable) HSpatialPrepass.DisableKeyword(DIFFUSE_BUFFER_UNAVAILABLE);

                // Downsample depth, normal, diffuse and ambient occlusion
                cmd.SetComputeTextureParam(HSpatialPrepass, (int)HSpatialPrepassKernel.GBufferDownsample, HShaderParams._GeometryNormal,          GeometryNormal.rt);
                cmd.SetComputeTextureParam(HSpatialPrepass, (int)HSpatialPrepassKernel.GBufferDownsample, HShaderParams._ProbeNormalDepth_Output, ProbeNormalDepth_Intermediate.rt);
                cmd.SetComputeTextureParam(HSpatialPrepass, (int)HSpatialPrepassKernel.GBufferDownsample, HShaderParams._ProbeDiffuse_Output,     ProbeDiffuse.rt);
                cmd.SetComputeTextureParam(HSpatialPrepass, (int)HSpatialPrepassKernel.GBufferDownsample, HShaderParams._SSAO,                    BentNormalsAO_Interpolated.rt);
                cmd.SetComputeTextureParam(HSpatialPrepass, (int)HSpatialPrepassKernel.GBufferDownsample, HShaderParams._ProbeSSAO_Output,        ProbeSSAO.rt);
                cmd.DispatchCompute(HSpatialPrepass, (int)HSpatialPrepassKernel.GBufferDownsample, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);

                // Smooth geometry normals
                cmd.SetComputeTextureParam(HSpatialPrepass, (int)HSpatialPrepassKernel.GeometryNormalsSmoothing, HShaderParams._ProbeNormalDepth,        ProbeNormalDepth_Intermediate.rt);
                cmd.SetComputeTextureParam(HSpatialPrepass, (int)HSpatialPrepassKernel.GeometryNormalsSmoothing, HShaderParams._ProbeNormalDepth_Output, ProbeNormalDepth.rt);
                cmd.DispatchCompute(HSpatialPrepass, (int)HSpatialPrepassKernel.GeometryNormalsSmoothing, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);
            }

            // ---------------------------------------- PROBE TEMPORAL REPROJECTION ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_ProbeTemporalReprojectionProfilingSampler))
            {
                cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.ProbeReprojection, HShaderParams._HistoryIndirection,                   HistoryIndirection.rt);
                cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.ProbeReprojection, HShaderParams._ProbeNormalDepth,                     ProbeNormalDepth.rt);
                cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.ProbeReprojection, HShaderParams._ProbeWorldPosNormal_History,          ProbeWorldPosNormal_History.rt);
                cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.ProbeReprojection, HShaderParams._ReprojectionCoords_Output,            ReprojectionCoord.rt);
                cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.ProbeReprojection, HShaderParams._ReprojectionWeights_Output,           ReprojectionWeights.rt);
                cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.ProbeReprojection, HShaderParams._PersistentReprojectionWeights_Output, PersistentReprojectionWeights.rt);
                cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.ProbeReprojection, HShaderParams._PersistentReprojectionCoord_Output,   PersistentReprojectionCoord.rt);
                cmd.DispatchCompute(HTemporalReprojection, (int)HTemporalReprojectionKernel.ProbeReprojection, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);
            }


            // Update probe world position & normal history buffer
            cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.HistoryProbeBuffersUpdate, HShaderParams._ProbeNormalDepth, ProbeNormalDepth.rt);
            cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.HistoryProbeBuffersUpdate, HShaderParams._ProbeWorldPosNormal_HistoryOutput, ProbeWorldPosNormal_History.rt);
            cmd.DispatchCompute(HTemporalReprojection, (int)HTemporalReprojectionKernel.HistoryProbeBuffersUpdate, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);


            // ---------------------------------------- RAY GENERATION ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_RayGenerationProfilingSampler))
            {
                // Generate ray directions and compute lists of indirectly dispatched threads
                cmd.SetComputeTextureParam(HRayGeneration, (int)HRayGenerationKernel.RayGeneration, HShaderParams._ProbeNormalDepth,             ProbeNormalDepth.rt);
                cmd.SetComputeTextureParam(HRayGeneration, (int)HRayGenerationKernel.RayGeneration, HShaderParams._ReprojectionCoords,           ReprojectionCoord.rt);
                cmd.SetComputeTextureParam(HRayGeneration, (int)HRayGenerationKernel.RayGeneration, HShaderParams._RayDirectionsJittered_Output, RayDirections.rt);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.RayGeneration, HShaderParams._IndirectCoordsSS_Output, IndirectCoordsSS.ComputeBuffer);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.RayGeneration, HShaderParams._IndirectCoordsOV_Output, IndirectCoordsOV.ComputeBuffer);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.RayGeneration, HShaderParams._IndirectCoordsSF_Output, IndirectCoordsSF.ComputeBuffer);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.RayGeneration, HShaderParams._RayCounter_Output,       RayCounter);
                cmd.DispatchCompute(HRayGeneration, (int)HRayGenerationKernel.RayGeneration, probeAtlasResX_8, probeAtlasResY_8, HRenderer.TextureXrSlices);
                
                // Prepare arguments for screen space indirect dispatch
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, HShaderParams._RayCounter,               RayCounter);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, HShaderParams._TracingCoords,            IndirectCoordsSS.ComputeBuffer);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, HShaderParams._IndirectArguments_Output, IndirectArgumentsSS);
                cmd.SetComputeIntParam(HRayGeneration, HShaderParams._RayCounterIndex, 0);
                cmd.DispatchCompute(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, 1, 1, HRenderer.TextureXrSlices);
                
                // Prepare arguments for occlusion validation indirect dispatch
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, HShaderParams._RayCounter,               RayCounter);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, HShaderParams._TracingCoords,            IndirectCoordsOV.ComputeBuffer);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, HShaderParams._IndirectArguments_Output, IndirectArgumentsOV);
                cmd.SetComputeIntParam(HRayGeneration, HShaderParams._RayCounterIndex, 1);
                cmd.DispatchCompute(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, 1, 1, HRenderer.TextureXrSlices);
                
                // Prepare arguments for spatial filter indirect dispatch
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, HShaderParams._RayCounter,               RayCounter);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, HShaderParams._TracingCoords,            IndirectCoordsSF.ComputeBuffer);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, HShaderParams._IndirectArguments_Output, IndirectArgumentsSF);
                cmd.SetComputeIntParam(HRayGeneration, HShaderParams._RayCounterIndex, 2);
                cmd.DispatchCompute(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, 1, 1, HRenderer.TextureXrSlices);
            }

            // ---------------------------------------- CLEAR CHECKERBOARD TARGETS ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_ClearTargetsProfilingSampler))
            {
                // Clear hit targets
                CoreUtils.SetRenderTarget(cmd, HitDistanceScreenSpace.rt, ClearFlag.Color, Color.clear, 0, CubemapFace.Unknown, -1);
                CoreUtils.SetRenderTarget(cmd, HitDistanceWorldSpace.rt,  ClearFlag.Color, Color.clear, 0, CubemapFace.Unknown, -1);
                CoreUtils.SetRenderTarget(cmd, HitCoordScreenSpace.rt,    ClearFlag.Color, Color.clear, 0, CubemapFace.Unknown, -1);
                CoreUtils.SetRenderTarget(cmd, HitRadiance.rt,            ClearFlag.Color, Color.clear, 0, CubemapFace.Unknown, -1);
                
                // Clear voxel payload targets
                CoreUtils.SetRenderTarget(cmd, VoxelPayload.rt, ClearFlag.Color, Color.clear, 0, CubemapFace.Unknown, -1);
            }
            
            // ---------------------------------------- SCREEN SPACE LIGHTING ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_ScreenSpaceLightingProfilingSampler))
            {
                if (HSettings.ScreenSpaceLightingSettings.EvaluateHitLighting && !diffuseBufferUnavailable) HTracingScreenSpace.EnableKeyword(HIT_SCREEN_SPACE_LIGHTING);
                if (!HSettings.ScreenSpaceLightingSettings.EvaluateHitLighting || diffuseBufferUnavailable) HTracingScreenSpace.DisableKeyword(HIT_SCREEN_SPACE_LIGHTING);

                // Trace screen-space rays
                cmd.SetComputeTextureParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.ScreenSpaceTracing, HShaderParams._ColorPyramid_History, previousColorBuffer);
                cmd.SetComputeTextureParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.ScreenSpaceTracing, HShaderParams._ProbeNormalDepth,     ProbeNormalDepth.rt);
                cmd.SetComputeTextureParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.ScreenSpaceTracing, HShaderParams._NormalDepth_History,  NormalDepth_History.rt);
                cmd.SetComputeTextureParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.ScreenSpaceTracing, HShaderParams._GeometryNormal,       GeometryNormal.rt);
                cmd.SetComputeTextureParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.ScreenSpaceTracing, HShaderParams._RayDirection,         RayDirections.rt);
                cmd.SetComputeTextureParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.ScreenSpaceTracing, HShaderParams._HitRadiance_Output,   HitRadiance.rt);
                cmd.SetComputeTextureParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.ScreenSpaceTracing, HShaderParams._HitDistance_Output,   HitDistanceScreenSpace.rt);
                cmd.SetComputeTextureParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.ScreenSpaceTracing, HShaderParams._HitCoord_Output,      HitCoordScreenSpace.rt);
                cmd.SetComputeBufferParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.ScreenSpaceTracing, HShaderParams._RayCounter,    RayCounter);
                cmd.SetComputeBufferParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.ScreenSpaceTracing, HShaderParams._TracingCoords, IndirectCoordsSS.ComputeBuffer);
                cmd.SetComputeIntParam(HTracingScreenSpace, HShaderParams._IndexXR, 0);
                cmd.DispatchCompute(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.ScreenSpaceTracing, IndirectArgumentsSS, 0);
                
                // Evaluate screen-space hit it requested 
                if (HSettings.ScreenSpaceLightingSettings.EvaluateHitLighting && !diffuseBufferUnavailable)
                {
                    cmd.SetComputeTextureParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.LightEvaluation, HShaderParams._ColorPyramid_History, previousColorBuffer);
                    cmd.SetComputeTextureParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.LightEvaluation, HShaderParams._Radiance_History,     RadianceAccumulated.rt);
                    cmd.SetComputeTextureParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.LightEvaluation, HShaderParams._GeometryNormal,       GeometryNormal.rt);
                    cmd.SetComputeTextureParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.LightEvaluation, HShaderParams._HitCoord,             HitCoordScreenSpace.rt);
                    cmd.SetComputeTextureParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.LightEvaluation, HShaderParams._HitRadiance_Output,   HitRadiance.rt);
                    cmd.SetComputeBufferParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.LightEvaluation, HShaderParams._RayCounter,    RayCounter);
                    cmd.SetComputeBufferParam(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.LightEvaluation, HShaderParams._TracingCoords, IndirectCoordsSS.ComputeBuffer);
                    cmd.SetComputeIntParam(HTracingScreenSpace, HShaderParams._IndexXR, 0);
                    cmd.DispatchCompute(HTracingScreenSpace, (int)HTracingScreenSpaceKernel.LightEvaluation, IndirectArgumentsSS, 0);
                }
            }
            return;
            // ---------------------------------------- RAY COMPACTION ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_RayCompactionProfilingSampler))
            {
                // Compact rays
                cmd.SetComputeTextureParam(HRayGeneration, (int)HRayGenerationKernel.RayCompaction, HShaderParams._HitDistance,        HitDistanceScreenSpace.rt);
                cmd.SetComputeTextureParam(HRayGeneration, (int)HRayGenerationKernel.RayCompaction, HShaderParams._HitDistance_Output, HitDistanceWorldSpace.rt);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.RayCompaction, HShaderParams._RayCounter,               RayCounter);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.RayCompaction, HShaderParams._TracingCoords,            IndirectCoordsSS.ComputeBuffer);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.RayCompaction, HShaderParams._TracingRayCounter_Output, RayCounterWS);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.RayCompaction, HShaderParams._TracingCoords_Output,     IndirectCoordsWS.ComputeBuffer);
                cmd.SetComputeIntParam(HRayGeneration, HShaderParams._IndexXR, 0);
                cmd.DispatchCompute(HRayGeneration, (int)HRayGenerationKernel.RayCompaction, IndirectArgumentsSS, 0);
                if (HRenderer.TextureXrSlices > 1)
                {
                    cmd.SetComputeIntParam(HRayGeneration, HShaderParams._IndexXR, 1);
                    cmd.DispatchCompute(HRayGeneration, (int)HRayGenerationKernel.RayCompaction, IndirectArgumentsSS, sizeof(uint) * 3);
                }

                // Prepare indirect arguments for world space lighting
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, HShaderParams._RayCounter,               RayCounterWS);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, HShaderParams._TracingCoords,            IndirectCoordsWS.ComputeBuffer);
                cmd.SetComputeBufferParam(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, HShaderParams._IndirectArguments_Output, IndirectArgumentsWS); 
                cmd.SetComputeIntParam(HRayGeneration, HShaderParams._RayCounterIndex, 0);
                cmd.DispatchCompute(HRayGeneration, (int)HRayGenerationKernel.IndirectArguments, 1, 1, HRenderer.TextureXrSlices);
            }
            

            // TDR timeout protection
            if (SkipFirstFrame 
                || History.TracingMode != HSettings.GeneralSettings.TracingMode
                || History.RayCountMode != HSettings.GeneralSettings.RayCountMode)
            {
	            SkipFirstFrame = false;
                return;
            }
            
            // ---------------------------------------- WORLD SPACE LIGHTING ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_WorldSpaceLightingProfilingSampler))
            {
                if (HSettings.GeneralSettings.Multibounce == Multibounce.None) { HTracingWorldSpace.DisableKeyword(MULTIBOUNCE_CACHE); HTracingWorldSpace.DisableKeyword(MULTIBOUNCE_APV); HTracingWorldSpace.EnableKeyword(MULTIBOUNCE_OFF); }
                if (HSettings.GeneralSettings.Multibounce == Multibounce.IrradianceCache) { HTracingWorldSpace.DisableKeyword(MULTIBOUNCE_OFF); HTracingWorldSpace.DisableKeyword(MULTIBOUNCE_APV); HTracingWorldSpace.EnableKeyword(MULTIBOUNCE_CACHE); }
                if (HSettings.GeneralSettings.Multibounce == Multibounce.AdaptiveProbeVolumes) { HTracingWorldSpace.DisableKeyword(MULTIBOUNCE_CACHE); HTracingWorldSpace.DisableKeyword(MULTIBOUNCE_OFF); HTracingWorldSpace.EnableKeyword(MULTIBOUNCE_APV); }

                if (HSettings.LightingSettings.EvaluatePunctualLights) HTracingWorldSpace.EnableKeyword(EVALUATE_PUNCTUAL_LIGHTS);
                else HTracingWorldSpace.DisableKeyword(EVALUATE_PUNCTUAL_LIGHTS);
                
                // Trace world-space rays
                using (new HTraceProfilingScope(cmd, s_WorldSpaceTracingProfilingSampler)) 
                {
                    cmd.SetComputeTextureParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.WorldSpaceTracing, HShaderParams._HitDistance,         HitDistanceScreenSpace.rt);
                    cmd.SetComputeTextureParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.WorldSpaceTracing, HShaderParams._ProbeNormalDepth,    ProbeNormalDepth.rt);
                    cmd.SetComputeTextureParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.WorldSpaceTracing, HShaderParams._GeometryNormal,      GeometryNormal.rt);
                    cmd.SetComputeTextureParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.WorldSpaceTracing, HShaderParams._RayDirection,        RayDirections.rt);
                    cmd.SetComputeTextureParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.WorldSpaceTracing, HShaderParams._HitDistance_Output,  HitDistanceWorldSpace.rt);
                    cmd.SetComputeTextureParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.WorldSpaceTracing, HShaderParams._VoxelPayload_Output, VoxelPayload.rt);
                    cmd.SetComputeTextureParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.WorldSpaceTracing, HShaderParams._HitRadiance_Output,  HitRadiance.rt);
                    cmd.SetComputeBufferParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.WorldSpaceTracing, HShaderParams._PointDistribution, PointDistributionBuffer);
                    cmd.SetComputeBufferParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.WorldSpaceTracing, HShaderParams._TracingCoords,     IndirectCoordsWS.ComputeBuffer);
                    cmd.SetComputeBufferParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.WorldSpaceTracing, HShaderParams._RayCounter,        RayCounterWS);
                    cmd.SetComputeFloatParam(HTracingWorldSpace, HShaderParams._RayLength, HSettings.GeneralSettings.RayLength);
                    cmd.SetComputeIntParam(HTracingWorldSpace, HShaderParams._IndexXR, 0);
                    cmd.DispatchCompute(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.WorldSpaceTracing, IndirectArgumentsWS, 0);
                    if (HRenderer.TextureXrSlices > 1)
                    {
                        cmd.SetComputeIntParam(HTracingWorldSpace, HShaderParams._IndexXR, 1);
                        cmd.DispatchCompute(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.WorldSpaceTracing, IndirectArgumentsWS, sizeof(uint) * 3);
                    }
                }
                
                // Evaluate world-space lighting
                using (new HTraceProfilingScope(cmd, s_LightEvaluationProfilingSampler))
                {
                    cmd.SetComputeTextureParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.LightEvaluation, HShaderParams._VoxelPayload,       VoxelPayload.rt);
                    cmd.SetComputeTextureParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.LightEvaluation, HShaderParams._ProbeNormalDepth,   ProbeNormalDepth.rt);
                    cmd.SetComputeTextureParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.LightEvaluation, HShaderParams._HitRadiance_Output, HitRadiance.rt);
                    cmd.SetComputeBufferParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.LightEvaluation, HShaderParams._TracingCoords, IndirectCoordsWS.ComputeBuffer);
                    cmd.SetComputeBufferParam(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.LightEvaluation, HShaderParams._RayCounter,    RayCounterWS);
                    cmd.SetComputeIntParam(HTracingWorldSpace, HShaderParams._IndexXR, 0);
                    cmd.DispatchCompute(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.LightEvaluation, IndirectArgumentsWS, 0);
                    if (HRenderer.TextureXrSlices > 1)
                    {
                        cmd.SetComputeIntParam(HTracingWorldSpace, HShaderParams._IndexXR, 1);
                        cmd.DispatchCompute(HTracingWorldSpace, (int)HTracingWorldSpaceKernel.LightEvaluation, IndirectArgumentsWS, sizeof(uint) * 3);
                    }
                }
            } 
            
            // ---------------------------------------- RADIANCE CACHING ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_RadianceCachingProfilingSampler))
            {
                if (HSettings.GeneralSettings.Multibounce == Multibounce.IrradianceCache)
                {
	                if (HSettings.LightingSettings.EvaluatePunctualLights) HRadianceCache.EnableKeyword(EVALUATE_PUNCTUAL_LIGHTS);
	                else HRadianceCache.DisableKeyword(EVALUATE_PUNCTUAL_LIGHTS);
	                
	                // Clear cache on request
	                if (ClearRadianceCache)
	                {
		                cmd.DispatchCompute(HRadianceCache, (int)HRadianceCacheKernel.CacheDataClear, HConstants.HASH_STORAGE_SIZE / 64, 1, 1);
		                ClearRadianceCache = false;
	                }
	              
                    HRadianceCache.SetInt(HShaderParams._HashUpdateFrameIndex, _hashUpdateFrameIndex);
                    //Debug.Log("_hashUpdateFrameIndex = "+ _hashUpdateFrameIndex);

                    // Cache writing at primary surfaces
                    using (new HTraceProfilingScope(cmd, s_PrimaryCacheSpawnProfilingSampler))
                    {
                        cmd.SetComputeTextureParam(HRadianceCache, (int)HRadianceCacheKernel.CachePrimarySpawn, HShaderParams._ReprojectionCoords, ReprojectionCoord.rt);
                        cmd.SetComputeTextureParam(HRadianceCache, (int)HRadianceCacheKernel.CachePrimarySpawn, HShaderParams._ProbeNormalDepth, ProbeNormalDepth.rt);
                        cmd.SetComputeTextureParam(HRadianceCache, (int)HRadianceCacheKernel.CachePrimarySpawn, HShaderParams._GeometryNormal, GeometryNormal.rt);
                        cmd.SetComputeTextureParam(HRadianceCache, (int)HRadianceCacheKernel.CachePrimarySpawn, HShaderParams._RadianceAtlas, HitRadiance.rt);
                        cmd.DispatchCompute(HRadianceCache, (int)HRadianceCacheKernel.CachePrimarySpawn, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);
                    }

                    // Cache tracing update
                    using (new HTraceProfilingScope(cmd, s_CacheTracingUpdateProfilingSampler))
                    {
                        cmd.SetComputeFloatParam(HRadianceCache, HShaderParams._RayLength, HSettings.GeneralSettings.RayLength);
                        cmd.DispatchCompute(HRadianceCache, (int)HRadianceCacheKernel.CacheTracingUpdate, (HConstants.HASH_STORAGE_SIZE / HConstants.HASH_UPDATE_FRACTION) / 64, 1, 1); 
                    }

                    // Cache light evaluation at hit points
                    using (new HTraceProfilingScope(cmd, s_CacheLightEvaluationProfilingSampler))
                    {
                        cmd.DispatchCompute(HRadianceCache, (int)HRadianceCacheKernel.CacheLightEvaluation, (HConstants.HASH_STORAGE_SIZE / HConstants.HASH_UPDATE_FRACTION) / 64, 1, 1); 
                    }

                    //// Cache writing at primary surfaces
                    //using (new HTraceProfilingScope(cmd, s_PrimaryCacheSpawnProfilingSampler))
                    //{  
                    //    cmd.SetComputeTextureParam(HRadianceCache, (int)HRadianceCacheKernel.CachePrimarySpawn, HShaderParams._ReprojectionCoords, ReprojectionCoord.rt);
                    //    cmd.SetComputeTextureParam(HRadianceCache, (int)HRadianceCacheKernel.CachePrimarySpawn, HShaderParams._ProbeNormalDepth,   ProbeNormalDepth.rt);
                    //    cmd.SetComputeTextureParam(HRadianceCache, (int)HRadianceCacheKernel.CachePrimarySpawn, HShaderParams._GeometryNormal,     GeometryNormal.rt);
                    //    cmd.SetComputeTextureParam(HRadianceCache, (int)HRadianceCacheKernel.CachePrimarySpawn, HShaderParams._RadianceAtlas,      HitRadiance.rt);
                    //    cmd.DispatchCompute(HRadianceCache, (int)HRadianceCacheKernel.CachePrimarySpawn, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);
                    //}

                    // Cache counter update, deallocation of out-of-bounds entries, filtered cache population
                    using (new HTraceProfilingScope(cmd, s_CacheDataUpdateProfilingSampler))
                    {   
                        // Clear filtered cache every frame before writing to it
                        // CoreUtils.SetRenderTarget(ctx.cmd, RadianceCacheFiltered, ClearFlag.Color, Color.clear, 0, CubemapFace.Unknown, -1);
                        
                        cmd.DispatchCompute(HRadianceCache, (int)HRadianceCacheKernel.CacheDataUpdate, HConstants.HASH_STORAGE_SIZE / 64, 1, 1);
                    }  
                }
                
            }

            
            // ---------------------------------------- SPATIAL PREPASS ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_SpatialPrepassProfilingSampler))
            {   
                // Gather probe ambient occlusion from ray hit distance and temporally accumulate
                cmd.SetComputeTextureParam(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusion, HShaderParams._RayDistanceSS,                 HitDistanceScreenSpace.rt);
                cmd.SetComputeTextureParam(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusion, HShaderParams._RayDistanceWS,                 HitDistanceWorldSpace.rt);
                cmd.SetComputeTextureParam(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusion, HShaderParams._ProbeNormalDepth,              ProbeNormalDepth.rt);
                cmd.SetComputeTextureParam(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusion, HShaderParams._ReprojectionWeights,           PersistentReprojectionWeights.rt);
                cmd.SetComputeTextureParam(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusion, HShaderParams._PersistentReprojectionCoord,   PersistentReprojectionCoord.rt);
                cmd.SetComputeTextureParam(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusion, HShaderParams._ProbeAmbientOcclusion_History, ProbeAmbientOcclusion_History.rt);
                cmd.SetComputeTextureParam(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusion, HShaderParams._ProbeAmbientOcclusion_Output,  ProbeAmbientOcclusion.rt);
                cmd.DispatchCompute(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusion, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);
                
                // Prepare offsets and weights for further spatial passes
                cmd.SetComputeTextureParam(HSpatialPrepass, (int)HSpatialPrepassKernel.SpatialPrepass, HShaderParams._ProbeNormalDepth,         ProbeNormalDepth.rt);
                cmd.SetComputeTextureParam(HSpatialPrepass, (int)HSpatialPrepassKernel.SpatialPrepass, HShaderParams._ProbeAmbientOcclusion,    ProbeAmbientOcclusion.rt);
                cmd.SetComputeTextureParam(HSpatialPrepass, (int)HSpatialPrepassKernel.SpatialPrepass, HShaderParams._ProbeNormalDepth_History, ProbeNormalDepth_History.rt);
                cmd.SetComputeTextureParam(HSpatialPrepass, (int)HSpatialPrepassKernel.SpatialPrepass, HShaderParams._SpatialOffsets_Output,    SpatialOffsetsPacked.rt);
                cmd.SetComputeTextureParam(HSpatialPrepass, (int)HSpatialPrepassKernel.SpatialPrepass, HShaderParams._SpatialWeights_Output,    SpatialWeightsPacked.rt);
                cmd.SetComputeBufferParam(HSpatialPrepass, (int)HSpatialPrepassKernel.SpatialPrepass, HShaderParams._PointDistribution,    PointDistributionBuffer);
                cmd.SetComputeBufferParam(HSpatialPrepass, (int)HSpatialPrepassKernel.SpatialPrepass, HShaderParams._SpatialOffsetsBuffer, SpatialOffsetsBuffer);
                cmd.DispatchCompute(HSpatialPrepass, (int)HSpatialPrepassKernel.SpatialPrepass, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);
       
                // Spatially filter probe ambient occlusion
                cmd.SetComputeTextureParam(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusionSpatialFilter, HShaderParams._SpatialWeightsPacked,                 SpatialWeightsPacked.rt);
                cmd.SetComputeTextureParam(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusionSpatialFilter, HShaderParams._SpatialOffsetsPacked,                 SpatialOffsetsPacked.rt);
                cmd.SetComputeTextureParam(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusionSpatialFilter, HShaderParams._ProbeAmbientOcclusion,                ProbeAmbientOcclusion.rt);
                cmd.SetComputeTextureParam(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusionSpatialFilter, HShaderParams._ProbeAmbientOcclusion_OutputFiltered, ProbeAmbientOcclusion_Filtered.rt);
                cmd.DispatchCompute(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusionSpatialFilter, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);
            }
            

            // ---------------------------------------- ReSTIR TEMPORAL REUSE ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_ReSTIRTemporalReuseProfilingSampler))
            {
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasTemporalReuse, HShaderParams._ShadowGuidanceMask, ShadowGuidanceMask_Filtered.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasTemporalReuse, HShaderParams._RayDirection, RayDirections.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasTemporalReuse, HShaderParams._RayDistance, HitDistanceWorldSpace.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasTemporalReuse, HShaderParams._RadianceAtlas, HitRadiance.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasTemporalReuse, HShaderParams._ProbeDiffuse, ProbeDiffuse.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasTemporalReuse, HShaderParams._ProbeNormalDepth, ProbeNormalDepth.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasTemporalReuse, HShaderParams._ReprojectionWeights, PersistentReprojectionWeights.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasTemporalReuse, HShaderParams._PersistentReprojectionCoord, PersistentReprojectionCoord.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasTemporalReuse, HShaderParams._ReservoirAtlas_Output, ReservoirAtlas.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasTemporalReuse, HShaderParams._ReservoirAtlas_History, ReservoirAtlas_History.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasTemporalReuse, HShaderParams._ReservoirAtlasRayData_Output, ReservoirAtlasRayData_A.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasTemporalReuse, HShaderParams._ReservoirAtlasRadianceData_Output, ReservoirAtlasRadianceData_A.rt);
                cmd.SetComputeIntParam(HReSTIR, HShaderParams._UseDiffuseWeight, HSettings.GeneralSettings.DebugModeWS == DebugModeWS.None ? 1 : 0);
                cmd.DispatchCompute(HReSTIR, (int)HReSTIRKernel.ProbeAtlasTemporalReuse, probeAtlasResX_8, probeAtlasResY_8, HRenderer.TextureXrSlices);
            }
            
         
            // ---------------------------------------- RESERVOIR OCCLUSION VALIDATION ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_ReservoirOcclusionValidationProfilingSampler))
            {
                // Run one pass of spatial reuse in disocclusion areas to generate shadow guidance mask
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuseDisocclusion, HShaderParams._ProbeDiffuse, ProbeDiffuse.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuseDisocclusion, HShaderParams._SpatialWeightsPacked, SpatialWeightsPacked.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuseDisocclusion, HShaderParams._SpatialOffsetsPacked, SpatialOffsetsPacked.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuseDisocclusion, HShaderParams._ReservoirAtlasRayData, ReservoirAtlasRayData_A.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuseDisocclusion, HShaderParams._ReservoirAtlasRadianceData, ReservoirAtlasRadianceData_A.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuseDisocclusion, HShaderParams._ReservoirAtlasRayData_Output, ReservoirAtlasRayData_C.rt);
                cmd.SetComputeBufferParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuseDisocclusion, HShaderParams._TracingCoords, IndirectCoordsSF.ComputeBuffer);
                cmd.SetComputeBufferParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuseDisocclusion, HShaderParams._RayCounter,    RayCounter);
                cmd.SetComputeIntParam(HReSTIR, HShaderParams._IndexXR, 0);
                cmd.DispatchCompute(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuseDisocclusion, IndirectArgumentsSF, 0);
                if (HRenderer.TextureXrSlices > 1)
                {
                    cmd.SetComputeIntParam(HReSTIR, HShaderParams._IndexXR, 1);
                    cmd.DispatchCompute(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuseDisocclusion, IndirectArgumentsSF, sizeof(uint) * 3);
                }
           
                // Reproject occlusion checkerboarded history
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionReprojection, HShaderParams._ReprojectionCoords,         ReprojectionCoord.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionReprojection, HShaderParams._ProbeAmbientOcclusion,      ProbeAmbientOcclusion_Filtered.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionReprojection, HShaderParams._ShadowGuidanceMask_History, ShadowGuidanceMask_CheckerboardHistory.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionReprojection, HShaderParams._ShadowGuidanceMask_Output,  ShadowGuidanceMask.rt);
                cmd.DispatchCompute(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionReprojection, probeAtlasResX_8, probeAtlasResY_8, HRenderer.TextureXrSlices);
           
                // Validate reservoir occlusion
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionValidation, HShaderParams._ProbeNormalDepth, ProbeNormalDepth.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionValidation, HShaderParams._ReprojectionCoords, ReprojectionCoord.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionValidation, HShaderParams._ReservoirAtlasRayData, ReservoirAtlasRayData_B.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionValidation, HShaderParams._ReservoirAtlasRayData_Disocclusion, ReservoirAtlasRayData_C.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionValidation, HShaderParams._ReservoirAtlas, ReservoirAtlas.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionValidation, HShaderParams._ReservoirAtlasRadianceData_Inout, ReservoirAtlasRadianceData_B.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionValidation, HShaderParams._ShadowGuidanceMask_Output, ShadowGuidanceMask.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionValidation, HShaderParams._ProbeAmbientOcclusion, ProbeAmbientOcclusion_Filtered.rt);
                cmd.SetComputeBufferParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionValidation, HShaderParams._PointDistribution, PointDistributionBuffer);
                cmd.SetComputeBufferParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionValidation, HShaderParams._RayCounter, RayCounter);
                cmd.SetComputeBufferParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionValidation, HShaderParams._TracingCoords, IndirectCoordsOV.ComputeBuffer);
                cmd.SetComputeIntParam(HReservoirValidation, HShaderParams._IndexXR, 0);
                cmd.DispatchCompute(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionValidation, IndirectArgumentsOV, 0);
                if (HRenderer.TextureXrSlices > 1)
                {
                    cmd.SetComputeIntParam(HReservoirValidation, HShaderParams._IndexXR, 1);
                    cmd.DispatchCompute(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionValidation, IndirectArgumentsOV, sizeof(uint) * 3);
                }
           
                // Temporal accumulation pass
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionTemporalFilter, HShaderParams._ReprojectionWeights,        ReprojectionWeights.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionTemporalFilter, HShaderParams._ReprojectionCoords,         ReprojectionCoord.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionTemporalFilter, HShaderParams._ShadowGuidanceMask,         ShadowGuidanceMask.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionTemporalFilter, HShaderParams._SampleCount_History,        ShadowGuidanceMask_SamplecountHistory.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionTemporalFilter, HShaderParams._SampleCount_Output,         ShadowGuidanceMask_Samplecount.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionTemporalFilter, HShaderParams._ShadowGuidanceMask_History, ShadowGuidanceMask_History.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionTemporalFilter, HShaderParams._ShadowGuidanceMask_Output,  ShadowGuidanceMask_Accumulated.rt);
                cmd.DispatchCompute(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionTemporalFilter, probeAtlasResX_8, probeAtlasResY_8, HRenderer.TextureXrSlices);
           
                // Spatial filtering pass
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionSpatialFilter, HShaderParams._SpatialWeightsPacked,             SpatialWeightsPacked.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionSpatialFilter, HShaderParams._SpatialOffsetsPacked,             SpatialOffsetsPacked.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionSpatialFilter, HShaderParams._SampleCount,                      ShadowGuidanceMask_Samplecount.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionSpatialFilter, HShaderParams._ShadowGuidanceMask,               ShadowGuidanceMask_Accumulated.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionSpatialFilter, HShaderParams._ShadowGuidanceMask_Output,        ShadowGuidanceMask_Filtered.rt);
                cmd.SetComputeTextureParam(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionSpatialFilter, HShaderParams._ReservoirAtlasRadianceData_Inout, ReservoirAtlasRadianceData_A.rt);
                cmd.DispatchCompute(HReservoirValidation, (int)HReservoirValidationKernel.OcclusionSpatialFilter, probeAtlasResX_8, probeAtlasResY_8, HRenderer.TextureXrSlices);
            }
            
            
            // ---------------------------------------- ReSTIR SPATIAL REUSE ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_ReSTIRSpatialReuseProfilingSampler))
            {
                // Prepare spatial kernel
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ProbeDiffuse,         ProbeDiffuse.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._SpatialWeightsPacked, SpatialWeightsPacked.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._SpatialOffsetsPacked, SpatialOffsetsPacked.rt);
                
                // 1st spatial disk pass
                cmd.SetComputeIntParam(HReSTIR, HShaderParams._PassNumber, 1);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRayData,             ReservoirAtlasRayData_A.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRadianceData,        ReservoirAtlasRadianceData_A.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRayData_Output,      ReservoirAtlasRayData_B.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRadianceData_Output, ReservoirAtlasRadianceData_B.rt);
                cmd.DispatchCompute(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, probeAtlasResX_8, probeAtlasResY_8, HRenderer.TextureXrSlices);
                
                // 2nd spatial disk pass
                cmd.SetComputeIntParam(HReSTIR, HShaderParams._PassNumber, 2);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRayData,             ReservoirAtlasRayData_B.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRadianceData,        ReservoirAtlasRadianceData_B.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRayData_Output,      ReservoirAtlasRayData_A.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRadianceData_Output, ReservoirAtlasRadianceData_A.rt);
                cmd.DispatchCompute(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, probeAtlasResX_8, probeAtlasResY_8, HRenderer.TextureXrSlices);

                // 3rd spatial disk pass
                cmd.SetComputeIntParam(HReSTIR, HShaderParams._PassNumber, 3);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRayData,             ReservoirAtlasRayData_A.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRadianceData,        ReservoirAtlasRadianceData_A.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRayData_Output,      ReservoirAtlasRayData_B.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRadianceData_Output, ReservoirAtlasRadianceData_B.rt);
                cmd.DispatchCompute(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, probeAtlasResX_8, probeAtlasResY_8, HRenderer.TextureXrSlices);
                
                // 3rd spatial disk pass
                cmd.SetComputeIntParam(HReSTIR, HShaderParams._PassNumber, 2);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRayData,             ReservoirAtlasRayData_B.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRadianceData,        ReservoirAtlasRadianceData_B.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRayData_Output,      ReservoirAtlasRayData_A.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, HShaderParams._ReservoirAtlasRadianceData_Output, ReservoirAtlasRadianceData_A.rt);
                cmd.DispatchCompute(HReSTIR, (int)HReSTIRKernel.ProbeAtlasSpatialReuse, probeAtlasResX_8, probeAtlasResY_8, HRenderer.TextureXrSlices);
            }   
            
      
            // ---------------------------------------- PERSISTENT HISTORY UPDATE ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_PersistentHistoryUpdateProfilingSampler))
            {   
                // Scroll history indirection array slice by slice
                cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.HistoryIndirectionScroll, HShaderParams._ReprojectionCoord,  ReprojectionCoord.rt);
                cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.HistoryIndirectionScroll, HShaderParams._HistoryIndirection, HistoryIndirection.rt);
                
                // Scrolling cycle
                for (int i = HConstants.PERSISTENT_HISTORY_SAMPLES - 1; i > 0; i--)
                {
                    cmd.SetComputeIntParam(HTemporalReprojection, HShaderParams._HistoryArrayIndex, i);
                    cmd.DispatchCompute(HTemporalReprojection, (int)HTemporalReprojectionKernel.HistoryIndirectionScroll, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);
                }
                
                // Update history indirection coord buffer
                cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.HistoryIndirectionUpdate, HShaderParams._HistoryIndirection, HistoryIndirection.rt);
                cmd.DispatchCompute(HTemporalReprojection, (int)HTemporalReprojectionKernel.HistoryIndirectionUpdate, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);
                
                //// Update probe world position & normal history buffer
                //cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.HistoryProbeBuffersUpdate, HShaderParams._ProbeNormalDepth,                  ProbeNormalDepth.rt);
                //cmd.SetComputeTextureParam(HTemporalReprojection, (int)HTemporalReprojectionKernel.HistoryProbeBuffersUpdate, HShaderParams._ProbeWorldPosNormal_HistoryOutput, ProbeWorldPosNormal_History.rt);
                //cmd.DispatchCompute(HTemporalReprojection, (int)HTemporalReprojectionKernel.HistoryProbeBuffersUpdate, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);
                
                // Update probe ambient occlusion history buffer
                cmd.SetComputeTextureParam(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusionHistoryUpdate, HShaderParams._ProbeAmbientOcclusion,        ProbeAmbientOcclusion.rt);
                cmd.SetComputeTextureParam(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusionHistoryUpdate, HShaderParams._ProbeAmbientOcclusion_ArrayOutput, ProbeAmbientOcclusion_History.rt);
                cmd.DispatchCompute(HProbeAmbientOcclusion, (int)HProbeAmbientOcclusionKernel.ProbeAmbientOcclusionHistoryUpdate, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);
                
                // Update reserovir history buffer
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ReservoirHistoryUpdate, HShaderParams._ReservoirAtlas,        ReservoirAtlas.rt);
                cmd.SetComputeTextureParam(HReSTIR, (int)HReSTIRKernel.ReservoirHistoryUpdate, HShaderParams._ReservoirAtlas_ArrayOutput, ReservoirAtlas_History.rt);
                cmd.DispatchCompute(HReSTIR, (int)HReSTIRKernel.ReservoirHistoryUpdate, probeAtlasResX_8, probeAtlasResY_8, HRenderer.TextureXrSlices);
            }
            
            
            // ---------------------------------------- INTERPOLATION ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_InterpolationProfilingSampler))
            {   
                // Spherical harmonics gather
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.GatherSH, HShaderParams._ShadowGuidanceMask,         ShadowGuidanceMask_Accumulated.rt);
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.GatherSH, HShaderParams._ReservoirAtlasRadianceData, ReservoirAtlasRadianceData_A.rt);
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.GatherSH, HShaderParams._ReservoirAtlasRayData,      ReservoirAtlasRayData_A.rt);
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.GatherSH, HShaderParams._ProbeNormalDepth,           ProbeNormalDepth.rt);
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.GatherSH, HShaderParams._Temp,                       ShadowGuidanceMask_Accumulated.rt);
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.GatherSH, HShaderParams._PackedSH_A_Output,          PackedSH_A.rt);
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.GatherSH, HShaderParams._PackedSH_B_Output,          PackedSH_B.rt);
                cmd.DispatchCompute(HInterpolation, (int)HInterpolationKernel.GatherSH, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);
                
                // Interpolation to the final resolution
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.Interpolation, HShaderParams._ProbeSSAO,        ProbeSSAO.rt);
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.Interpolation, HShaderParams._PackedSH_A,       PackedSH_A.rt);
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.Interpolation, HShaderParams._PackedSH_B,       PackedSH_B.rt);
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.Interpolation, HShaderParams._GeometryNormal,   GeometryNormal.rt);
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.Interpolation, HShaderParams._BentNormalsAO,    BentNormalsAO_Interpolated.rt);
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.Interpolation, HShaderParams._Radiance_Output,  Radiance_Interpolated.rt);
                cmd.SetComputeTextureParam(HInterpolation, (int)HInterpolationKernel.Interpolation, HShaderParams._ProbeNormalDepth, ProbeNormalDepth.rt);
                cmd.SetComputeFloatParam(HInterpolation, HShaderParams._AO_Intensity, HSettings.ScreenSpaceLightingSettings.OcclusionIntensity);
                cmd.DispatchCompute(HInterpolation, (int)HInterpolationKernel.Interpolation, fullResX_8, fullResY_8, HRenderer.TextureXrSlices);
            }

            
            // ---------------------------------------- TEMPORAL DENOISER ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_TemporalDenoisingProfilingSampler))
            {
                cmd.SetComputeTextureParam(HTemporalDenoiser, (int)HTemporalDenoiserKernel.TemporalDenoising, HShaderParams._GeometryNormal,         GeometryNormal.rt);
                cmd.SetComputeTextureParam(HTemporalDenoiser, (int)HTemporalDenoiserKernel.TemporalDenoising, HShaderParams._NormalDepth_History,    NormalDepth_History.rt);
                cmd.SetComputeTextureParam(HTemporalDenoiser, (int)HTemporalDenoiserKernel.TemporalDenoising, HShaderParams._Radiance,               Radiance_Interpolated.rt);
                cmd.SetComputeTextureParam(HTemporalDenoiser, (int)HTemporalDenoiserKernel.TemporalDenoising, HShaderParams._Radiance_History,       RadianceAccumulated_History.rt);
                cmd.SetComputeTextureParam(HTemporalDenoiser, (int)HTemporalDenoiserKernel.TemporalDenoising, HShaderParams._Radiance_Output,        RadianceAccumulated.rt);
                cmd.SetComputeTextureParam(HTemporalDenoiser, (int)HTemporalDenoiserKernel.TemporalDenoising, HShaderParams._LuminanceDelta_Output,  LuminanceDelta.rt);
                cmd.SetComputeTextureParam(HTemporalDenoiser, (int)HTemporalDenoiserKernel.TemporalDenoising, HShaderParams._LuminanceDelta_History, LuminanceDelta_History.rt);
                cmd.DispatchCompute(HTemporalDenoiser, (int)HTemporalDenoiserKernel.TemporalDenoising, fullResX_8, fullResY_8, HRenderer.TextureXrSlices);
            }
            
            
            // ---------------------------------------- SPATIAL CLEANUP ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_SpatialCleanupProfilingSampler))
            {
                cmd.SetComputeTextureParam(HTemporalDenoiser, (int)HTemporalDenoiserKernel.SpatialCleanup, HShaderParams._GeometryNormal,            GeometryNormal.rt);
                cmd.SetComputeTextureParam(HTemporalDenoiser, (int)HTemporalDenoiserKernel.SpatialCleanup, HShaderParams._Radiance,                  RadianceAccumulated.rt);
                cmd.SetComputeTextureParam(HTemporalDenoiser, (int)HTemporalDenoiserKernel.SpatialCleanup, HShaderParams._Radiance_HistoryOutput,    RadianceAccumulated_History.rt);
                cmd.SetComputeTextureParam(HTemporalDenoiser, (int)HTemporalDenoiserKernel.SpatialCleanup, HShaderParams._NormalDepth_HistoryOutput, NormalDepth_History.rt);
                cmd.DispatchCompute(HTemporalDenoiser, (int)HTemporalDenoiserKernel.SpatialCleanup, fullResX_8, fullResY_8, HRenderer.TextureXrSlices);
            }
            
            
             // ---------------------------------------- COPY BUFFERS ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_CopyBuffersProfilingSampler))
            {
                cmd.SetComputeTextureParam(HCopy, (int)HCopyKernel.CopyProbeBuffers, HShaderParams._ShadowGuidanceMask_Samplecount,              ShadowGuidanceMask_Samplecount.rt);
                cmd.SetComputeTextureParam(HCopy, (int)HCopyKernel.CopyProbeBuffers, HShaderParams._ShadowGuidanceMask_SamplecountHistoryOutput, ShadowGuidanceMask_SamplecountHistory.rt);
                cmd.DispatchCompute(HCopy, (int)HCopyKernel.CopyProbeBuffers, probeResX_8, probeResY_8, HRenderer.TextureXrSlices);
                
                cmd.SetComputeTextureParam(HCopy, (int)HCopyKernel.CopyProbeAtlases, HShaderParams._ShadowGuidanceMask,                           ShadowGuidanceMask.rt);
                cmd.SetComputeTextureParam(HCopy, (int)HCopyKernel.CopyProbeAtlases, HShaderParams._ShadowGuidanceMask_CheckerboardHistoryOutput, ShadowGuidanceMask_CheckerboardHistory.rt);
                cmd.SetComputeTextureParam(HCopy, (int)HCopyKernel.CopyProbeAtlases, HShaderParams._ShadowGuidanceMask_Accumulated,               ShadowGuidanceMask_Accumulated.rt);
                cmd.SetComputeTextureParam(HCopy, (int)HCopyKernel.CopyProbeAtlases, HShaderParams._ShadowGuidanceMask_HistoryOutput,             ShadowGuidanceMask_History.rt);
                cmd.DispatchCompute(HCopy, (int)HCopyKernel.CopyProbeAtlases, probeAtlasResX_8, probeAtlasResY_8, HRenderer.TextureXrSlices);

                cmd.SetComputeTextureParam(HCopy, (int)HCopyKernel.CopyFullResBuffers, HShaderParams._GeometryNormal,            GeometryNormal.rt);
                cmd.SetComputeTextureParam(HCopy, (int)HCopyKernel.CopyFullResBuffers, HShaderParams._NormalDepth_HistoryOutput, NormalDepth_History.rt);
                cmd.DispatchCompute(HCopy, (int)HCopyKernel.CopyFullResBuffers, fullResX_8, fullResY_8, HRenderer.TextureXrSlices);
            }
            
            // Final output
            cmd.SetGlobalTexture(HShaderParams.g_HTraceBufferGI, RadianceAccumulated.rt);
            
            
            // ---------------------------------------- DEBUG (DON'T SHIP!) ---------------------------------------- //
            using (new HTraceProfilingScope(cmd, s_DebugPassthroughProfilingSampler))
            {
                if (HSettings.GeneralSettings.DebugModeWS != DebugModeWS.None)
                {
                    cmd.SetComputeTextureParam(HDebugPassthrough, (int)HDebugPassthroughKernel.DebugPassthrough, HShaderParams._InputA, RadianceAccumulated.rt);
                    //cmd.SetComputeTextureParam(HDebugPassthrough, hDebugPassthrough_Kernel, HShaderParams._InputB, VoxelVisualizationRayDirections.rt);
                    cmd.SetComputeTextureParam(HDebugPassthrough, (int)HDebugPassthroughKernel.DebugPassthrough, HShaderParams._Output, DebugOutput.rt);
                    cmd.DispatchCompute(HDebugPassthrough, (int)HDebugPassthroughKernel.DebugPassthrough, fullResX_8, fullResY_8, HRenderer.TextureXrSlices); 
                }
            }
            
            // Visualize voxels if requested
            if (HSettings.GeneralSettings.DebugModeWS == DebugModeWS.VoxelizedLighting || HSettings.GeneralSettings.DebugModeWS == DebugModeWS.VoxelizedColor)
            {
                {
                    if (HSettings.GeneralSettings.Multibounce == Multibounce.None) { HTracingWorldSpace.DisableKeyword(MULTIBOUNCE_CACHE); HTracingWorldSpace.DisableKeyword(MULTIBOUNCE_APV); HTracingWorldSpace.EnableKeyword(MULTIBOUNCE_OFF); }
                    if (HSettings.GeneralSettings.Multibounce == Multibounce.IrradianceCache) { HTracingWorldSpace.DisableKeyword(MULTIBOUNCE_OFF); HTracingWorldSpace.DisableKeyword(MULTIBOUNCE_APV); HTracingWorldSpace.EnableKeyword(MULTIBOUNCE_CACHE); }
                    if (HSettings.GeneralSettings.Multibounce == Multibounce.AdaptiveProbeVolumes) { HTracingWorldSpace.DisableKeyword(MULTIBOUNCE_CACHE); HTracingWorldSpace.DisableKeyword(MULTIBOUNCE_OFF); HTracingWorldSpace.EnableKeyword(MULTIBOUNCE_APV); }
                    
                    //this part in Voxelization.cs
                }
            }
            
            if (Time.frameCount % 2 == 0)
                _hFrameIndex++;
            
            _hashUpdateFrameIndex++;
        
			cmd.SetGlobalTexture(HShaderParams.g_GeometryNormal, GeometryNormal.rt);
			
            if (HSettings.GeneralSettings.DebugModeWS != DebugModeWS.None)
                cmd.SetGlobalTexture(HShaderParams.g_HTraceBufferGI, DebugOutput.rt);
		} 
	}
}
