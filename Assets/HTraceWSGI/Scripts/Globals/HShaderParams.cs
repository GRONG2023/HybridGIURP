using UnityEngine;
using UnityEngine.Rendering;

namespace HTraceWSGI.Scripts.Globals
{
	public static class HShaderParams
	{
		//  ---------------------------------------------- Globals, "g_" prefix ----------------------------------------------
		public static readonly int g_HTraceGBuffer0         = Shader.PropertyToID("g_HTraceGBuffer0");
		public static readonly int g_HTraceGBuffer1         = Shader.PropertyToID("g_HTraceGBuffer1");
		public static readonly int g_HTraceGBuffer2         = Shader.PropertyToID("g_HTraceGBuffer2");
		public static readonly int g_HTraceGBuffer3         = Shader.PropertyToID("g_HTraceGBuffer3");
		public static readonly int g_HTraceRenderLayerMask  = Shader.PropertyToID("g_HTraceRenderLayerMask");
		public static readonly int g_HTraceColor            = Shader.PropertyToID("g_HTraceColor");
		public static readonly int g_HTraceDepth            = Shader.PropertyToID("g_HTraceDepth");
		public static readonly int g_HTraceDepthPyramidWSGI = Shader.PropertyToID("g_HTraceDepthPyramidWSGI");
		//public static readonly int g_HTraceStencilBuffer    = Shader.PropertyToID("g_HTraceStencilBuffer");
		public static readonly int g_HTraceMotionVectors    = Shader.PropertyToID("g_HTraceMotionVectors");
		public static readonly int g_HTraceMotionMask       = Shader.PropertyToID("g_HTraceMotionMask");
		public static readonly int g_HTraceBufferGI         = Shader.PropertyToID("_HTraceBufferGI");
		public static readonly int g_HTraceShadowmap        = Shader.PropertyToID("g_HTraceShadowmap");
		public static readonly int g_LightClusterDebug = Shader.PropertyToID("g_LightClusterDebug");
		public static readonly int g_CameraDepthTexture          = Shader.PropertyToID("_CameraDepthTexture");
		public static readonly int g_CameraNormalsTexture        = Shader.PropertyToID("_CameraNormalsTexture");
		
		public static readonly int g_VoxelPositionPyramid      = Shader.PropertyToID("_VoxelPositionPyramid");
		public static readonly int g_ProbeSize                 = Shader.PropertyToID("_ProbeSize");
		public static readonly int g_OctahedralSize            = Shader.PropertyToID("_OctahedralSize");
		public static readonly int g_HFrameIndex               = Shader.PropertyToID("_HFrameIndex");
		public static readonly int g_ReprojectSkippedFrame     = Shader.PropertyToID("_ReprojectSkippedFrame");
		public static readonly int g_PersistentHistorySamples  = Shader.PropertyToID("_PersistentHistorySamples");
		public static readonly int g_SkyOcclusionCone          = Shader.PropertyToID("_SkyOcclusionCone");
		public static readonly int g_DirectionalLightIntensity = Shader.PropertyToID("_DirectionalLightIntensity");
		public static readonly int g_SurfaceDiffuseIntensity   = Shader.PropertyToID("_SurfaceDiffuseIntensity");
		public static readonly int g_SkyLightIntensity         = Shader.PropertyToID("_SkyLightIntensity");
		public static readonly int g_HashBuffer_Key            = Shader.PropertyToID("_HashBuffer_Key");
		public static readonly int g_HashBuffer_Payload        = Shader.PropertyToID("_HashBuffer_Payload");
		public static readonly int g_HashBuffer_Counter        = Shader.PropertyToID("_HashBuffer_Counter");
		public static readonly int g_HashBuffer_Radiance       = Shader.PropertyToID("_HashBuffer_Radiance");
		public static readonly int g_HashBuffer_Position       = Shader.PropertyToID("_HashBuffer_Position");
		public static readonly int g_HashStorageSize           = Shader.PropertyToID("_HashStorageSize");
		public static readonly int g_HashUpdateFraction        = Shader.PropertyToID("_HashUpdateFraction");
		public static readonly int g_GeometryNormal            = Shader.PropertyToID("_GeometryNormal");
		public static readonly int g_TestCheckbox              = Shader.PropertyToID("_TestCheckbox");
        public static readonly int _ScramblingTileXSPP = Shader.PropertyToID("_ScramblingTileXSPP");
        public static readonly int _RankingTileXSPP = Shader.PropertyToID("_RankingTileXSPP");
        public static readonly int _ScramblingTexture = Shader.PropertyToID("_ScramblingTexture");
        public static readonly int _OwenScrambledTexture = Shader.PropertyToID("_OwenScrambledTexture");
        public static readonly int unity_SpecCube2 = Shader.PropertyToID("unity_SpecCube2");
        public static readonly int unity_SpecCube0 = Shader.PropertyToID("unity_SpecCube0");


