#define NUM_TEX_COORD_INTERPOLATORS 1
#define NUM_MATERIAL_TEXCOORDS_VERTEX 1
#define NUM_CUSTOM_VERTEX_INTERPOLATORS 0

struct Input
{
	//float3 Normal;
	float2 uv_MainTex : TEXCOORD0;
	float2 uv2_Material_Texture2D_0 : TEXCOORD1;
	float4 color : COLOR;
	float4 tangent;
	//float4 normal;
	float3 viewDir;
	float4 screenPos;
	float3 worldPos;
	//float3 worldNormal;
	float3 normal2;
};
struct SurfaceOutputStandard
{
	float3 Albedo;		// base (diffuse or specular) color
	float3 Normal;		// tangent space normal, if written
	half3 Emission;
	half Metallic;		// 0=non-metal, 1=metal
	// Smoothness is the user facing name, it should be perceptual smoothness but user should not have to deal with it.
	// Everywhere in the code you meet smoothness it is perceptual smoothness
	half Smoothness;	// 0=rough, 1=smooth
	half Occlusion;		// occlusion (default 1)
	float Alpha;		// alpha for transparencies
};

//#define HDRP 1
#define URP 1
#define UE5
#define HAS_CUSTOMIZED_UVS 1
#define MATERIAL_TANGENTSPACENORMAL 1
//struct Material
//{
	//samplers start
SAMPLER( SamplerState_Linear_Repeat );
SAMPLER( SamplerState_Linear_Clamp );
TEXTURE2D(       _NormalMap );
SAMPLER(  sampler_NormalMap );
TEXTURE2D(       _EmissiveColorMap );
SAMPLER(  sampler_EmissiveColorMap );
TEXTURE2D(       _BaseColorMap );
SAMPLER(  sampler_BaseColorMap );
CBUFFER_START(UnityPerMaterial)
float4 _NormalMap_TexelSize;
float4 _NormalMap_ST;
float4 _EmissiveColorMap_TexelSize;
float4 _EmissiveColorMap_ST;
float4 _BaseColorMap_TexelSize;
float4 _BaseColorMap_ST;
uniform float4 SelectionColor;
uniform float AtlasSize;
uniform float AtlasTileSize;
uniform float Padding;
uniform float SpecPosCount;
CBUFFER_END

//};

#ifdef UE5
	#define UE_LWC_RENDER_TILE_SIZE			2097152.0
	#define UE_LWC_RENDER_TILE_SIZE_SQRT	1448.15466
	#define UE_LWC_RENDER_TILE_SIZE_RSQRT	0.000690533954
	#define UE_LWC_RENDER_TILE_SIZE_RCP		4.76837158e-07
	#define UE_LWC_RENDER_TILE_SIZE_FMOD_PI		0.673652053
	#define UE_LWC_RENDER_TILE_SIZE_FMOD_2PI	0.673652053
	#define INVARIANT(X) X
	#define PI 					(3.1415926535897932)

	#include "LargeWorldCoordinates.hlsl"
#endif
struct MaterialStruct
{
	float4 PreshaderBuffer[4];
	float4 ScalarExpressions[1];
	float VTPackedPageTableUniform[2];
	float VTPackedUniform[1];
};
#define SVTPackedUniform VTPackedUniform
static SamplerState View_MaterialTextureBilinearWrapedSampler;
static SamplerState View_MaterialTextureBilinearClampedSampler;
struct ViewStruct
{
	float GameTime;
	float RealTime;
	float DeltaTime;
	float PrevFrameGameTime;
	float PrevFrameRealTime;
	float MaterialTextureMipBias;	
	float4 PrimitiveSceneData[ 40 ];
	float4 TemporalAAParams;
	float2 ViewRectMin;
	float4 ViewSizeAndInvSize;
	float2 ResolutionFractionAndInv;
	float MaterialTextureDerivativeMultiply;
	uint StateFrameIndexMod8;
	uint StateFrameIndex;
	float FrameNumber;
	float2 FieldOfViewWideAngles;
	float4 RuntimeVirtualTextureMipLevel;
	float PreExposure;
	float4 BufferBilinearUVMinMax;
    float OneOverPreExposure;
};
struct ResolvedViewStruct
{
	#ifdef UE5
		FLWCVector3 WorldCameraOrigin;
		FLWCVector3 PrevWorldCameraOrigin;
		FLWCVector3 PreViewTranslation;
		FLWCVector3 WorldViewOrigin;
	#else
		float3 WorldCameraOrigin;
		float3 PrevWorldCameraOrigin;
		float3 PreViewTranslation;
		float3 WorldViewOrigin;
	#endif
	float4 ScreenPositionScaleBias;
	float4x4 TranslatedWorldToView;
	float4x4 TranslatedWorldToCameraView;
	float4x4 TranslatedWorldToClip;
	float4x4 ViewToTranslatedWorld;
	float4x4 PrevViewToTranslatedWorld;
	float4x4 CameraViewToTranslatedWorld;
	float4 BufferBilinearUVMinMax;
	float4 XRPassthroughCameraUVs[ 2 ];
};
struct PrimitiveStruct
{
	float4x4 WorldToLocal;
	float4x4 LocalToWorld;
};

