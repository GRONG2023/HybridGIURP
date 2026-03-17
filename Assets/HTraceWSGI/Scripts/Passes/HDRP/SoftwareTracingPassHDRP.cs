//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Data.Private;
using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Passes.Shared;
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
    internal class SoftwareTracingPassHDRP : ScriptableRenderPass
    {
        private bool _initialized = false;

        public SoftwareTracingPassHDRP()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingGbuffer + 5;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!_initialized)
            {
                if (SoftwareTracingShared.HReservoirValidation == null) SoftwareTracingShared.HReservoirValidation = HExtensions.LoadComputeShader("HReservoirValidation");
                if (SoftwareTracingShared.HTracingScreenSpace == null) SoftwareTracingShared.HTracingScreenSpace = HExtensions.LoadComputeShader("HTracingScreenSpace");
                if (SoftwareTracingShared.HTracingWorldSpace == null) SoftwareTracingShared.HTracingWorldSpace = HExtensions.LoadComputeShader("HTracingWorldSpace");
                if (SoftwareTracingShared.HRadianceCache == null) SoftwareTracingShared.HRadianceCache = HExtensions.LoadComputeShader("HRadianceCache");
                if (SoftwareTracingShared.HTemporalReprojection == null) SoftwareTracingShared.HTemporalReprojection = HExtensions.LoadComputeShader("HTemporalReprojection");
                if (SoftwareTracingShared.HSpatialPrepass == null) SoftwareTracingShared.HSpatialPrepass = HExtensions.LoadComputeShader("HSpatialPrepass");
                if (SoftwareTracingShared.HProbeAmbientOcclusion == null) SoftwareTracingShared.HProbeAmbientOcclusion = HExtensions.LoadComputeShader("HProbeAmbientOcclusion");
                if (SoftwareTracingShared.HReSTIR == null) SoftwareTracingShared.HReSTIR = HExtensions.LoadComputeShader("HReSTIR");
                if (SoftwareTracingShared.HCopy == null) SoftwareTracingShared.HCopy = HExtensions.LoadComputeShader("HCopy");
                if (SoftwareTracingShared.HRayGeneration == null) SoftwareTracingShared.HRayGeneration = HExtensions.LoadComputeShader("HRayGeneration");
                if (SoftwareTracingShared.HRenderAO == null) SoftwareTracingShared.HRenderAO = HExtensions.LoadComputeShader("HRenderAO");
                if (SoftwareTracingShared.HDebugPassthrough == null) SoftwareTracingShared.HDebugPassthrough = HExtensions.LoadComputeShader("HDebugPassthrough");
                if (SoftwareTracingShared.HInterpolation == null) SoftwareTracingShared.HInterpolation = HExtensions.LoadComputeShader("HInterpolation");
                if (SoftwareTracingShared.HTemporalDenoiser == null) SoftwareTracingShared.HTemporalDenoiser = HExtensions.LoadComputeShader("HTemporalDenoiser");

                Allocation();

                SoftwareTracingShared.SkipFirstFrame = true;
                _initialized = true;
            }
        }

        internal static void Allocation(bool onlyRelease = false)
        {
            AllocateMainRT(onlyRelease);
            AllocateSSAO_RT(onlyRelease);
            AllocationHashBuffers(onlyRelease);
            AllocateDebugRT(onlyRelease);
            AllocateIndirectionBuffers(onlyRelease);
        }

        internal static void AllocateMainRT(bool onlyRelease = false)
        {
            void ReleaseTextures()
            {
                SoftwareTracingShared.ColorPreviousFrame.HRelease();

                SoftwareTracingShared.VoxelPayload.HRelease();
                SoftwareTracingShared.RayDirections.HRelease();
                SoftwareTracingShared.HitRadiance.HRelease();
                SoftwareTracingShared.HitDistanceScreenSpace.HRelease();
                SoftwareTracingShared.HitDistanceWorldSpace.HRelease();
                SoftwareTracingShared.HitCoordScreenSpace.HRelease();

                SoftwareTracingShared.ProbeAmbientOcclusion.HRelease();
                SoftwareTracingShared.ProbeAmbientOcclusion_History.HRelease();
                SoftwareTracingShared.ProbeAmbientOcclusion_Filtered.HRelease();

                SoftwareTracingShared.GeometryNormal.HRelease();
                SoftwareTracingShared.NormalDepth_History.HRelease();
                SoftwareTracingShared.ProbeNormalDepth.HRelease();
                SoftwareTracingShared.ProbeNormalDepth_History.HRelease();
                SoftwareTracingShared.ProbeWorldPosNormal_History.HRelease();
                SoftwareTracingShared.ProbeNormalDepth_Intermediate.HRelease();
                SoftwareTracingShared.ProbeDiffuse.HRelease();

                SoftwareTracingShared.HistoryIndirection.HRelease();
                SoftwareTracingShared.ReprojectionWeights.HRelease();
                SoftwareTracingShared.PersistentReprojectionWeights.HRelease();
                SoftwareTracingShared.ReprojectionCoord.HRelease();
                SoftwareTracingShared.PersistentReprojectionCoord.HRelease();

                SoftwareTracingShared.SpatialOffsetsPacked.HRelease();
                SoftwareTracingShared.SpatialWeightsPacked.HRelease();

                SoftwareTracingShared.ReservoirAtlas.HRelease();
                SoftwareTracingShared.ReservoirAtlas_History.HRelease();
                SoftwareTracingShared.ReservoirAtlasRadianceData_A.HRelease();
                SoftwareTracingShared.ReservoirAtlasRadianceData_B.HRelease();
                SoftwareTracingShared.ReservoirAtlasRadianceData_C.HRelease();
                SoftwareTracingShared.ReservoirAtlasRayData_A.HRelease();
                SoftwareTracingShared.ReservoirAtlasRayData_B.HRelease();
                SoftwareTracingShared.ReservoirAtlasRayData_C.HRelease();

                SoftwareTracingShared.ShadowGuidanceMask.HRelease();
                SoftwareTracingShared.ShadowGuidanceMask_Accumulated.HRelease();
                SoftwareTracingShared.ShadowGuidanceMask_Filtered.HRelease();
                SoftwareTracingShared.ShadowGuidanceMask_History.HRelease();
                SoftwareTracingShared.ShadowGuidanceMask_CheckerboardHistory.HRelease();
                SoftwareTracingShared.ShadowGuidanceMask_Samplecount.HRelease();
                SoftwareTracingShared.ShadowGuidanceMask_SamplecountHistory.HRelease();

                SoftwareTracingShared.PackedSH_A.HRelease();
                SoftwareTracingShared.PackedSH_B.HRelease();
                SoftwareTracingShared.Radiance_Interpolated.HRelease();

                SoftwareTracingShared.RadianceAccumulated.HRelease();
                SoftwareTracingShared.RadianceAccumulated_History.HRelease();
                SoftwareTracingShared.LuminanceDelta.HRelease();
                SoftwareTracingShared.LuminanceDelta_History.HRelease();

                SoftwareTracingShared.RadianceCacheFiltered.HRelease();

                SoftwareTracingShared.RayCounter.HRelease();
                SoftwareTracingShared.RayCounterWS.HRelease();
                SoftwareTracingShared.IndirectArgumentsSS.HRelease();
                SoftwareTracingShared.IndirectArgumentsWS.HRelease();
                SoftwareTracingShared.IndirectArgumentsOV.HRelease();
                SoftwareTracingShared.IndirectArgumentsSF.HRelease();

                SoftwareTracingShared.RayCounter = null;
                SoftwareTracingShared.RayCounterWS = null;
                SoftwareTracingShared.IndirectArgumentsSS = null;
                SoftwareTracingShared.IndirectArgumentsWS = null;
                SoftwareTracingShared.IndirectArgumentsOV = null;
                SoftwareTracingShared.IndirectArgumentsSF = null;

                SoftwareTracingShared.PointDistributionBuffer.HRelease();
                SoftwareTracingShared.SpatialOffsetsBuffer.HRelease();

                SoftwareTracingShared.PointDistributionBuffer = null;
                SoftwareTracingShared.SpatialOffsetsBuffer = null;

                SoftwareTracingShared.HashBuffer_Key.HRelease();
                SoftwareTracingShared.HashBuffer_Payload.HRelease();
                SoftwareTracingShared.HashBuffer_Counter.HRelease();
                SoftwareTracingShared.HashBuffer_Radiance.HRelease();
                SoftwareTracingShared.HashBuffer_Position.HRelease();

                SoftwareTracingShared.HashBuffer_Key = null;
                SoftwareTracingShared.HashBuffer_Payload = null;
                SoftwareTracingShared.HashBuffer_Counter = null;
                SoftwareTracingShared.HashBuffer_Radiance = null;
                SoftwareTracingShared.HashBuffer_Position = null;
            }

            if (onlyRelease)
            {
                ReleaseTextures();
                return;
            }

            ReleaseTextures();

            Vector2 FullRes = Vector2.one;
            Vector2 HalfRes = Vector2.one / 2;
            Vector2 ProbeRes = Vector2.one / HSettings.GeneralSettings.RayCountMode.ParseToProbeSize();
            Vector2 ProbeAtlasRes = Vector2.one / (float)HSettings.GeneralSettings.RayCountMode.ParseToProbeSize() * (float)HConstants.OCTAHEDRAL_SIZE;

            // -------------------------------------- BUFFERS -------------------------------------- //

            // URP中TextureXrSlices固定为1
            int textureXrSlices = 1;

            if (SoftwareTracingShared.RayCounter == null) SoftwareTracingShared.RayCounter = new ComputeBuffer(10 * textureXrSlices, sizeof(uint));
            if (SoftwareTracingShared.RayCounterWS == null) SoftwareTracingShared.RayCounterWS = new ComputeBuffer(10 * textureXrSlices, sizeof(uint));
            if (SoftwareTracingShared.IndirectArgumentsSS == null) SoftwareTracingShared.IndirectArgumentsSS = new ComputeBuffer(3 * textureXrSlices, sizeof(uint), ComputeBufferType.IndirectArguments);
            if (SoftwareTracingShared.IndirectArgumentsWS == null) SoftwareTracingShared.IndirectArgumentsWS = new ComputeBuffer(3 * textureXrSlices, sizeof(uint), ComputeBufferType.IndirectArguments);
            if (SoftwareTracingShared.IndirectArgumentsOV == null) SoftwareTracingShared.IndirectArgumentsOV = new ComputeBuffer(3 * textureXrSlices, sizeof(uint), ComputeBufferType.IndirectArguments);
            if (SoftwareTracingShared.IndirectArgumentsSF == null) SoftwareTracingShared.IndirectArgumentsSF = new ComputeBuffer(3 * textureXrSlices, sizeof(uint), ComputeBufferType.IndirectArguments);

            if (SoftwareTracingShared.PointDistributionBuffer == null) SoftwareTracingShared.PointDistributionBuffer = new ComputeBuffer(textureXrSlices * 32 * 4, 2 * sizeof(float));
            if (SoftwareTracingShared.SpatialOffsetsBuffer == null) SoftwareTracingShared.SpatialOffsetsBuffer = new ComputeBuffer(9 * 9, 2 * sizeof(int));

            if (SoftwareTracingShared.HashBuffer_Key == null) SoftwareTracingShared.HashBuffer_Key = new ComputeBuffer(HConstants.HASH_STORAGE_SIZE, 1 * sizeof(uint));
            if (SoftwareTracingShared.HashBuffer_Payload == null) SoftwareTracingShared.HashBuffer_Payload = new ComputeBuffer(HConstants.HASH_STORAGE_SIZE / HConstants.HASH_UPDATE_FRACTION, 2 * sizeof(uint));
            if (SoftwareTracingShared.HashBuffer_Counter == null) SoftwareTracingShared.HashBuffer_Counter = new ComputeBuffer(HConstants.HASH_STORAGE_SIZE, 1 * sizeof(uint));
            if (SoftwareTracingShared.HashBuffer_Radiance == null) SoftwareTracingShared.HashBuffer_Radiance = new ComputeBuffer(HConstants.HASH_STORAGE_SIZE, 4 * sizeof(uint));
            if (SoftwareTracingShared.HashBuffer_Position == null) SoftwareTracingShared.HashBuffer_Position = new ComputeBuffer(HConstants.HASH_STORAGE_SIZE, 4 * sizeof(uint));

            SoftwareTracingShared.ColorPreviousFrame.HTextureAlloc("_ColorPreviousFrame", Vector2.one, GraphicsFormat.B10G11R11_UFloatPack32, useMipMap: true);

            // -------------------------------------- TRACING RT -------------------------------------- //
            SoftwareTracingShared.VoxelPayload.HTextureAlloc("_VoxelPayload", ProbeAtlasRes, GraphicsFormat.R32G32_UInt);
            SoftwareTracingShared.RayDirections.HTextureAlloc("_RayDirections", ProbeAtlasRes, GraphicsFormat.R8G8B8A8_UNorm);
            SoftwareTracingShared.HitDistanceScreenSpace.HTextureAlloc("_HitDistanceScreenSpace", ProbeAtlasRes, GraphicsFormat.R16_UInt);
            SoftwareTracingShared.HitDistanceWorldSpace.HTextureAlloc("_HitDistanceWorldSpace", ProbeAtlasRes, GraphicsFormat.R16_SFloat);
            SoftwareTracingShared.HitRadiance.HTextureAlloc("_HitRadiance", ProbeAtlasRes, GraphicsFormat.R16G16B16A16_SFloat);
            SoftwareTracingShared.HitCoordScreenSpace.HTextureAlloc("_HitCoordScreenSpace", FullRes, GraphicsFormat.R16G16_UInt);

            // -------------------------------------- PROBE AO RT -------------------------------------- //
            SoftwareTracingShared.ProbeAmbientOcclusion.HTextureAlloc("_ProbeAmbientOcclusion", ProbeRes, GraphicsFormat.R16_UInt);
            SoftwareTracingShared.ProbeAmbientOcclusion_History.HTextureAlloc("_ProbeAmbientOcclusion_History", ProbeRes, GraphicsFormat.R16_UInt, textureXrSlices * HConstants.PERSISTENT_HISTORY_SAMPLES, textureDimension: TextureDimension.Tex2DArray);
            SoftwareTracingShared.ProbeAmbientOcclusion_Filtered.HTextureAlloc("_ProbeAmbientOcclusion_Filtered", ProbeRes, GraphicsFormat.R8_UNorm);

            // -------------------------------------- GBUFFER RT -------------------------------------- //
            SoftwareTracingShared.GeometryNormal.HTextureAlloc("_GeometryNormal", FullRes, GraphicsFormat.R16G16B16A16_SFloat);
            SoftwareTracingShared.NormalDepth_History.HTextureAlloc("_NormalDepth_History", FullRes, GraphicsFormat.R32G32_UInt);
            SoftwareTracingShared.ProbeNormalDepth.HTextureAlloc("_ProbeNormalDepth", ProbeRes, GraphicsFormat.R32G32_UInt);
            SoftwareTracingShared.ProbeNormalDepth_History.HTextureAlloc("_ProbeNormalDepth_History", ProbeRes, GraphicsFormat.R32G32_UInt);
            SoftwareTracingShared.ProbeWorldPosNormal_History.HTextureAlloc("_ProbeWorldPosNormal_History", ProbeRes, GraphicsFormat.R32G32B32A32_UInt, textureXrSlices * HConstants.PERSISTENT_HISTORY_SAMPLES, textureDimension: TextureDimension.Tex2DArray);
            SoftwareTracingShared.ProbeNormalDepth_Intermediate.HTextureAlloc("_ProbeNormalDepth_Intermediate", ProbeRes, GraphicsFormat.R32G32_UInt);
            SoftwareTracingShared.ProbeDiffuse.HTextureAlloc("_ProbeDiffuse", ProbeRes, GraphicsFormat.R8G8B8A8_UNorm);

            // -------------------------------------- REPROJECTION RT -------------------------------------- //
            SoftwareTracingShared.HistoryIndirection.HTextureAlloc("_HistoryIndirection", ProbeRes, GraphicsFormat.R16G16_UInt, textureXrSlices * HConstants.PERSISTENT_HISTORY_SAMPLES, textureDimension: TextureDimension.Tex2DArray);
            SoftwareTracingShared.ReprojectionWeights.HTextureAlloc("_ReprojectionWeights", ProbeRes, GraphicsFormat.R8G8B8A8_UNorm);
            SoftwareTracingShared.PersistentReprojectionWeights.HTextureAlloc("_PersistentReprojectionWeights", ProbeRes, GraphicsFormat.R8G8B8A8_UNorm);
            SoftwareTracingShared.ReprojectionCoord.HTextureAlloc("_ReprojectionCoord", ProbeRes, GraphicsFormat.R16G16_UInt);
            SoftwareTracingShared.PersistentReprojectionCoord.HTextureAlloc("_PersistentReprojectionCoord", ProbeRes, GraphicsFormat.R16G16_UInt);

            // -------------------------------------- SPATIAL PREPASS RT -------------------------------------- //
            SoftwareTracingShared.SpatialOffsetsPacked.HTextureAlloc("_SpatialOffsetsPacked", ProbeRes, GraphicsFormat.R32G32B32A32_UInt, textureXrSlices * 4, textureDimension: TextureDimension.Tex2DArray);
            SoftwareTracingShared.SpatialWeightsPacked.HTextureAlloc("_SpatialWeightsPacked", ProbeRes, GraphicsFormat.R16G16B16A16_UInt, textureXrSlices * 4, textureDimension: TextureDimension.Tex2DArray);

            // -------------------------------------- RESERVOIR RT -------------------------------------- //
            SoftwareTracingShared.ReservoirAtlas.HTextureAlloc("_ReservoirAtlas", ProbeAtlasRes, GraphicsFormat.R32G32B32A32_UInt);
            SoftwareTracingShared.ReservoirAtlas_History.HTextureAlloc("_ReservoirAtlas_History", ProbeAtlasRes, GraphicsFormat.R32G32B32A32_UInt, textureXrSlices * HConstants.PERSISTENT_HISTORY_SAMPLES, textureDimension: TextureDimension.Tex2DArray);
            SoftwareTracingShared.ReservoirAtlasRadianceData_A.HTextureAlloc("_ReservoirAtlasRadianceData_A", ProbeAtlasRes, GraphicsFormat.R32G32_UInt);
            SoftwareTracingShared.ReservoirAtlasRadianceData_B.HTextureAlloc("_ReservoirAtlasRadianceData_B", ProbeAtlasRes, GraphicsFormat.R32G32_UInt);
            SoftwareTracingShared.ReservoirAtlasRadianceData_C.HTextureAlloc("_ReservoirAtlasRadianceData_C", ProbeAtlasRes, GraphicsFormat.R32G32_UInt);
            SoftwareTracingShared.ReservoirAtlasRayData_A.HTextureAlloc("_ReservoirAtlasRayData_A", ProbeAtlasRes, GraphicsFormat.R32_UInt);
            SoftwareTracingShared.ReservoirAtlasRayData_B.HTextureAlloc("_ReservoirAtlasRayData_B", ProbeAtlasRes, GraphicsFormat.R32_UInt, textureXrSlices * HConstants.PERSISTENT_HISTORY_SAMPLES, textureDimension: TextureDimension.Tex2DArray);
            SoftwareTracingShared.ReservoirAtlasRayData_C.HTextureAlloc("_ReservoirAtlasRayData_C", ProbeAtlasRes, GraphicsFormat.R32_UInt);

            // -------------------------------------- SHADOW GUIDANCE MASK RT -------------------------------------- //
            SoftwareTracingShared.ShadowGuidanceMask.HTextureAlloc("_ShadowGuidanceMask", ProbeAtlasRes, GraphicsFormat.R8_UNorm);
            SoftwareTracingShared.ShadowGuidanceMask_Accumulated.HTextureAlloc("_ShadowGuidanceMask_Accumulated", ProbeAtlasRes, GraphicsFormat.R8_UNorm);
            SoftwareTracingShared.ShadowGuidanceMask_Filtered.HTextureAlloc("_ShadowGuidanceMask_Filtered", ProbeAtlasRes, GraphicsFormat.R8_UNorm);
            SoftwareTracingShared.ShadowGuidanceMask_History.HTextureAlloc("_ShadowGuidanceMask_History", ProbeAtlasRes, GraphicsFormat.R8_UNorm);
            SoftwareTracingShared.ShadowGuidanceMask_CheckerboardHistory.HTextureAlloc("_ShadowGuidanceMask_CheckerboardHistory", ProbeAtlasRes, GraphicsFormat.R8_UNorm);
            SoftwareTracingShared.ShadowGuidanceMask_Samplecount.HTextureAlloc("_ShadowGuidanceMask_Samplecount", ProbeRes, GraphicsFormat.R16_SFloat);
            SoftwareTracingShared.ShadowGuidanceMask_SamplecountHistory.HTextureAlloc("_ShadowGuidanceMask_SamplecountHistory", ProbeRes, GraphicsFormat.R16_SFloat);

            // -------------------------------------- INTERPOLATION RT -------------------------------------- //
            SoftwareTracingShared.PackedSH_A.HTextureAlloc("_PackedSH_A", ProbeRes, GraphicsFormat.R32G32B32A32_UInt);
            SoftwareTracingShared.PackedSH_B.HTextureAlloc("_PackedSH_B", ProbeRes, GraphicsFormat.R32G32B32A32_UInt);
            SoftwareTracingShared.Radiance_Interpolated.HTextureAlloc("_Radiance_Interpolated", FullRes, GraphicsFormat.R32_UInt);

            // -------------------------------------- TEMPORAL DENOISER RT -------------------------------------- //
            SoftwareTracingShared.RadianceAccumulated.HTextureAlloc("_RadianceAccumulated", FullRes, GraphicsFormat.R16G16B16A16_SFloat);
            SoftwareTracingShared.RadianceAccumulated_History.HTextureAlloc("_RadianceAccumulated_History", FullRes, GraphicsFormat.R16G16B16A16_SFloat);
            SoftwareTracingShared.LuminanceDelta.HTextureAlloc("_RadianceLumaDelta", FullRes, GraphicsFormat.R16_SFloat);
            SoftwareTracingShared.LuminanceDelta_History.HTextureAlloc("_RadianceLumaDelta_History", FullRes, GraphicsFormat.R16_SFloat);
        }

        internal static void AllocateSSAO_RT(bool onlyRelease = false)
        {
            void ReleaseTextures()
            {
                SoftwareTracingShared.ProbeSSAO.HRelease();
                SoftwareTracingShared.NormalDepthHalf.HRelease();
                SoftwareTracingShared.BentNormalsAO.HRelease();
                SoftwareTracingShared.BentNormalsAO_Interpolated.HRelease();
                SoftwareTracingShared.BentNormalsAO_History.HRelease();
                SoftwareTracingShared.BentNormalsAO_Accumulated.HRelease();
                SoftwareTracingShared.BentNormalsAO_Samplecount.HRelease();
                SoftwareTracingShared.BentNormalsAO_SamplecountHistory.HRelease();
            }

            if (onlyRelease)
            {
                ReleaseTextures();
                return;
            }

            ReleaseTextures();

            if (SoftwareTracingShared.UseDirectionalOcclusion == false)
                return;

            Vector2 fullRes = Vector2.one;
            Vector2 halfRes = Vector2.one / 2;
            Vector2 probeSize = Vector2.one / HSettings.GeneralSettings.RayCountMode.ParseToProbeSize();

            if (HSettings.ScreenSpaceLightingSettings.DirectionalOcclusion)
            {
                SoftwareTracingShared.ProbeSSAO.HTextureAlloc("_ProbeSSAO", probeSize, GraphicsFormat.R8_UNorm);
                SoftwareTracingShared.BentNormalsAO.HTextureAlloc("_BentNormalsAO", fullRes, GraphicsFormat.R16G16B16A16_SFloat);
                SoftwareTracingShared.BentNormalsAO_Interpolated.HTextureAlloc("_BentNormalsAO_Interpolated", fullRes, GraphicsFormat.R16G16B16A16_SFloat);
                SoftwareTracingShared.NormalDepthHalf.HTextureAlloc("_NormalDepthHalf", fullRes / 2, GraphicsFormat.R16G16B16A16_SFloat);
            }
            else
            {
                SoftwareTracingShared.ProbeSSAO.HTextureAlloc("_ProbeSSAO", 1, 1, GraphicsFormat.R8_UNorm, 1);
                SoftwareTracingShared.BentNormalsAO.HTextureAlloc("_BentNormalsAO", 1, 1, GraphicsFormat.R16G16B16A16_SFloat, 1);
                SoftwareTracingShared.BentNormalsAO_Interpolated.HTextureAlloc("_BentNormalsAO_Interpolated", 1, 1, GraphicsFormat.R16G16B16A16_SFloat, 1);
                SoftwareTracingShared.NormalDepthHalf.HTextureAlloc("_NormalDepthHalf", 1, 1, GraphicsFormat.R16G16B16A16_SFloat, 1);
            }
        }

        internal static void AllocateDebugRT(bool onlyRelease = false)
        {
            void ReleaseTextures()
            {
                SoftwareTracingShared.DebugOutput.HRelease();
            }

            if (onlyRelease)
            {
                ReleaseTextures();
                return;
            }

            ReleaseTextures();

            if (HSettings.GeneralSettings.DebugModeWS == DebugModeWS.None)
                return;

            Vector2 fullRes = Vector2.one;

            if (HSettings.GeneralSettings.DebugModeWS != DebugModeWS.None)
            {
                SoftwareTracingShared.DebugOutput.HTextureAlloc("_DebugOutput", fullRes, GraphicsFormat.B10G11R11_UFloatPack32);
            }
        }

        internal static void AllocateIndirectionBuffers(bool onlyRelease = false)
        {
            void ReleaseTextures()
            {
                SoftwareTracingShared.IndirectCoordsSS.HRelease();
                SoftwareTracingShared.IndirectCoordsWS.HRelease();
                SoftwareTracingShared.IndirectCoordsOV.HRelease();
                SoftwareTracingShared.IndirectCoordsSF.HRelease();
                SoftwareTracingShared.IndirectCoordsSS = null;
                SoftwareTracingShared.IndirectCoordsWS = null;
                SoftwareTracingShared.IndirectCoordsOV = null;
                SoftwareTracingShared.IndirectCoordsSF = null;
            }

            if (onlyRelease)
            {
                ReleaseTextures();
                return;
            }

            ReleaseTextures();

            int textureXrSlices = 1;

            if (SoftwareTracingShared.IndirectCoordsSS == null) SoftwareTracingShared.IndirectCoordsSS = new HDynamicBuffer(BufferType.ComputeBuffer, 2 * sizeof(uint), textureXrSlices, avoidDownscale: true);
            if (SoftwareTracingShared.IndirectCoordsWS == null) SoftwareTracingShared.IndirectCoordsWS = new HDynamicBuffer(BufferType.ComputeBuffer, 2 * sizeof(uint), textureXrSlices, avoidDownscale: true);
            if (SoftwareTracingShared.IndirectCoordsOV == null) SoftwareTracingShared.IndirectCoordsOV = new HDynamicBuffer(BufferType.ComputeBuffer, 2 * sizeof(uint), textureXrSlices, avoidDownscale: true);
            if (SoftwareTracingShared.IndirectCoordsSF == null) SoftwareTracingShared.IndirectCoordsSF = new HDynamicBuffer(BufferType.ComputeBuffer, 2 * sizeof(uint), textureXrSlices, avoidDownscale: true);
        }

        internal static void AllocationHashBuffers(bool onlyRelease = false)
        {
            void ReleaseTextures()
            {
                SoftwareTracingShared.HashBuffer_Key.HRelease();
                SoftwareTracingShared.HashBuffer_Payload.HRelease();
                SoftwareTracingShared.HashBuffer_Counter.HRelease();
                SoftwareTracingShared.HashBuffer_Radiance.HRelease();
                SoftwareTracingShared.HashBuffer_Position.HRelease();
                SoftwareTracingShared.HashBuffer_Key = null;
                SoftwareTracingShared.HashBuffer_Payload = null;
                SoftwareTracingShared.HashBuffer_Counter = null;
                SoftwareTracingShared.HashBuffer_Radiance = null;
                SoftwareTracingShared.HashBuffer_Position = null;
            }

            if (onlyRelease)
            {
                ReleaseTextures();
                return;
            }

            ReleaseTextures();

            if (SoftwareTracingShared.HashBuffer_Key == null) SoftwareTracingShared.HashBuffer_Key = new ComputeBuffer(HConstants.HASH_STORAGE_SIZE, 1 * sizeof(uint));
            if (SoftwareTracingShared.HashBuffer_Payload == null) SoftwareTracingShared.HashBuffer_Payload = new ComputeBuffer(HConstants.HASH_STORAGE_SIZE / HConstants.HASH_UPDATE_FRACTION, 2 * sizeof(uint));
            if (SoftwareTracingShared.HashBuffer_Counter == null) SoftwareTracingShared.HashBuffer_Counter = new ComputeBuffer(HConstants.HASH_STORAGE_SIZE, 1 * sizeof(uint));
            if (SoftwareTracingShared.HashBuffer_Position == null) SoftwareTracingShared.HashBuffer_Position = new ComputeBuffer(HConstants.HASH_STORAGE_SIZE, 4 * sizeof(uint));
            if (SoftwareTracingShared.HashBuffer_Radiance == null)
            {
                SoftwareTracingShared.HashBuffer_Radiance = new ComputeBuffer(HConstants.HASH_STORAGE_SIZE, 4 * sizeof(uint));
                uint[] zeroArray = new uint[HConstants.HASH_STORAGE_SIZE * 4];
                SoftwareTracingShared.HashBuffer_Radiance.SetData(zeroArray);
            }
        }

        public override void Execute(ScriptableRenderContext renderContext, ref RenderingData renderingData)
        {

            var camera = renderingData.cameraData.camera;
            int width = renderingData.cameraData.cameraTargetDescriptor.width;
            int height = renderingData.cameraData.cameraTargetDescriptor.height;

            if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
                return;

            var cmd = CommandBufferPool.Get(HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME);

            try
            {
                ReallocateConditions(width, height);

#if UNITY_EDITOR
                if (SceneViewDrawModeTracker.IsUnlit && camera.cameraType == CameraType.SceneView)
                    return;
#endif
                var renderer = renderingData.cameraData.renderer as UniversalRenderer;

                // URP中通过RTHandle获取ColorBuffer
                // 对应HDRP的ctx.cameraColorBuffer
                RTHandle cameraColorBuffer = renderer.cameraColorTargetHandle;

                // URP中没有HDRP的GetPreviousFrameRT，通过ColorPreviousFrame RT手动管理历史帧
                // 对应HDRP的ctx.hdCamera.GetPreviousFrameRT(HDCameraFrameHistoryType.ColorBufferMipChain)
                RenderTexture previousColorBuffer = SoftwareTracingShared.ColorPreviousFrame.rt;

                // URP中Forward模式下diffuseBuffer为null
                // URP没有Deferred模式的GBuffer0，统一为null或从全局纹理获取
                Texture diffuseBuffer = Shader.GetGlobalTexture(HShaderParams.g_HTraceGBuffer0);

                SoftwareTracingShared.Execute(cmd, camera, width, height, cameraColorBuffer, previousColorBuffer, diffuseBuffer);
                SoftwareTracingShared.History.Update();

                renderContext.ExecuteCommandBuffer(cmd);
            }
            finally
            {
                CommandBufferPool.Release(cmd);
            }
        }

        internal void ReallocateConditions(int cameraWidth, int cameraHeight)
        {
            if (SoftwareTracingShared.History.DirectionalOcclusion != SoftwareTracingShared.UseDirectionalOcclusion)
                AllocateSSAO_RT(!SoftwareTracingShared.UseDirectionalOcclusion);

            if (SoftwareTracingShared.History.DebugModeEnabled == (HSettings.GeneralSettings.DebugModeWS == DebugModeWS.None))
                AllocateDebugRT(HSettings.GeneralSettings.DebugModeWS == DebugModeWS.None);

            if (SoftwareTracingShared.History.TracingMode != HSettings.GeneralSettings.TracingMode
                || SoftwareTracingShared.History.RayCountMode != HSettings.GeneralSettings.RayCountMode
               )
            {
                Allocation(HSettings.GeneralSettings.TracingMode == Globals.TracingMode.HardwareTracing);
            }

            if (HSettings.GeneralSettings.TracingMode == Globals.TracingMode.SoftwareTracing)
            {
                var newResolution = new Vector2Int(cameraWidth, cameraHeight);
                SoftwareTracingShared.IndirectCoordsSS?.ReAllocIfNeeded(newResolution);
                SoftwareTracingShared.IndirectCoordsWS?.ReAllocIfNeeded(newResolution);
                SoftwareTracingShared.IndirectCoordsOV?.ReAllocIfNeeded(newResolution);
                SoftwareTracingShared.IndirectCoordsSF?.ReAllocIfNeeded(newResolution);
            }
        }

        public void Cleanup()
        {
            Allocation(true);
            _initialized = false;
        }
    }
}