        // ---------------------------------------------- GBuffer ----------------------------------------------
        public static readonly int _GBuffer0                     = Shader.PropertyToID("_GBuffer0");
		public static readonly int _GBuffer1                     = Shader.PropertyToID("_GBuffer1");
		public static readonly int _GBuffer2                     = Shader.PropertyToID("_GBuffer2");
		public static readonly int _GBuffer3                     = Shader.PropertyToID("_GBuffer3");
		//public static readonly int _CameraRenderingLayersTexture = Shader.PropertyToID("_CameraRenderingLayersTexture");
		//public static readonly int _GBufferTexture0              = Shader.PropertyToID("_GBufferTexture0");
		public static readonly int _CameraGBufferTexture0        = Shader.PropertyToID("_CameraGBufferTexture0");

		public static readonly int H_SHAr = Shader.PropertyToID("H_SHAr");
		public static readonly int H_SHAg = Shader.PropertyToID("H_SHAg");
		public static readonly int H_SHAb = Shader.PropertyToID("H_SHAb");
		public static readonly int H_SHBr = Shader.PropertyToID("H_SHBr");
		public static readonly int H_SHBg = Shader.PropertyToID("H_SHBg");
		public static readonly int H_SHBb = Shader.PropertyToID("H_SHBb");
		public static readonly int H_SHC  = Shader.PropertyToID("H_SHC");

		public static readonly string _GBUFFER_NORMALS_OCT    = "_GBUFFER_NORMALS_OCT";
		public static readonly string _WRITE_RENDERING_LAYERS = "_WRITE_RENDERING_LAYERS";

		public static readonly ShaderTagId UniversalGBufferTag = new ShaderTagId("UniversalGBuffer");

		// DepthPyramid
		public static readonly int _DepthIntermediate        = Shader.PropertyToID("_DepthIntermediate");
		public static readonly int _DepthIntermediate_Output = Shader.PropertyToID("_DepthIntermediate_Output");
		public static readonly int _DepthPyramid_OutputMIP0  = Shader.PropertyToID("_DepthPyramid_OutputMIP0");
		public static readonly int _DepthPyramid_OutputMIP1  = Shader.PropertyToID("_DepthPyramid_OutputMIP1");
		public static readonly int _DepthPyramid_OutputMIP2  = Shader.PropertyToID("_DepthPyramid_OutputMIP2");
		public static readonly int _DepthPyramid_OutputMIP3  = Shader.PropertyToID("_DepthPyramid_OutputMIP3");
		public static readonly int _DepthPyramid_OutputMIP4  = Shader.PropertyToID("_DepthPyramid_OutputMIP4");
		public static readonly int _DepthPyramid_OutputMIP5  = Shader.PropertyToID("_DepthPyramid_OutputMIP5");
		public static readonly int _DepthPyramid_OutputMIP6  = Shader.PropertyToID("_DepthPyramid_OutputMIP6");
		public static readonly int _DepthPyramid_OutputMIP7  = Shader.PropertyToID("_DepthPyramid_OutputMIP7");
		public static readonly int _DepthPyramid_OutputMIP8  = Shader.PropertyToID("_DepthPyramid_OutputMIP8");

		// ---------------------------------------------- Matrix ----------------------------------------------
		public static readonly int H_MATRIX_VP        = Shader.PropertyToID("_H_MATRIX_VP");
		public static readonly int H_MATRIX_I_VP      = Shader.PropertyToID("_H_MATRIX_I_VP");
		public static readonly int H_MATRIX_PREV_VP   = Shader.PropertyToID("_H_MATRIX_PREV_VP");
		public static readonly int H_MATRIX_PREV_I_VP = Shader.PropertyToID("_H_MATRIX_PREV_I_VP");

		// ---------------------------------------------- Additional ----------------------------------------------
		public static int HRenderScale         = Shader.PropertyToID("_HRenderScale");
		public static int HRenderScalePrevious = Shader.PropertyToID("_HRenderScalePrevious");
		public static int HFrameCount           = Shader.PropertyToID("_HFrameCount");
		public static int ScreenSize           = Shader.PropertyToID("_ScreenSize");