static ViewStruct View;
static ResolvedViewStruct ResolvedView;
static PrimitiveStruct Primitive;
uniform float4 View_BufferSizeAndInvSize;
uniform float4 LocalObjectBoundsMin;
uniform float4 LocalObjectBoundsMax;
static SamplerState Material_Wrap_WorldGroupSettings;
static SamplerState Material_Clamp_WorldGroupSettings;

#include "UnrealCommon.cginc"

static MaterialStruct Material;
void InitializeExpressions()
{
	Material.PreshaderBuffer[0] = float4(0.062012,0.249512,0.061523,0.061523);//(Unknown)
	Material.PreshaderBuffer[1] = float4(0.000000,4096.000000,16.000000,0.062500);//(Unknown)
	Material.PreshaderBuffer[2] = float4(0.062500,0.061523,0.000488,0.000488);//(Unknown)
	Material.PreshaderBuffer[3] = float4(0.000000,0.000000,0.000000,0.000000);//(Unknown)

	Material.PreshaderBuffer[0].xy = Append((AtlasTileSize - (Padding - 1) * 2) / AtlasSize, (AtlasTileSize * 4 - (Padding - 1) * 2) / AtlasSize);
	Material.PreshaderBuffer[0].zw = Append((AtlasTileSize - Padding * 2) / AtlasSize, (AtlasTileSize - Padding * 2) / AtlasSize);
	Material.PreshaderBuffer[1].x = SpecPosCount;
	Material.PreshaderBuffer[1].y = AtlasSize;
	Material.PreshaderBuffer[1].z = AtlasSize / AtlasTileSize;
	Material.PreshaderBuffer[1].w = 1 / (AtlasSize / AtlasTileSize);
	Material.PreshaderBuffer[2].x = rcp( AtlasSize / AtlasTileSize );
	Material.PreshaderBuffer[2].y = (AtlasTileSize - Padding * 2) / AtlasSize;
	Material.PreshaderBuffer[2].zw = Append(Padding / AtlasSize, Padding / AtlasSize);
	Material.PreshaderBuffer[3].x = SelectionColor.w;
	Material.PreshaderBuffer[3].yzw = SelectionColor.xyz;
}
float3 GetMaterialWorldPositionOffset(FMaterialVertexParameters Parameters)
{
SHADER_PUSH_WARNINGS_STATE
SHADER_DISABLE_WARNINGS
	return MaterialFloat3(0.00000000,0.00000000,0.00000000);;
SHADER_POP_WARNINGS_STATE
}
void CalcPixelMaterialInputs(in out FMaterialPixelParameters Parameters, in out FPixelMaterialInputs PixelMaterialInputs)
{
	float3 WorldNormalCopy = Parameters.WorldNormal;

SHADER_PUSH_WARNINGS_STATE
SHADER_DISABLE_WARNINGS
	// Initial calculations (required for Normal)
	MaterialFloat2 Local0 = Parameters.TexCoords[0].xy;
	MaterialFloat2 Local1 = DDX((float2)DERIV_BASE_VALUE(Local0));
	MaterialFloat2 Local2 = (DERIV_BASE_VALUE(Local1) * Material.PreshaderBuffer[0].xy);
	MaterialFloat2 Local3 = (DERIV_BASE_VALUE(Local1) * Material.PreshaderBuffer[0].zw);
	MaterialFloat4 Local4 = Parameters.VertexColor;
	MaterialFloat Local5 = DERIV_BASE_VALUE(Local4).a;
	MaterialFloat Local6 = (DERIV_BASE_VALUE(Local5) * 255.00000000);
	MaterialFloat Local7 = round(DERIV_BASE_VALUE(Local6));
	MaterialFloat Local8 = step(Material.PreshaderBuffer[1].x,Local7);
	MaterialFloat2 Local9 = lerp(DERIV_BASE_VALUE(Local2),DERIV_BASE_VALUE(Local3),Local8);
	MaterialFloat2 Local10 = (((MaterialFloat2)Material.PreshaderBuffer[1].y) * DERIV_BASE_VALUE(Local9));
	MaterialFloat Local11 = length(DERIV_BASE_VALUE(Local10));
	MaterialFloat2 Local12 = DDY((float2)DERIV_BASE_VALUE(Local0));
	MaterialFloat2 Local13 = (DERIV_BASE_VALUE(Local12) * Material.PreshaderBuffer[0].xy);
	MaterialFloat2 Local14 = (Material.PreshaderBuffer[0].zw * DERIV_BASE_VALUE(Local12));
	MaterialFloat2 Local15 = lerp(DERIV_BASE_VALUE(Local13),DERIV_BASE_VALUE(Local14),Local8);
	MaterialFloat2 Local16 = (((MaterialFloat2)Material.PreshaderBuffer[1].y) * DERIV_BASE_VALUE(Local15));
	MaterialFloat Local17 = length(DERIV_BASE_VALUE(Local16));
	MaterialFloat Local18 = max(DERIV_BASE_VALUE(Local11),DERIV_BASE_VALUE(Local17));
	MaterialFloat Local19 = max(DERIV_BASE_VALUE(Local18),0.00000100);
	MaterialFloat Local20 = log2(DERIV_BASE_VALUE(Local19));
	MaterialFloat Local21 = min(DERIV_BASE_VALUE(Local20),3.00000000);
	MaterialFloat Local22 = fmod(Local7,Material.PreshaderBuffer[1].z);
	MaterialFloat Local23 = (Material.PreshaderBuffer[1].w * Local22);
	MaterialFloat Local24 = (Local7 * Material.PreshaderBuffer[2].x);
	MaterialFloat Local25 = floor(Local24);
	MaterialFloat Local26 = (Local25 * Material.PreshaderBuffer[1].w);
	MaterialFloat2 Local27 = frac(DERIV_BASE_VALUE(Local0));
	MaterialFloat2 Local28 = (DERIV_BASE_VALUE(Local27) * Material.PreshaderBuffer[0].xy);
	MaterialFloat2 Local29 = (MaterialFloat2(Local23,Local26) + DERIV_BASE_VALUE(Local28));
	MaterialFloat2 Local30 = (((MaterialFloat2)Material.PreshaderBuffer[2].y) * DERIV_BASE_VALUE(Local27));
	MaterialFloat2 Local31 = (MaterialFloat2(Local23,Local26) + DERIV_BASE_VALUE(Local30));
	MaterialFloat2 Local32 = (DERIV_BASE_VALUE(Local31) + Material.PreshaderBuffer[2].zw);
	MaterialFloat2 Local33 = lerp(DERIV_BASE_VALUE(Local29),DERIV_BASE_VALUE(Local32),Local8);
	MaterialFloat Local34 = (DERIV_BASE_VALUE(Local21) + View.MaterialTextureMipBias);
	MaterialFloat Local35 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local33), 0);
	MaterialFloat4 Local36 = ProcessMaterialLinearColorTextureLookup(Texture2DSampleLevel(_NormalMap, sampler_NormalMap, DERIV_BASE_VALUE(Local33), DERIV_BASE_VALUE(Local34)));
	MaterialFloat Local37 = MaterialStoreTexSample(Parameters, Local36, 0);
	MaterialFloat2 Local38 = (MaterialFloat2(Local36.r,Local36.g) * ((MaterialFloat2)2.00000000));
	MaterialFloat2 Local39 = (Local38 - ((MaterialFloat2)1.00000000));
	MaterialFloat Local40 = sign(Local39.r);
	MaterialFloat Local41 = abs(Local39.g);
	MaterialFloat Local42 = (1.00000000 - Local41);
	MaterialFloat Local43 = (Local40 * Local42);
	MaterialFloat Local44 = sign(Local39.g);
	MaterialFloat Local45 = abs(Local39.r);
	MaterialFloat Local46 = (1.00000000 - Local45);
	MaterialFloat Local47 = (Local44 * Local46);
	MaterialFloat Local48 = (Local46 - Local41);
	MaterialFloat Local49 = step(0.00000000,Local48);
	MaterialFloat3 Local50 = lerp(MaterialFloat3(MaterialFloat2(Local43,Local47),Local48),MaterialFloat3(MaterialFloat2(Local39.r,Local39.g),Local48),Local49);
	MaterialFloat3 Local51 = normalize(Local50);

	// The Normal is a special case as it might have its own expressions and also be used to calculate other inputs, so perform the assignment here
	PixelMaterialInputs.Normal = Local51;

