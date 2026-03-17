#include "config.hlsl"
#include "VoxelizationCommon.hlsl"
#include "../Includes/LightCluster.hlsl"

#define HAS_LIGHTLOOP

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
///#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/PunctualLightCommon.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightEvaluation.hlsl"


// Since we use slope-scale bias, the constant bias is for now set as a small fixed value
#define FIXED_UNIFORM_BIAS (1.0f / 65536.0f)

#pragma warning (disable : 3206)

StructuredBuffer<float3> _PrevBuffer;
StructuredBuffer<uint> _LightClusterCounterBuffer;
StructuredBuffer<uint> _LightClusterIndexesBuffer;

H_TEXTURE_DX(float, g_HTraceShadowmap);
float4x4 g_DirLightMatrix;
float4 _PreviousCameraPosition;

float4 H_SHAr;
float4 H_SHAg;
float4 H_SHAb;
float4 H_SHBr;
float4 H_SHBg;
float4 H_SHBb;
float4 H_SHC;



float3 EvaluateSky(float3 Direction)
{
	
    return H_SAMPLE_TEXTURECUBE_ARRAY_LOD(unity_SpecCube2, H_SAMPLER_TRILINEAR_CLAMP, Direction.xyz, 0, 2).xyz * SKY_LIGHT_INTENSITY;
//  return H_SAMPLE_TEXTURECUBE_ARRAY_LOD(_SkyTexture, H_SAMPLER_TRILINEAR_CLAMP, Direction.xyz, 0, 2).xyz * SKY_LIGHT_INTENSITY;
  
}


float EvaluateShadowmap(float3 PositionTC)
{
  return H_SAMPLE_SHADOW(g_HTraceShadowmap, H_SAMPLER_LINEAR_CLAMP_COMPARE, PositionTC).x;
  
}

float EvaluateDirectionalShadowOcclusion(float3 WorldPos)
{
  if (!EVALUATE_SKY_OCCLUSION)
    return 1;
  
  // Calculate shadowmap coordinates
  float4 PosCS = mul(g_DirLightMatrix, float4(WorldPos, 1.0));
  float3 PosTC = float3(saturate(PosCS.xy * 0.5f + 0.5f), PosCS.z);

  if (any(PosTC < 0.0f) || any(PosTC > 1.0f))
    return 1.0f;

  float SkyOcclusion = 0;
  SkyOcclusion = EvaluateShadowmap(PosTC.xyz);
  return lerp(SkyOcclusion, 1.0f,  MINIMAL_SKY_LIGHTING);
}

float EvaluateDirectionalShadowHDRP(float3 LightDirection, float3 WorldPos, float3 Normal)
{
  // Initialize shadow to 1
  float DirectionalShadow = 1.0f;

  // Normal *= FastSign(dot(Normal, DirLightDirection)); //TODO: do we need this?

  // Detect if we can early out on zero dot product
  float NdotL = saturate(dot(Normal, LightDirection));
  if (NdotL == 0)
    return 0;

  // Calculate normal bias
  float WorldTexelSize = 0.025f; // ~for 2048 shadowmap;
  float NormalBias = 1.5f; //_VoxelSize * 10.0f * 3.5f; // 1.5f;
  WorldPos += Normal * NormalBias * WorldTexelSize * lerp(0.35, 1, NdotL);

  // Calculate shadowmap coordinates
  float4 PosCS = mul(g_DirLightMatrix, float4(WorldPos, 1.0));
  float3 PosTC = float3(saturate(PosCS.xy * 0.5f + 0.5f), PosCS.z);
  PosTC.z += FIXED_UNIFORM_BIAS;
  
  DirectionalShadow *= NdotL;
  DirectionalShadow *= EvaluateShadowmap(PosTC.xyz);
  
  return DirectionalShadow;
}

bool EvaluateHitLighting(inout VoxelPayload Payload)
{
  bool IsEmissive = false;
  Payload.HitDiffuse = UnpackVoxelColor(asuint(H_LOAD3D(_VoxelData, Payload.HitCoord)), IsEmissive);
  
  Payload.HitColor = 0;
  float DirectionalLightShadow = 1;

	if (_DirectionalLightCount > 0)
	{
		DirectionalLightData DirectionalLightData = _DirectionalLightDatas[0];
		DirectionalLightShadow = EvaluateDirectionalShadowHDRP(-DirectionalLightData.forward, Payload.HitPosition, Payload.HitNormal);
		
		Payload.HitColor += DirectionalLightData.color * DIRECTIONAL_LIGHT_INTENSITY;
		Payload.HitColor *= DirectionalLightShadow;
	}
	
	Payload.HitColor *= Payload.HitDiffuse;
	Payload.HitColor /= H_PI;
	

  
  if (IsEmissive)
  {
    Payload.HitColor = Payload.HitDiffuse;
    Payload.HitColor *= HGetInverseCurrentExposureMultiplier;
  }

  //Payload.HitColor = 0;
  return DirectionalLightShadow == 0 ? false : true;
}

  