		// ---------------------------------------------- Shared Params Other ----------------------------------------------
		public static readonly int _BentNormalAmbientOcclusion_Output = Shader.PropertyToID("_BentNormalAmbientOcclusion_Output");
		public static readonly int _NormalDepthHalf_Output            = Shader.PropertyToID("_NormalDepthHalf_Output");
		public static readonly int _Camera_FOV                        = Shader.PropertyToID("_Camera_FOV");
		public static readonly int _AmbientOcclusion                  = Shader.PropertyToID("_AmbientOcclusion");
		public static readonly int _NormalDepthHalf                   = Shader.PropertyToID("_NormalDepthHalf");
		public static readonly int _BentNormalAO_Output               = Shader.PropertyToID("_BentNormalAO_Output");
		public static readonly int _GeometryNormal_Output             = Shader.PropertyToID("_GeometryNormal_Output");

		public static readonly int _PointDistribution_Output    = Shader.PropertyToID("_PointDistribution_Output");
		public static readonly int _SpatialOffsetsBuffer_Output = Shader.PropertyToID("_SpatialOffsetsBuffer_Output");

		public static readonly int _GeometryNormal          = Shader.PropertyToID("_GeometryNormal");
		public static readonly int _ProbeNormalDepth_Output = Shader.PropertyToID("_ProbeNormalDepth_Output");
		public static readonly int _ProbeDiffuse_Output     = Shader.PropertyToID("_ProbeDiffuse_Output");
		public static readonly int _SSAO                    = Shader.PropertyToID("_SSAO");
		public static readonly int _ProbeSSAO_Output        = Shader.PropertyToID("_ProbeSSAO_Output");
		public static readonly int _ProbeNormalDepth        = Shader.PropertyToID("_ProbeNormalDepth");

		public static readonly int _HistoryIndirection                   = Shader.PropertyToID("_HistoryIndirection");
		public static readonly int _ProbeWorldPosNormal_History          = Shader.PropertyToID("_ProbeWorldPosNormal_History");
		public static readonly int _ReprojectionCoords_Output            = Shader.PropertyToID("_ReprojectionCoords_Output");
		public static readonly int _ReprojectionWeights_Output           = Shader.PropertyToID("_ReprojectionWeights_Output");
		public static readonly int _PersistentReprojectionWeights_Output = Shader.PropertyToID("_PersistentReprojectionWeights_Output");
		public static readonly int _PersistentReprojectionCoord_Output   = Shader.PropertyToID("_PersistentReprojectionCoord_Output");

		public static readonly int _ReprojectionCoords           = Shader.PropertyToID("_ReprojectionCoords");
		public static readonly int _RayDirectionsJittered_Output = Shader.PropertyToID("_RayDirectionsJittered_Output");
		public static readonly int _IndirectCoordsSS_Output      = Shader.PropertyToID("_IndirectCoordsSS_Output");
		public static readonly int _IndirectCoordsOV_Output      = Shader.PropertyToID("_IndirectCoordsOV_Output");
		public static readonly int _IndirectCoordsSF_Output      = Shader.PropertyToID("_IndirectCoordsSF_Output");
		public static readonly int _RayCounter_Output            = Shader.PropertyToID("_RayCounter_Output");
		public static readonly int _RayCounter                   = Shader.PropertyToID("_RayCounter");
		public static readonly int _TracingCoords                = Shader.PropertyToID("_TracingCoords");
		public static readonly int _IndirectArguments_Output     = Shader.PropertyToID("_IndirectArguments_Output");
		public static readonly int _RayCounterIndex              = Shader.PropertyToID("_RayCounterIndex");

		public static readonly int _NormalDepth_History  = Shader.PropertyToID("_NormalDepth_History");
		public static readonly int _RayDirection         = Shader.PropertyToID("_RayDirection");
		public static readonly int _HitDistance_Output   = Shader.PropertyToID("_HitDistance_Output");
		public static readonly int _HitCoord_Output      = Shader.PropertyToID("_HitCoord_Output");
		public static readonly int _IndexXR              = Shader.PropertyToID("_IndexXR");
		public static readonly int _ColorPyramid_History = Shader.PropertyToID("_ColorPyramid_History");
		public static readonly int _Radiance_History     = Shader.PropertyToID("_Radiance_History");
		public static readonly int _HitCoord             = Shader.PropertyToID("_HitCoord");
		public static readonly int _HitRadiance_Output   = Shader.PropertyToID("_HitRadiance_Output");