SHADER_POP_WARNINGS_STATE

#if TEMPLATE_USES_SUBSTRATE
	Parameters.SubstratePixelFootprint = SubstrateGetPixelFootprint(Parameters.WorldPosition_CamRelative, GetRoughnessFromNormalCurvature(Parameters));
	Parameters.SharedLocalBases = SubstrateInitialiseSharedLocalBases();
	Parameters.SubstrateTree = GetInitialisedSubstrateTree();
#if SUBSTRATE_USE_FULLYSIMPLIFIED_MATERIAL == 1
	Parameters.SharedLocalBasesFullySimplified = SubstrateInitialiseSharedLocalBases();
	Parameters.SubstrateTreeFullySimplified = GetInitialisedSubstrateTree();
#endif
#endif

	// Note that here MaterialNormal can be in world space or tangent space
	float3 MaterialNormal = GetMaterialNormal(Parameters, PixelMaterialInputs);

#if MATERIAL_TANGENTSPACENORMAL

#if FEATURE_LEVEL >= FEATURE_LEVEL_SM4
	// Mobile will rely on only the final normalize for performance
	MaterialNormal = normalize(MaterialNormal);
#endif

	// normalizing after the tangent space to world space conversion improves quality with sheared bases (UV layout to WS causes shrearing)
	// use full precision normalize to avoid overflows
	Parameters.WorldNormal = TransformTangentNormalToWorld(Parameters.TangentToWorld, MaterialNormal);

