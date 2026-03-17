#define VOXEL_MATERIAL_EVALUATION_INCLUDE

#pragma once
#include "../Includes/Config.hlsl"
#include "../Headers/HMain.hlsl"

// ------------------------ STRUCTS ------------------------
struct VoxelSurfaceData
{
	// Inputs
	float2 TexCoord0;
	float2 TexCoord1;
	
	// Outputs
	float3 Color;
	float Alpha;
	float IsEmissive;
};

// ------------------------ FUNCTIONS ------------------------
float3 ClampDiffuseColor(float3 DiffuseColor)
{
	DiffuseColor *= SURFACE_DIFFUSE_INTENSITY;
	
	// DiffuseColor = FastLinearToSRGB(DiffuseColor);
	// DiffuseColor = RgbToHsv(DiffuseColor);
	// DiffuseColor.z = min(DiffuseColor.z, 0.9f);
	// DiffuseColor = HsvToRgb(DiffuseColor);
	// DiffuseColor = FastSRGBToLinear(DiffuseColor);

	return DiffuseColor;
}




// ------------------------ MATERIAL PROPERTIES ------------------------

// Standard Shader properties
float _Cutoff;
float4 _Color;
float4 _MainTex_ST;
H_TEXTURE(_MainTex);
H_SAMPLER(sampler_MainTex); 



// ------------------------ MATERIAL EVALUATION ------------------------
bool EvaluateSurfaceColor(inout VoxelSurfaceData SurfaceData)
{
	float3 DiffuseColor = 0;

	SurfaceData.Color = 0;
	SurfaceData.Alpha = 0;
	SurfaceData.IsEmissive = 0;
	
	if (1) //EVALUATE_LIT)
	{
		float2 TextureCoord = SurfaceData.TexCoord0 * _MainTex_ST.xy + _MainTex_ST.zw;
		float4 LitColor = H_SAMPLE_2D(_MainTex, sampler_MainTex, TextureCoord) * _Color;
		SurfaceData.Alpha = LitColor.w < _Cutoff ? 1 : 0;

		DiffuseColor += LitColor.xyz;
		SurfaceData.Color = LitColor.xyz;
	}

	return true;
}