		public static readonly int _HitDistance              = Shader.PropertyToID("_HitDistance");
		public static readonly int _TracingRayCounter_Output = Shader.PropertyToID("_TracingRayCounter_Output");
		public static readonly int _TracingCoords_Output     = Shader.PropertyToID("_TracingCoords_Output");
		public static readonly int _VoxelPayload_Output      = Shader.PropertyToID("_VoxelPayload_Output");
		public static readonly int _PointDistribution        = Shader.PropertyToID("_PointDistribution");
		public static readonly int _RayLength                = Shader.PropertyToID("_RayLength");
		public static readonly int _VoxelPayload             = Shader.PropertyToID("_VoxelPayload");
		public static readonly int _HashUpdateFrameIndex     = Shader.PropertyToID("_HashUpdateFrameIndex");
		public static readonly int _RadianceAtlas            = Shader.PropertyToID("_RadianceAtlas");

		public static readonly int _RayDistanceSS                        = Shader.PropertyToID("_RayDistanceSS");
		public static readonly int _RayDistanceWS                        = Shader.PropertyToID("_RayDistanceWS");
		public static readonly int _ReprojectionWeights                  = Shader.PropertyToID("_ReprojectionWeights");
		public static readonly int _PersistentReprojectionCoord          = Shader.PropertyToID("_PersistentReprojectionCoord");
		public static readonly int _ProbeAmbientOcclusion_History        = Shader.PropertyToID("_ProbeAmbientOcclusion_History");
		public static readonly int _ProbeAmbientOcclusion_Output         = Shader.PropertyToID("_ProbeAmbientOcclusion_Output");
		public static readonly int _ProbeNormalDepth_History             = Shader.PropertyToID("_ProbeNormalDepth_History");
		public static readonly int _SpatialOffsets_Output                = Shader.PropertyToID("_SpatialOffsets_Output");
		public static readonly int _SpatialWeights_Output                = Shader.PropertyToID("_SpatialWeights_Output");
		public static readonly int _SpatialOffsetsBuffer                 = Shader.PropertyToID("_SpatialOffsetsBuffer");
		public static readonly int _SpatialWeightsPacked                 = Shader.PropertyToID("_SpatialWeightsPacked");
		public static readonly int _SpatialOffsetsPacked                 = Shader.PropertyToID("_SpatialOffsetsPacked");
		public static readonly int _ProbeAmbientOcclusion                = Shader.PropertyToID("_ProbeAmbientOcclusion");
		public static readonly int _ProbeAmbientOcclusion_ArrayOutput    = Shader.PropertyToID("_ProbeAmbientOcclusion_ArrayOutput");
		public static readonly int _ProbeAmbientOcclusion_OutputFiltered = Shader.PropertyToID("_ProbeAmbientOcclusion_OutputFiltered");

		public static readonly int _ShadowGuidanceMask                 = Shader.PropertyToID("_ShadowGuidanceMask");
		public static readonly int _RayDistance                        = Shader.PropertyToID("_RayDistance");
		public static readonly int _ProbeDiffuse                       = Shader.PropertyToID("_ProbeDiffuse");
		public static readonly int _ReservoirAtlas_Output              = Shader.PropertyToID("_ReservoirAtlas_Output");
		public static readonly int _ReservoirAtlas_History             = Shader.PropertyToID("_ReservoirAtlas_History");
		public static readonly int _ReservoirAtlasRayData_Output       = Shader.PropertyToID("_ReservoirAtlasRayData_Output");
		public static readonly int _ReservoirAtlasRadianceData_Output  = Shader.PropertyToID("_ReservoirAtlasRadianceData_Output");
		public static readonly int _UseDiffuseWeight                   = Shader.PropertyToID("_UseDiffuseWeight");
		public static readonly int _PassNumber                         = Shader.PropertyToID("_PassNumber");
		public static readonly int _ReservoirAtlasRayData              = Shader.PropertyToID("_ReservoirAtlasRayData");
		public static readonly int _ReservoirAtlasRadianceData         = Shader.PropertyToID("_ReservoirAtlasRadianceData");
		public static readonly int _ShadowGuidanceMask_History         = Shader.PropertyToID("_ShadowGuidanceMask_History");
		public static readonly int _ShadowGuidanceMask_Output          = Shader.PropertyToID("_ShadowGuidanceMask_Output");
		public static readonly int _ReservoirAtlasRayData_Disocclusion = Shader.PropertyToID("_ReservoirAtlasRayData_Disocclusion");
		public static readonly int _ReservoirAtlas                     = Shader.PropertyToID("_ReservoirAtlas");
		public static readonly int _ReservoirAtlas_ArrayOutput         = Shader.PropertyToID("_ReservoirAtlas_ArrayOutput");
		public static readonly int _ReservoirAtlasRadianceData_Inout   = Shader.PropertyToID("_ReservoirAtlasRadianceData_Inout");
		public static readonly int _SampleCount_History                = Shader.PropertyToID("_SampleCount_History");
		public static readonly int _SampleCount_Output                 = Shader.PropertyToID("_SampleCount_Output");
		public static readonly int _SampleCount                        = Shader.PropertyToID("_SampleCount");
		public static readonly int _ReprojectionCoord                  = Shader.PropertyToID("_ReprojectionCoord");
		public static readonly int _HistoryArrayIndex                  = Shader.PropertyToID("_HistoryArrayIndex");
		public static readonly int _ProbeWorldPosNormal_HistoryOutput  = Shader.PropertyToID("_ProbeWorldPosNormal_HistoryOutput");