#else //MATERIAL_TANGENTSPACENORMAL

	Parameters.WorldNormal = normalize(MaterialNormal);

#endif //MATERIAL_TANGENTSPACENORMAL

#if MATERIAL_TANGENTSPACENORMAL || TWO_SIDED_WORLD_SPACE_SINGLELAYERWATER_NORMAL
	// flip the normal for backfaces being rendered with a two-sided material
	Parameters.WorldNormal *= Parameters.TwoSidedSign;
#endif

	Parameters.ReflectionVector = ReflectionAboutCustomWorldNormal(Parameters, Parameters.WorldNormal, false);

#if !PARTICLE_SPRITE_FACTORY
	Parameters.Particle.MotionBlurFade = 1.0f;
#endif // !PARTICLE_SPRITE_FACTORY

SHADER_PUSH_WARNINGS_STATE
SHADER_DISABLE_WARNINGS
	// Now the rest of the inputs
	MaterialFloat Local52 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local33), 1);
	MaterialFloat4 Local53 = ProcessMaterialColorTextureLookup(Texture2DSampleLevel(_EmissiveColorMap, sampler_EmissiveColorMap, DERIV_BASE_VALUE(Local33), DERIV_BASE_VALUE(Local34)));
	MaterialFloat Local54 = MaterialStoreTexSample(Parameters, Local53, 1);
	MaterialFloat3 Local55 = lerp(Local53.rgb,Material.PreshaderBuffer[3].yzw,Material.PreshaderBuffer[3].x);
	MaterialFloat3 Local56 = DERIV_BASE_VALUE(Local4).rgb;
	MaterialFloat Local57 = DERIV_BASE_VALUE(Local56).r;
	MaterialFloat Local58 = DERIV_BASE_VALUE(Local56).g;
	MaterialFloat Local59 = (DERIV_BASE_VALUE(Local57) + DERIV_BASE_VALUE(Local58));
	MaterialFloat Local60 = DERIV_BASE_VALUE(Local56).b;
	MaterialFloat Local61 = (DERIV_BASE_VALUE(Local59) + DERIV_BASE_VALUE(Local60));
	MaterialFloat Local62 = step(0.02000000,DERIV_BASE_VALUE(Local61));
	MaterialFloat3 Local63 = lerp(MaterialFloat3(1.00000000,1.00000000,1.00000000).rgb,DERIV_BASE_VALUE(Local56),Local62);
	MaterialFloat Local64 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local33), 2);
	MaterialFloat4 Local65 = ProcessMaterialColorTextureLookup(Texture2DSampleLevel(_BaseColorMap, sampler_BaseColorMap, DERIV_BASE_VALUE(Local33), DERIV_BASE_VALUE(Local34)));
	MaterialFloat Local66 = MaterialStoreTexSample(Parameters, Local65, 2);
	MaterialFloat3 Local67 = (DERIV_BASE_VALUE(Local63) * Local65.rgb);

	PixelMaterialInputs.EmissiveColor = Local55;
	PixelMaterialInputs.Opacity = 1.00000000;
	PixelMaterialInputs.OpacityMask = 1.00000000;
	PixelMaterialInputs.BaseColor = Local67;
	PixelMaterialInputs.Metallic = Local36.a;
	PixelMaterialInputs.Specular = 0.50000000;
	PixelMaterialInputs.Roughness = Local36.b;
	PixelMaterialInputs.Anisotropy = 0.00000000;
	PixelMaterialInputs.Normal = Local51;
	PixelMaterialInputs.Tangent = MaterialFloat3(1.00000000,0.00000000,0.00000000);
	PixelMaterialInputs.Subsurface = 0;
	PixelMaterialInputs.AmbientOcclusion = Local65.a;
	PixelMaterialInputs.Refraction = 0;
	PixelMaterialInputs.PixelDepthOffset = 0.00000000;
	PixelMaterialInputs.ShadingModel = 1;
	PixelMaterialInputs.FrontMaterial = GetInitialisedSubstrateData();
	PixelMaterialInputs.SurfaceThickness = 0.01000000;
	PixelMaterialInputs.Displacement = -1.00000000;

