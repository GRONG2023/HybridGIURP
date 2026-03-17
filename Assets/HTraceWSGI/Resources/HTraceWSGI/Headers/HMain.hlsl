//pipelinedefine
#define H_HDRP

#ifndef HMAIN_INCLUDED
#define HMAIN_INCLUDED

// TODO: check if we need all these includes or some can be removed?
// --------------------------------- INCLUDE FILES ----------------------------- //

#define SHADOW_LOW
#define AREA_SHADOW_LOW

//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Builtin/BuiltinData.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/TextureXR.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinGIUtilities.hlsl"
//#include "Packages/com.unity.render-pipelines.core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/HDShadow.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
////#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassRenderers.hlsl"  //we can't add it because ReflectionGI get redefenition etc.
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingSampling.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RaytracingSampling.hlsl"

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"




#include "HMath.hlsl"
#include "HPacking.hlsl"

float4x4 _PrevInvViewProjMatrix;

#define UNITY_MATRIX_PREV_I_VP _PrevInvViewProjMatrix

// --------------------------------- INCLUDE FILES BELOW ALSO!!!!!!!!!! ----------------------------- //

// ---------------------------------- MATRICES ----------------------------- //
#define H_MATRIX_PREV_I_VP                  UNITY_MATRIX_PREV_I_VP
#define H_MATRIX_I_VP                       UNITY_MATRIX_I_VP
#define H_MATRIX_VP                         UNITY_MATRIX_VP
#define H_MATRIX_V                          UNITY_MATRIX_V
#define H_MATRIX_I_V                        UNITY_MATRIX_I_V



// --------------------------------- ADDITIONAL INCLUDE FILES ----------------------------- //

#include "HSpaceTransforms.hlsl"

// --------------------------------- VALUES  ----------------------------- //

float4 _HRenderScale;
float4 _HRenderScalePrevious;

int _HFrameCount;
uint _DirectionalLightCount;

TEXTURECUBE(unity_SpecCube2);
SAMPLER(samplerunity_SpecCube2);

struct DirectionalLightData
{
    float3 positionRWS;
    uint lightLayers;
    float lightDimmer;
    float volumetricLightDimmer;
    float3 forward;
    int cookieMode;
    float4 cookieScaleOffset;
    float3 right;
    int shadowIndex;
    float3 up;
    int contactShadowIndex;
    float3 color;
    int contactShadowMask;
    float3 shadowTint;
    float shadowDimmer;
    float volumetricShadowDimmer;
    int nonLightMappedOnly;
    real minRoughness;
    int screenSpaceShadowIndex;
    real4 shadowMaskSelector;
    float diffuseDimmer;
    float specularDimmer;
    float penumbraTint;
    float isRayTracedContactShadow;
    float distanceFromCamera;
    float angularDiameter;
    float flareFalloff;
    float flareCosInner;
    float flareCosOuter;
    float __unused__;
    float3 flareTint;
    float flareSize;
    float3 surfaceTint;
    float4 surfaceTextureScaleOffset;
};


StructuredBuffer<DirectionalLightData> _DirectionalLightDatas;

#define HRenderScale _HRenderScale
#define HRenderScalePrevious _HRenderScalePrevious
//Unity's _RTHandleScale in URP always (1,1,1,1)?





// --------------------------------- CONSTANTS  ----------------------------- //
#define H_TWO_PI (6.28318530718f)
#define H_PI (3.1415926535897932384626433832795)
#define H_PI_HALF (1.5707963267948966192313216916398)



// --------------------------------- TEXTURE READ / WRITE HELPERS ----------------------------- //
#define H_COORD(pixelCoord)     pixelCoord
#define H_INDEX_ARRAY(slot)     slot

// --------------------------------- TEXTURE SAMPLERS ----------------------------- //
#define H_SAMPLER                                   SAMPLER
#define H_SAMPLER_POINT_CLAMP                       sampler_PointClamp
#define H_SAMPLER_LINEAR_CLAMP                      sampler_LinearClamp
#define H_SAMPLER_LINEAR_REPEAT                     sampler_LinearRepeat
#define H_SAMPLER_TRILINEAR_CLAMP                   sampler_LinearClamp
#define H_SAMPLER_TRILINEAR_REPEAT                  sampler_LinearRepeat
#define H_SAMPLER_LINEAR_CLAMP_COMPARE              sampler_LinearClampCompare



// ----------------------------- TEXTURE PROPERTY DECLARATIONS ----------------------------- //

// TEXTURE
#define H_TEXTURE(textureName)                      TEXTURE2D(textureName)
#define H_TEXTURE3D(type, textureName)              Texture3D<type> textureName
#define H_TEXTURE_ARRAY(textureName)                TEXTURE2D_ARRAY(textureName)
#define H_TEXTURE_DX(type, textureName)             Texture2D<type> textureName
#define H_TEXTURE_UINT2(textureName)                TEXTURE2D(textureName)