		public static readonly int _Temp              = Shader.PropertyToID("_Temp");
		public static readonly int _PackedSH_A_Output = Shader.PropertyToID("_PackedSH_A_Output");
		public static readonly int _PackedSH_B_Output = Shader.PropertyToID("_PackedSH_B_Output");
		public static readonly int _ProbeSSAO         = Shader.PropertyToID("_ProbeSSAO");
		public static readonly int _PackedSH_A        = Shader.PropertyToID("_PackedSH_A");
		public static readonly int _PackedSH_B        = Shader.PropertyToID("_PackedSH_B");
		public static readonly int _BentNormalsAO     = Shader.PropertyToID("_BentNormalsAO");
		public static readonly int _Radiance_Output   = Shader.PropertyToID("_Radiance_Output");
		public static readonly int _AO_Intensity      = Shader.PropertyToID("_AO_Intensity");

		public static readonly int _Radiance                  = Shader.PropertyToID("_Radiance");
		public static readonly int _LuminanceDelta_Output     = Shader.PropertyToID("_LuminanceDelta_Output");
		public static readonly int _LuminanceDelta_History    = Shader.PropertyToID("_LuminanceDelta_History");
		public static readonly int _Radiance_HistoryOutput    = Shader.PropertyToID("_Radiance_HistoryOutput");
		public static readonly int _NormalDepth_HistoryOutput = Shader.PropertyToID("_NormalDepth_HistoryOutput");

		public static readonly int _ShadowGuidanceMask_Samplecount               = Shader.PropertyToID("_ShadowGuidanceMask_Samplecount");
		public static readonly int _ShadowGuidanceMask_SamplecountHistoryOutput  = Shader.PropertyToID("_ShadowGuidanceMask_SamplecountHistoryOutput");
		public static readonly int _ShadowGuidanceMask_CheckerboardHistoryOutput = Shader.PropertyToID("_ShadowGuidanceMask_CheckerboardHistoryOutput");
		public static readonly int _ShadowGuidanceMask_Accumulated               = Shader.PropertyToID("_ShadowGuidanceMask_Accumulated");
		public static readonly int _ShadowGuidanceMask_HistoryOutput             = Shader.PropertyToID("_ShadowGuidanceMask_HistoryOutput");

		public static readonly int _InputA               = Shader.PropertyToID("_InputA");
		public static readonly int _InputB               = Shader.PropertyToID("_InputB");
		public static readonly int _Output               = Shader.PropertyToID("_Output");
		public static readonly int _DebugCameraFrustum   = Shader.PropertyToID("_DebugCameraFrustum");
		public static readonly int _DebugRayDirection    = Shader.PropertyToID("_DebugRayDirection");
		public static readonly int _Visualization_Output = Shader.PropertyToID("_Visualization_Output");
		public static readonly int _MultibounceMode      = Shader.PropertyToID("_MultibounceMode");
		
		public static readonly int _HTraceReflectionsGI_SpatialFilteringRadius = Shader.PropertyToID("_HTraceReflectionsGI_SpatialFilteringRadius");
		public static readonly int _HTraceReflectionsGI_JitterRadius           = Shader.PropertyToID("_HTraceReflectionsGI_JitterRadius");
		public static readonly int _HTraceReflectionsGI_TemporalJitter         = Shader.PropertyToID("_HTraceReflectionsGI_TemporalJitter");
		public static readonly int _HTraceReflectionsGI_RayBias                = Shader.PropertyToID("_HTraceReflectionsGI_RayBias");
		public static readonly int _HTraceReflectionsGI_MaxRayLength           = Shader.PropertyToID("_HTraceReflectionsGI_MaxRayLength");
		
		public static readonly int _ClearTexture      = Shader.PropertyToID("_ClearTexture");
	}
}