SHADER_POP_WARNINGS_STATE

#if MATERIAL_USES_ANISOTROPY
	Parameters.WorldTangent = CalculateAnisotropyTangent(Parameters, PixelMaterialInputs);
#else
	Parameters.WorldTangent = 0;
#endif
}
void GetMaterialCustomizedUVs(FMaterialPixelParameters PixelParameters, inout float2 OutTexCoords[NUM_TEX_COORD_INTERPOLATORS])
{
	FMaterialVertexParameters Parameters = (FMaterialVertexParameters)0;
	Parameters.TangentToWorld = PixelParameters.TangentToWorld;
	Parameters.WorldPosition = PixelParameters.AbsoluteWorldPosition;
	Parameters.VertexColor = PixelParameters.VertexColor;
#if NUM_MATERIAL_TEXCOORDS_VERTEX > 0
	int m = min( NUM_MATERIAL_TEXCOORDS_VERTEX, NUM_TEX_COORD_INTERPOLATORS );
	for( int i = 0; i < m; i++ )
	{
		Parameters.TexCoords[i] = PixelParameters.TexCoords[i];
	}
#endif
	Parameters.PrimitiveId = PixelParameters.PrimitiveId;

	MaterialFloat2 Local68 = Parameters.TexCoords[0].xy;
	OutTexCoords[0] = Local68;

}



#define UnityObjectToWorldDir TransformObjectToWorld

void SetupCommonData( int Parameters_PrimitiveId )
{
	View_MaterialTextureBilinearWrapedSampler = SamplerState_Linear_Repeat;
	View_MaterialTextureBilinearClampedSampler = SamplerState_Linear_Clamp;

	Material_Wrap_WorldGroupSettings = SamplerState_Linear_Repeat;
	Material_Clamp_WorldGroupSettings = SamplerState_Linear_Clamp;

	#ifdef FULLSCREEN_SHADERGRAPH
		View.GameTime = View.RealTime = -_Time.y;// _Time is (t/20, t, t*2, t*3)
	#else
		View.GameTime = View.RealTime = _Time.y;// _Time is (t/20, t, t*2, t*3)
	#endif
	
	View.PrevFrameGameTime = View.GameTime - unity_DeltaTime.x;//(dt, 1/dt, smoothDt, 1/smoothDt)
	View.PrevFrameRealTime = View.RealTime;
	View.DeltaTime = unity_DeltaTime.x;
	View.MaterialTextureMipBias = 0.0;
	View.TemporalAAParams = float4( 0, 0, 0, 0 );
	View.ViewRectMin = float2( 0, 0 );
	View.ViewSizeAndInvSize = View_BufferSizeAndInvSize;
	View.ResolutionFractionAndInv = float2( View_BufferSizeAndInvSize.x / View_BufferSizeAndInvSize.y, 1.0 / ( View_BufferSizeAndInvSize.x / View_BufferSizeAndInvSize.y ));
	View.MaterialTextureDerivativeMultiply = 1.0f;
	View.StateFrameIndexMod8 = 0;
	View.FrameNumber = (int)_Time.y;
	View.FieldOfViewWideAngles = float2( PI * 0.42f, PI * 0.42f );//75degrees, default unity
	View.RuntimeVirtualTextureMipLevel = float4( 0, 0, 0, 0 );
	View.PreExposure = 1;
    View.OneOverPreExposure = 1;
	View.BufferBilinearUVMinMax = float4(
		View_BufferSizeAndInvSize.z * ( 0 + 0.5 ),//EffectiveViewRect.Min.X
		View_BufferSizeAndInvSize.w * ( 0 + 0.5 ),//EffectiveViewRect.Min.Y
		View_BufferSizeAndInvSize.z * ( View_BufferSizeAndInvSize.x - 0.5 ),//EffectiveViewRect.Max.X
		View_BufferSizeAndInvSize.w * ( View_BufferSizeAndInvSize.y - 0.5 ) );//EffectiveViewRect.Max.Y

	for( int i2 = 0; i2 < 40; i2++ )
		View.PrimitiveSceneData[ i2 ] = float4( 0, 0, 0, 0 );

	float4x4 LocalToWorld = transpose( UNITY_MATRIX_M );
    LocalToWorld[3] = float4(ToUnrealPos(LocalToWorld[3]), LocalToWorld[3].w);
	float4x4 WorldToLocal = transpose( UNITY_MATRIX_I_M );
	float4x4 ViewMatrix = transpose( UNITY_MATRIX_V );
	float4x4 InverseViewMatrix = transpose( UNITY_MATRIX_I_V );
	float4x4 ViewProjectionMatrix = transpose( UNITY_MATRIX_VP );
	uint PrimitiveBaseOffset = Parameters_PrimitiveId * PRIMITIVE_SCENE_DATA_STRIDE;
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 0 ] = LocalToWorld[ 0 ];//LocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 1 ] = LocalToWorld[ 1 ];//LocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 2 ] = LocalToWorld[ 2 ];//LocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 3 ] = LocalToWorld[ 3 ];//LocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 5 ] = float4( ToUnrealPos( SHADERGRAPH_OBJECT_POSITION ), 100.0 );//ObjectWorldPosition
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 6 ] = WorldToLocal[ 0 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 7 ] = WorldToLocal[ 1 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 8 ] = WorldToLocal[ 2 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 9 ] = WorldToLocal[ 3 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 10 ] = LocalToWorld[ 0 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 11 ] = LocalToWorld[ 1 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 12 ] = LocalToWorld[ 2 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 13 ] = LocalToWorld[ 3 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 18 ] = float4( ToUnrealPos( SHADERGRAPH_OBJECT_POSITION ), 0 );//ActorWorldPosition
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 19 ] = LocalObjectBoundsMax - LocalObjectBoundsMin;//ObjectBounds
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 21 ] = mul( LocalToWorld, float3( 1, 0, 0 ) );
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 23 ] = LocalObjectBoundsMin;//LocalObjectBoundsMin 
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 24 ] = LocalObjectBoundsMax;//LocalObjectBoundsMax

