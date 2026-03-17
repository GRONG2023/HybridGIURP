#ifndef REFLECTION_GI_INCLUDED
#define REFLECTION_GI_INCLUDED

#include "../Includes/VoxelTraversal.hlsl"
#include "../Includes/VoxelLightingEvaluation.hlsl"
#include "../Includes/SpatialHash.hlsl"


uint _HTraceReflectionsGI_JitterRadius;
uint _HTraceReflectionsGI_TemporalJitter;
uint _HTraceReflectionsGI_SpatialFilteringRadius;

float _HTraceReflectionsGI_RayBias;
float _HTraceReflectionsGI_MaxRayLength;

float3 HTraceIndirectLighting(uint2 pixCoord, float3 RayOriginWS, float3 NormalWS, float3 DiffuseColor)
{
	//TORELEASE: patreoncode, delete, regionstart
	float3 IndirectLighting = 0;
	float3 AbsolutePositionWS = GetAbsolutePositionWS(RayOriginWS);
	
	#ifdef GI_APPROXIMATION_IN_REFLECTIONS

	// Calculate voxel coord from position
	int3 VoxelBoxCenter = int3(_VoxelResolution.xzy / 2);
	int3 VoxelPosition = floor((AbsolutePositionWS - _VoxelCameraPos) / _VoxelSize);
	float3 VoxelCoord =  VoxelBoxCenter + VoxelPosition;
	
	// Load normal
	float3 NormalAbs = abs(NormalWS);
	float NormalMax = max(max(NormalAbs.x, NormalAbs.y), NormalAbs.z);
	
	float3 CacheNormal = 0;
	float3 CacheNormalSign = 0;
	float3 GatherDirection = 0;
	
	if (NormalMax == NormalAbs.x)
	{
		CacheNormal = float3(1,0,0);
		GatherDirection = float3(0,1,1);
		CacheNormalSign.x = sign(NormalWS.x);
	}
	if (NormalMax == NormalAbs.y)
	{
		CacheNormal = float3(0,1,0);
		GatherDirection = float3(1,0,1);
		CacheNormalSign.y = sign(NormalWS.y);
	
	}
	if (NormalMax == NormalAbs.z)
	{
		CacheNormal = float3(0,0,1);
		GatherDirection = float3(1,1,0);
		CacheNormalSign.z = sign(NormalWS.z);
	}
	
	// Offsets used for gather
	int2 Offsets5x5[25] =  {int2( 0,  0), int2( 0,  1),	int2( 1,  0), int2( 1,  1),	int2(-1,  0),
							int2(-1,  1), int2(-1, -1),	int2( 0, -1), int2( 1, -1),	int2(-2,  0),
							int2( 0, -2), int2( 2,  0),	int2( 0,  2), int2(-1,  2),	int2(-2,  1),
							int2( 1, -2), int2( 2, -1),	int2(-2, -1), int2(-1, -2),	int2( 2,  1),
							int2( 1,  2), int2(-2,  2),	int2( 2,  2), int2( 2, -2),	int2(-2, -2)};
	
	// Check neighbours for occlusions
	#ifdef GI_APPROXIMATION_IN_REFLECTIONS_OCCLUSION_CHECK
	{
		int2 Offsets3x3[12] = {	int2( 0,  1), int2( 1,  0), int2( 1,  1), int2(-1,  0),
			int2(-1,  1), int2(-1, -1), int2( 0, -1), int2( 1, -1),
			int2(-2,  0), int2( 2,  0), int2( 0, -2), int2( 0,  2) };
	 	
		for (int i = 0; i < 12; i++)
		{
			float3 Offset;
			Offset.x = Offsets3x3[i].x;
			Offset.y = Offsets3x3[i].y;
			Offset.z = Offsets3x3[i][GatherDirection.x];
	
			Offset.x *= GatherDirection.x;
			Offset.y *= GatherDirection.y;
			Offset.z *= GatherDirection.z;
	 		
			uint VoxelOccupancy = asuint(H_LOAD3D_LOD(_VoxelPositionPyramid, VoxelCoord + Offset + float3(1,1,1) * CacheNormal * CacheNormalSign, 0));
	 	
			if (VoxelOccupancy > 0)
			{
				Offsets5x5[i + 1] = 0; 
	 		
				if (i == 0) { Offsets5x5[13] = 0; Offsets5x5[12] = 0; Offsets5x5[20] = 0; }
				if (i == 1) { Offsets5x5[19] = 0; Offsets5x5[11] = 0; Offsets5x5[16] = 0; }
				if (i == 2) { Offsets5x5[20] = 0; Offsets5x5[22] = 0; Offsets5x5[19] = 0; }
				if (i == 3) { Offsets5x5[14] = 0; Offsets5x5[ 9] = 0; Offsets5x5[17] = 0; }
				if (i == 4) { Offsets5x5[13] = 0; Offsets5x5[21] = 0; Offsets5x5[14] = 0; }
				if (i == 5) { Offsets5x5[17] = 0; Offsets5x5[24] = 0; Offsets5x5[18] = 0; }
				if (i == 6) { Offsets5x5[18] = 0; Offsets5x5[10] = 0; Offsets5x5[15] = 0; }
				if (i == 7) { Offsets5x5[15] = 0; Offsets5x5[23] = 0; Offsets5x5[16] = 0; }
	 		
				if (i ==  8) { Offsets5x5[14] = 0; Offsets5x5[ 9] = 0; Offsets5x5[17] = 0; }
				if (i ==  9) { Offsets5x5[19] = 0; Offsets5x5[11] = 0; Offsets5x5[16] = 0; }
				if (i == 10) { Offsets5x5[18] = 0; Offsets5x5[10] = 0; Offsets5x5[15] = 0; }
				if (i == 11) { Offsets5x5[13] = 0; Offsets5x5[12] = 0; Offsets5x5[20] = 0; }
			}	
		}	
	}
	#endif
	
	float3 AccumulateCache = 0;
	float AccumulatedWeight = 0;
	
	// Accumulate radiance cache spatially
	for (int i = 0; i < _HTraceReflectionsGI_SpatialFilteringRadius; i++)  
	{
		int3 SampleCoord = VoxelCoord;
	
		// Apply sample offset
		float3 Offset;
		Offset.x = Offsets5x5[i].x;
		Offset.y = Offsets5x5[i].y;
		Offset.z = Offsets5x5[i][GatherDirection.x];  
	
		// Apply per-pixel jitter
		float PixelJitter = GetBNDSequenceSample(pixCoord.xy, (_HFrameCount % 8) * _HTraceReflectionsGI_TemporalJitter, i);
		Offset.x += Offsets5x5[PixelJitter * _HTraceReflectionsGI_JitterRadius].x;
		Offset.y += Offsets5x5[PixelJitter * _HTraceReflectionsGI_JitterRadius].y;
		Offset.z += Offsets5x5[PixelJitter * _HTraceReflectionsGI_JitterRadius][GatherDirection.x]; 
	
		SampleCoord.x += Offset.x * GatherDirection.x;
		SampleCoord.y += Offset.y * GatherDirection.y;
		SampleCoord.z += Offset.z * GatherDirection.z;
	
		// Get hash cell
		uint HashIndex = HashGetIndex(ComputeRadianceCacheCoord(SampleCoord), PackVoxelNormalIndex(NormalWS));
	
		uint HashIndexFound;
		uint HashKey = PackHashKey(ComputeRadianceCacheCoord(SampleCoord), NormalWS);
		bool HashFound = HashFindValid(HashIndex, HashKey, HashIndexFound);
	
		if (HashFound)
		{
			float3 RadianceCacheSample = UnpackCacheRadianceFull(_HashBuffer_Radiance[HashIndexFound].xyz); 
			AccumulateCache += RadianceCacheSample;
			AccumulatedWeight += 1;
		}
	}
	
	// Normalize
	if (AccumulatedWeight > 0)
		AccumulateCache /= AccumulatedWeight;
	
	IndirectLighting = AccumulateCache;
	#endif
	
	#ifdef GI_TRACING_IN_REFLECTIONS 
	VoxelPayload Payload;
	InitializePayload(Payload);

	uint SampleIndex = _HFrameCount % 16;
	
	float2 RayJitter;
	RayJitter.x = GetBNDSequenceSample(pixCoord.xy, SampleIndex, 0);
	RayJitter.y = GetBNDSequenceSample(pixCoord.yx, SampleIndex, 1);  
    
	float3 RayDirection = SampleHemisphereCosine(RayJitter.x, RayJitter.y, NormalWS);
	AbsolutePositionWS += (_VoxelSize * _HTraceReflectionsGI_RayBias * RayDirection) + (_VoxelSize * _HTraceReflectionsGI_RayBias * NormalWS);
	
	// Calculate ray distance
	float MaxRayDistance = MaxVoxelRayDistance(AbsolutePositionWS, RayDirection.xyz);
	float RayDistance = _HTraceReflectionsGI_MaxRayLength == 0 ? MaxRayDistance : _HTraceReflectionsGI_MaxRayLength;
     
	// Trace into Voxels
	bool HitFound = TraceVoxelsDiffuse(AbsolutePositionWS, RayDirection.xyz, RayDistance, 128, Payload);
	
	if (HitFound)
	{
		// Evauluate lighting on hit point
		EvaluateHitLighting(Payload);
		
		float TotalRayDistance = Payload.HitDistance;
	 	
		uint3 CacheCoord = ComputeRadianceCacheCoord(Payload.HitCoord);
		uint HashKey = PackHashKey(CacheCoord, Payload.HitNormal);

		bool IsEmpty;
		uint HashRank = 2;
		uint HashProbingIndex, HashLowestRankIndex;
		uint HashIndex = HashGetIndex(CacheCoord, PackVoxelNormalIndex(Payload.HitNormal));
		bool HashFound = HashFindAny(HashIndex, HashKey, HashRank, HashLowestRankIndex, HashProbingIndex, IsEmpty);

		int3 VoxelCoordAbsolute = VoxelCoordToAbsoluteVoxelCoord(Payload.HitCoord); 
		float3 VoxelHitOffset = (float3(VoxelCoordAbsolute) * _VoxelSize) - (Payload.HitPosition) ;

		if (HashFound) // If a valid entry was found we reset the decay counter to max value and use cache (it's main purpose)
		{
			uint3 HitCachePacked = _HashBuffer_Radiance[HashProbingIndex].xyz;

			float3 RadianceFullRange = UnpackCacheRadianceFull(HitCachePacked.xyz);
			float3 RadianceNearRange = UnpackCacheRadianceNear(HitCachePacked.xyz);

			// Choose far / near field cache based on the travelled ray distance
			Payload.HitCache = TotalRayDistance > _VoxelSize.x * 4 ? RadianceFullRange : min(RadianceNearRange, RadianceFullRange);

			// Progressively dim cache at a distance smaller than a voxel size
			Payload.HitCache *= lerp(0, 1, saturate(TotalRayDistance / 1 / _VoxelSize));

			// Clip cache
			Payload.HitCache *= GetCurrentExposureMultiplier();
			Payload.HitCache = HClipRadiance(Payload.HitCache, 10);
			Payload.HitCache *= GetInverseCurrentExposureMultiplier();
			
			// Add cache to hit radiance
			Payload.HitColor += Payload.HitCache * Payload.HitDiffuse; 
		}

		IndirectLighting += Payload.HitColor;
	}
	else
	{
		IndirectLighting += SAMPLE_TEXTURECUBE_ARRAY_LOD(_SkyTexture, H_SAMPLER_TRILINEAR_CLAMP, RayDirection, 0, 2).xyz;
	}
	
	#endif

	return IndirectLighting * DiffuseColor;
	//TORELEASE: regionend
	return float3(0, 0, 0);
}
#endif