// RW TEXTURE
#define H_RW_TEXTURE(type, textureName)             RWTexture2D<type> textureName//RWTexture2D<type> textureName
#define H_RW_TEXTURE3D(type, textureName)           RWTexture3D<type> textureName
#define H_RW_TEXTURE_ARRAY(type, textureName)       RWTexture2DArray<type> textureName
#define H_RW_TEXTURE_UINT2(textureName)             RWTexture2D<uint2> textureName


// ----------------------------- TEXTURE FETCH ----------------------------- //
#define H_LOAD(textureName, unCoord2)                                           textureName.Load(int3(unCoord2, 0))
#define H_LOAD_LOD(textureName, unCoord2, lod)                                  textureName.Load(int3(unCoord2,lod))
#define H_LOAD_ARRAY(textureName, unCoord2, index)                              textureName.Load(int4(unCoord2, index, 0))
#define H_LOAD_ARRAY_LOD(textureName, unCoord2, index, lod)                     textureName.Load(int4(unCoord2, index, lod))

// SAMPLE
#define H_SAMPLE(textureName, samplerName, coord2)                                      SAMPLE_TEXTURE2D(textureName, samplerName, coord2)
#define H_SAMPLE_2D(textureName, samplerName, coord2)                                   SAMPLE_TEXTURE2D(textureName, samplerName, coord2)
#define H_SAMPLE_LOD(textureName, samplerName, coord2, lod)                             SAMPLE_TEXTURE2D_LOD(textureName, samplerName, coord2, lod)
#define H_SAMPLE_ARRAY(textureName, samplerName, coord2, index)                         SAMPLE_TEXTURE2D_ARRAY(textureName, samplerName, coord2, index)
#define H_SAMPLE_ARRAY_LOD(textureName, samplerName, coord2, index, lod)                SAMPLE_TEXTURE2D_ARRAY_LOD(textureName, samplerName, coord2, index, lod)
#define H_SAMPLE_SHADOW(textureName, samplerName, coord2)                               SAMPLE_TEXTURE2D_SHADOW(textureName, samplerName, coord2)
#define H_SAMPLE_TEXTURECUBE_ARRAY_LOD(textureName, samplerName, coord3, index, lod)    SAMPLE_TEXTURECUBE_ARRAY_LOD(textureName, samplerName, coord3, index, lod)

// GATHER
#define H_GATHER_RED(textureName, samplerName, coord2, offset)                          textureName.GatherRed(samplerName, coord2, offset)
#define H_GATHER_BLUE(textureName, samplerName, coord2, offset)                         textureName.GatherBlue(samplerName, coord2, offset)
#define H_GATHER_GREEN(textureName, samplerName, coord2, offset)                        textureName.GatherGreen(samplerName, coord2, offset)
#define H_GATHER_ALPHA(textureName, samplerName, coord2, offset)                        textureName.GatherAlpha(samplerName, coord2, offset)

// LOAD 3D
#define H_LOAD3D(textureName, unCoord3)                                                 textureName.Load(int4(unCoord3, 0))
#define H_LOAD3D_LOD(textureName, unCoord3, lod)                                        textureName.Load(int4(unCoord3, lod))

// ------------------------------------- GBUFFER RESOURCES ----------------------------- //
H_TEXTURE(g_HTraceGBuffer0);
H_TEXTURE(g_HTraceGBuffer1);
H_TEXTURE(g_HTraceGBuffer2);
H_TEXTURE(g_HTraceGBuffer3);

H_TEXTURE(g_HTraceDepth);
H_TEXTURE(g_HTraceDepthPyramidWSGI);
H_TEXTURE(g_HTraceColor);
H_TEXTURE(g_HTraceMotionMask);
H_TEXTURE(g_HTraceMotionVectors);



// --------------------------------- GBUFFER FETCH ----------------------------- //
#define HBUFFER_DEPTH(pixCoord)							H_LOAD(g_HTraceDepth, pixCoord).x
#define HBUFFER_NORMAL_WS(pixCoord)						GetNormalWS(pixCoord)


#define HBUFFER_COLOR(pixCoord)							H_LOAD(g_HTraceColor, pixCoord)
#define HBUFFER_DIFFUSE(pixCoord)						H_LOAD(g_HTraceGBuffer0, pixCoord).xyz
#define HBUFFER_MOTION_VECTOR(pixCoord)				    H_LOAD(g_HTraceMotionVectors, pixCoord).xy
#define HBUFFER_MOTION_MASK(pixCoord)					GetMotionMask(pixCoord)
#define HBUFFER_GEOMETRICAL_NORMAL_FROM_DEPTH(pixCoord)             GeometricalNormalFromDepth(pixCoord)

float3 GetNormalWS(uint2 pixCoord)
{
    //float3 Normal = g_HTraceGBuffer2.Load(int3(pixCoord, 0)).xyz;
    float3 Normal = H_LOAD(g_HTraceGBuffer2, pixCoord).xyz;
    float2 OctNormalWS = Unpack888ToFloat2(Normal);
    return UnpackNormalOctQuadEncode(OctNormalWS * 2.0 - 1.0);
}