#ifdef UE5
	ResolvedView.WorldCameraOrigin = LWCPromote( ToUnrealPos( _WorldSpaceCameraPos.xyz ) );
	ResolvedView.PreViewTranslation = LWCPromote( float3( 0, 0, 0 ) );
	ResolvedView.WorldViewOrigin = LWCPromote( float3( 0, 0, 0 ) );
#else
	ResolvedView.WorldCameraOrigin = ToUnrealPos( _WorldSpaceCameraPos.xyz );
	ResolvedView.PreViewTranslation = float3( 0, 0, 0 );
	ResolvedView.WorldViewOrigin = float3( 0, 0, 0 );
#endif
	ResolvedView.PrevWorldCameraOrigin = ResolvedView.WorldCameraOrigin;
	ResolvedView.ScreenPositionScaleBias = float4( 1, 1, 0, 0 );
	ResolvedView.TranslatedWorldToView		 = ViewMatrix;
	ResolvedView.TranslatedWorldToCameraView = ViewMatrix;
	ResolvedView.TranslatedWorldToClip		 = ViewProjectionMatrix;
	ResolvedView.ViewToTranslatedWorld		 = InverseViewMatrix;
	ResolvedView.PrevViewToTranslatedWorld = ResolvedView.ViewToTranslatedWorld;
	ResolvedView.CameraViewToTranslatedWorld = InverseViewMatrix;
	ResolvedView.BufferBilinearUVMinMax = View.BufferBilinearUVMinMax;
	Primitive.WorldToLocal = WorldToLocal;
	Primitive.LocalToWorld = LocalToWorld;
}
#define VS_USES_UNREAL_SPACE 1
float3 PrepareAndGetWPO( float4 VertexColor, float3 UnrealWorldPos, float3 UnrealNormal, float4 InTangent,
						 float4 UV0, float4 UV1 )
{
	InitializeExpressions();
	FMaterialVertexParameters Parameters = (FMaterialVertexParameters)0;

	float3 InWorldNormal = UnrealNormal;
	float4 tangentWorld = InTangent;
	tangentWorld.xyz = normalize( tangentWorld.xyz );
	//float3x3 tangentToWorld = CreateTangentToWorldPerVertex( InWorldNormal, tangentWorld.xyz, tangentWorld.w );
	Parameters.TangentToWorld = float3x3( normalize( cross( InWorldNormal, tangentWorld.xyz ) * tangentWorld.w ), tangentWorld.xyz, InWorldNormal );

	
	#ifdef VS_USES_UNREAL_SPACE
		UnrealWorldPos = ToUnrealPos( UnrealWorldPos );
	#endif
	Parameters.WorldPosition = UnrealWorldPos;
	#ifdef VS_USES_UNREAL_SPACE
		Parameters.TangentToWorld[ 0 ] = Parameters.TangentToWorld[ 0 ].xzy;
		Parameters.TangentToWorld[ 1 ] = Parameters.TangentToWorld[ 1 ].xzy;
		Parameters.TangentToWorld[ 2 ] = Parameters.TangentToWorld[ 2 ].xzy;//WorldAligned texturing uses normals that think Z is up
	#endif

	Parameters.VertexColor = VertexColor;

#if NUM_MATERIAL_TEXCOORDS_VERTEX > 0			
	Parameters.TexCoords[ 0 ] = float2( UV0.x, UV0.y );
#endif
#if NUM_MATERIAL_TEXCOORDS_VERTEX > 1
	Parameters.TexCoords[ 1 ] = float2( UV1.x, UV1.y );
#endif
#if NUM_MATERIAL_TEXCOORDS_VERTEX > 2
	for( int i = 2; i < NUM_TEX_COORD_INTERPOLATORS; i++ )
	{
		Parameters.TexCoords[ i ] = float2( UV0.x, UV0.y );
	}
#endif

	Parameters.PrimitiveId = 0;

	SetupCommonData( Parameters.PrimitiveId );

#ifdef UE5
	Parameters.PrevFrameLocalToWorld = MakeLWCMatrix( float3( 0, 0, 0 ), Primitive.LocalToWorld );
#else
	Parameters.PrevFrameLocalToWorld = Primitive.LocalToWorld;
#endif
	
	float3 Offset = float3( 0, 0, 0 );
	Offset = GetMaterialWorldPositionOffset( Parameters );
	#ifdef VS_USES_UNREAL_SPACE
		//Convert from unreal units to unity
		Offset /= float3( 100, 100, 100 );
		Offset = Offset.xzy;
	#endif
	return Offset;
}

void SurfaceReplacement( Input In, out SurfaceOutputStandard o )
{
	InitializeExpressions();

	float3 Z3 = float3( 0, 0, 0 );
	float4 Z4 = float4( 0, 0, 0, 0 );

	float3 UnrealWorldPos = float3( In.worldPos.x, In.worldPos.y, In.worldPos.z );

	float3 UnrealNormal = In.normal2;	

	FMaterialPixelParameters Parameters = (FMaterialPixelParameters)0;
#if NUM_TEX_COORD_INTERPOLATORS > 0
    #ifdef FULLSCREEN_SHADERGRAPH
		Parameters.TexCoords[ 0 ] = float2( In.uv_MainTex.x, In.uv_MainTex.y );
	#else
		Parameters.TexCoords[ 0 ] = float2( In.uv_MainTex.x, 1.0 - In.uv_MainTex.y );
	#endif
#endif
#if NUM_TEX_COORD_INTERPOLATORS > 1
	Parameters.TexCoords[ 1 ] = float2( In.uv2_Material_Texture2D_0.x, 1.0 - In.uv2_Material_Texture2D_0.y );
#endif
#if NUM_TEX_COORD_INTERPOLATORS > 2
	for( int i = 2; i < NUM_TEX_COORD_INTERPOLATORS; i++ )
	{
		Parameters.TexCoords[ i ] = float2( In.uv_MainTex.x, 1.0 - In.uv_MainTex.y );
	}
#endif
	Parameters.PostProcessUV = In.uv_MainTex;
	Parameters.VertexColor = In.color;
	Parameters.WorldNormal = UnrealNormal;
	Parameters.ReflectionVector = half3( 0, 0, 1 );
	//Parameters.CameraVector = normalize( _WorldSpaceCameraPos.xyz - UnrealWorldPos.xyz );
	//Parameters.CameraVector = mul( ( float3x3 )unity_CameraToWorld, float3( 0, 0, 1 ) ) * -1;	
	float3 CameraDirection = (-1 * mul((float3x3)UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V)) [2].xyz));//From ShaderGraph
	Parameters.CameraVector = CameraDirection;
	Parameters.LightVector = half3( 0, 0, 0 );
	float4 screenpos = In.screenPos;
	screenpos /= screenpos.w;
	Parameters.SvPosition = screenpos;
	Parameters.ScreenPosition = Parameters.SvPosition;

	Parameters.UnMirrored = 1;

	Parameters.TwoSidedSign = 1;


	float3 InWorldNormal = UnrealNormal;	
	float4 tangentWorld = In.tangent;
	tangentWorld.xyz = normalize( tangentWorld.xyz );
	//float3x3 tangentToWorld = CreateTangentToWorldPerVertex( InWorldNormal, tangentWorld.xyz, tangentWorld.w );
	Parameters.TangentToWorld = float3x3( normalize( cross( InWorldNormal, tangentWorld.xyz ) * tangentWorld.w ), tangentWorld.xyz, InWorldNormal );

	//WorldAlignedTexturing in UE relies on the fact that coords there are 100x larger, prepare values for that
	//but watch out for any computation that might get skewed as a side effect
	UnrealWorldPos = ToUnrealPos( UnrealWorldPos );
	
	Parameters.AbsoluteWorldPosition = UnrealWorldPos;
	Parameters.WorldPosition_CamRelative = UnrealWorldPos;
	Parameters.WorldPosition_NoOffsets = UnrealWorldPos;

	Parameters.WorldPosition_NoOffsets_CamRelative = Parameters.WorldPosition_CamRelative;
	Parameters.LightingPositionOffset = float3( 0, 0, 0 );

	Parameters.AOMaterialMask = 0;

	Parameters.Particle.RelativeTime = 0;
	Parameters.Particle.MotionBlurFade;
	Parameters.Particle.Random = 0;
	Parameters.Particle.Velocity = half4( 1, 1, 1, 1 );
	Parameters.Particle.Color = half4( 1, 1, 1, 1 );
	Parameters.Particle.TranslatedWorldPositionAndSize = float4( UnrealWorldPos, 0 );
	Parameters.Particle.MacroUV = half4( 0, 0, 1, 1 );
	Parameters.Particle.DynamicParameter = half4( 0, 0, 0, 0 );
	Parameters.Particle.LocalToWorld = float4x4( Z4, Z4, Z4, Z4 );
	Parameters.Particle.Size = float2( 1, 1 );
	Parameters.Particle.SubUVCoords[ 0 ] = Parameters.Particle.SubUVCoords[ 1 ] = float2( 0, 0 );
	Parameters.Particle.SubUVLerp = 0.0;
	Parameters.TexCoordScalesParams = float2( 0, 0 );
	Parameters.PrimitiveId = 0;
	Parameters.VirtualTextureFeedback = 0;

	FPixelMaterialInputs PixelMaterialInputs = (FPixelMaterialInputs)0;
	PixelMaterialInputs.Normal = float3( 0, 0, 1 );
	PixelMaterialInputs.ShadingModel = 0;
	//PixelMaterialInputs.FrontMaterial = GetStrataUnlitBSDF( float3( 0, 0, 0 ), float3( 0, 0, 0 ) );

	SetupCommonData( Parameters.PrimitiveId );
	//CustomizedUVs
	#if NUM_TEX_COORD_INTERPOLATORS > 0 && HAS_CUSTOMIZED_UVS
		float2 OutTexCoords[ NUM_TEX_COORD_INTERPOLATORS ];
		//Prevent uninitialized reads
		for( int i = 0; i < NUM_TEX_COORD_INTERPOLATORS; i++ )
		{
			OutTexCoords[ i ] = float2( 0, 0 );
		}
		GetMaterialCustomizedUVs( Parameters, OutTexCoords );
		for( int i = 0; i < NUM_TEX_COORD_INTERPOLATORS; i++ )
		{
			Parameters.TexCoords[ i ] = OutTexCoords[ i ];
		}
	#endif
	//<-
	CalcPixelMaterialInputs( Parameters, PixelMaterialInputs );

	#define HAS_WORLDSPACE_NORMAL 0
	#if HAS_WORLDSPACE_NORMAL
		PixelMaterialInputs.Normal = mul( PixelMaterialInputs.Normal, (MaterialFloat3x3)( transpose( Parameters.TangentToWorld ) ) );
	#endif

	o.Albedo = PixelMaterialInputs.BaseColor.rgb;
	o.Alpha = PixelMaterialInputs.Opacity;
	//if( PixelMaterialInputs.OpacityMask < 0.333 ) discard;
	//o.Alpha = PixelMaterialInputs.OpacityMask;

	o.Metallic = PixelMaterialInputs.Metallic;
	o.Smoothness = 1.0 - PixelMaterialInputs.Roughness;
	o.Normal = normalize( PixelMaterialInputs.Normal );
	o.Emission = PixelMaterialInputs.EmissiveColor.rgb;
	o.Occlusion = PixelMaterialInputs.AmbientOcclusion;
}