float3 GeometricalNormalFromDepth(float2 pixCoord)
{
	// Option 1: Protect borders by dilation
	// if (pixCoord.x == 0) pixCoord.x += 2;
	// if (pixCoord.y == 0) pixCoord.y += 2;

	// Option 2: Protect borders by culling out-of-frame samples
	float CullX = 1;
	float CullY = 1;
	if (pixCoord.x == 0) CullX = 0;
	if (pixCoord.y == 0) CullY = 0; 

	// TODO: find out why do we need to protect borders < 0 (Left, Bottom) while going above _ScreenSize is okay (Top, Right)
	
    float DepthC = HBUFFER_DEPTH(pixCoord);

    // Early-out on the sky
    if (DepthC <= 1e-7)
        return 0;
	
    float DepthL = HBUFFER_DEPTH(pixCoord + int2(-1,  0)) * CullX;
    float DepthR = HBUFFER_DEPTH(pixCoord + int2( 1,  0));
    float DepthD = HBUFFER_DEPTH(pixCoord + int2( 0, -1)) * CullY;
    float DepthU = HBUFFER_DEPTH(pixCoord + int2( 0,  1));
    
    float3 WorldPosC = H_COMPUTE_POSITION_WS((pixCoord + 0.5 + float2( 0.0,  0.0)) * _ScreenSize.zw, DepthC, H_MATRIX_I_VP);
    float3 WorldPosL = H_COMPUTE_POSITION_WS((pixCoord + 0.5 + float2(-1.0,  0.0)) * _ScreenSize.zw, DepthL, H_MATRIX_I_VP) * CullX;
    float3 WorldPosR = H_COMPUTE_POSITION_WS((pixCoord + 0.5 + float2( 1.0,  0.0)) * _ScreenSize.zw, DepthR, H_MATRIX_I_VP);
    float3 WorldPosD = H_COMPUTE_POSITION_WS((pixCoord + 0.5 + float2( 0.0, -1.0)) * _ScreenSize.zw, DepthD, H_MATRIX_I_VP) * CullY;
    float3 WorldPosU = H_COMPUTE_POSITION_WS((pixCoord + 0.5 + float2( 0.0,  1.0)) * _ScreenSize.zw, DepthU, H_MATRIX_I_VP);

    float3 L = WorldPosC - WorldPosL;
    float3 R = WorldPosR - WorldPosC;
    float3 D = WorldPosC - WorldPosD;
    float3 U = WorldPosU - WorldPosC;
    
    float4 H = float4(HBUFFER_DEPTH(pixCoord + int2(-1, 0)) * CullX,
                      HBUFFER_DEPTH(pixCoord + int2( 1, 0)),
                      HBUFFER_DEPTH(pixCoord + int2(-2, 0)) * CullX,
                      HBUFFER_DEPTH(pixCoord + int2( 2, 0)));

    float4 V = float4(HBUFFER_DEPTH(pixCoord + int2(0, -1)) * CullY,
                      HBUFFER_DEPTH(pixCoord + int2(0,  1)),
                      HBUFFER_DEPTH(pixCoord + int2(0, -2)) * CullY,
                      HBUFFER_DEPTH(pixCoord + int2(0,  2)));
    
    float2 HE = abs((2 * H.xy - H.zw) - DepthC);
    float2 VE = abs((2 * V.xy - V.zw) - DepthC);
    
    half3 DerivH = HE.x < HE.y ? L : R;
    half3 DerivV = VE.x < VE.y ? D : U;

    return -normalize(cross(DerivH, DerivV));
}


// Exposure texture - 1x1 RG16F (r: exposure mult, g: exposure EV100)
TEXTURE2D(_ExposureTexture);
TEXTURE2D(_PrevExposureTexture);

float GetCurrentExposureMultiplier()
{
    return 0.00651;
    //return LOAD_TEXTURE2D(_ExposureTexture, int2(0, 0)).x;
}

float GetInverseCurrentExposureMultiplier()
{
    float exposure = GetCurrentExposureMultiplier();
    return rcp(exposure + (exposure == 0.0)); // zero-div guard
}


float GetPreviousExposureMultiplier()
{
    return 0.00651;
    //return LOAD_TEXTURE2D(_PrevExposureTexture, int2(0, 0)).x;
    // _ProbeExposureScale is a scale used to perform range compression to avoid saturation of the content of the probes. It is 1.0 if we are not rendering probes.
    //return LOAD_TEXTURE2D(_PrevExposureTexture, int2(0, 0)).x * _ProbeExposureScale;

}

float GetInversePreviousExposureMultiplier()
{
    float exposure = GetPreviousExposureMultiplier();
    return rcp(exposure + (exposure == 0.0)); // zero-div guard
}

// ---------------------------------- OTHER -----------------------------------------
#define HGetInversePreviousExposureMultiplier GetInversePreviousExposureMultiplier()
#define HGetInverseCurrentExposureMultiplier GetInverseCurrentExposureMultiplier()
#define HEnableProbeVolumes _EnableProbeVolumes
#define HGetCurrentExposureMultiplier GetCurrentExposureMultiplier()
#define HGetPreviousExposureMultiplier GetPreviousExposureMultiplier()


#endif // HMAIN_INCLUDED
