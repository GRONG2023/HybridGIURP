#define NUM_TEX_COORD_INTERPOLATORS 1
#define NUM_MATERIAL_TEXCOORDS_VERTEX 1
#define NUM_CUSTOM_VERTEX_INTERPOLATORS 0

struct Input
{
	//float3 Normal;
	float2 uv_MainTex : TEXCOORD0;
	float4 color : COLOR;
	float4 tangent;
	//float4 normal;
	float3 viewDir;
	float3 worldPos;
	float3 worldNormal;
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
//TEXTURE2D( _BaseColorMap );
//SAMPLER(  sampler_BaseColorMap );
//TEXTURE2D(       _NormalMap );
//SAMPLER(  sampler_NormalMap );
//TEXTURE2D(       _EmissiveColorMap );
//SAMPLER(  sampler_EmissiveColorMap );

CBUFFER_START(UnityPerMaterial2)
float4 Material_Texture2D_0_TexelSize;
float4 Material_Texture2D_0_ST;
float4 Material_Texture2D_1_TexelSize;
float4 Material_Texture2D_1_ST;
uniform float4 SelectionColor;
uniform float AtlasSize;
uniform float AtlasTileSize;
uniform float Padding;
uniform float SpecPosCount;
//uniform float _EmissiveIntensity;
uniform float _EmissiveScale;
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
};

static SamplerState View_MaterialTextureBilinearWrapedSampler;
static SamplerState View_MaterialTextureBilinearClampedSampler;
struct ViewStruct
{
	float4 PrimitiveSceneData[ 40 ];
	float4 ViewSizeAndInvSize;
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

void CalcPixelMaterialInputs(in FMaterialPixelParameters Parameters, in out FPixelMaterialInputs PixelMaterialInputs)
{
    float3 WorldNormalCopy = Parameters.WorldNormal;

    SHADER_PUSH_WARNINGS_STATE
    SHADER_DISABLE_WARNINGS

    // =============================================
    // ����Atlas UV�����MipMap����
    // =============================================
    
    float2 baseUV = Parameters.TexCoords[0].xy;
    
    // ��ȡ������ɫAlphaͨ��������ȷ��ͼ������
    float atlasIndex = round(Parameters.VertexColor.a * 255.0);
    
    // �ж��Ƿ�Ϊ����ͼ��λ��(SpecPos)
    // step(SpecPosCount, atlasIndex): atlasIndex >= SpecPosCount ʱΪ1
    float isSpecialTile = step(Material.PreshaderBuffer[1].x, atlasIndex);
    
    // �����Ƿ�Ϊ����ͼ����ѡ��ͬ��UV���ű���
    // PreshaderBuffer[0].xy = ���߿��UV���� (����ͼ����)
    // PreshaderBuffer[0].zw = �����߿��UV���� (��ͨͼ����)
    float2 uvScaleNormal = Material.PreshaderBuffer[0].xy; // (AtlasTileSize - (Padding-1)*2) / AtlasSize
    float2 uvScalePadded = Material.PreshaderBuffer[0].zw; // (AtlasTileSize - Padding*2) / AtlasSize
    
    // ����MipMap����
    // ͨ��DDX/DDY����UV������ӳ�䵽ͼ��ʵ�����ش�С��ȡlog2
    float atlasSize = Material.PreshaderBuffer[1].y; // AtlasSize
    float2 uvScale = lerp(uvScaleNormal, uvScalePadded, isSpecialTile);
    
    float2 ddxUV = ddx(baseUV);
    float2 ddyUV = ddy(baseUV);
    float2 ddxUVScaled = lerp(ddxUV * uvScaleNormal, ddxUV * uvScalePadded, isSpecialTile);
    float2 ddyUVScaled = lerp(ddyUV * uvScaleNormal, ddyUV * uvScalePadded, isSpecialTile);
    
    float ddxLength = length(atlasSize * ddxUVScaled);
    float ddyLength = length(atlasSize * ddyUVScaled);
    float maxDerivLength = max(max(ddxLength, ddyLength), 0.000001);
    float mipLevel = min(log2(maxDerivLength), 3.0);
    
    // =============================================
    // ����ͼ��Tile��UVƫ��
    // =============================================
    
    float tilesPerRow = Material.PreshaderBuffer[1].z; // AtlasSize / AtlasTileSize
    float tileUVSize = Material.PreshaderBuffer[1].w; // 1 / tilesPerRow
    
    // ���㵱ǰTile��ͼ���е�����λ��
    float tileCol = fmod(atlasIndex, tilesPerRow);
    float tileRow = floor(atlasIndex * Material.PreshaderBuffer[2].x); // atlasIndex / tilesPerRow
    
    // Tile���Ͻ���ͼ��UV�ռ��е�ƫ��
    float2 tileOffset = float2(tileCol * tileUVSize, tileRow * tileUVSize);
    
    // =============================================
    // �������ղ���UV
    // =============================================
    
    float2 fracUV = frac(baseUV);
    
    // ��ͨģʽ: ֱ���ô��߿����ŵ�UV
    float2 uvNormalMode = tileOffset + fracUV * uvScaleNormal;
    
    // ����ģʽ: �ò����߿����ŵ�UV��������Paddingƫ��
    float2 paddingOffset = Material.PreshaderBuffer[2].zw; // Padding / AtlasSize
    float2 uvSpecialMode = tileOffset + fracUV * uvScalePadded + paddingOffset;
    
    // ����isSpecialTileѡ������UV
    float2 atlasUV = lerp(uvNormalMode, uvSpecialMode, isSpecialTile);
    
    // =============================================
    // ����������ͼ������
    // =============================================
    
    MaterialStoreTexCoordScale(Parameters, atlasUV, 0);
    MaterialFloat4 normalSample = ProcessMaterialLinearColorTextureLookup(
        Texture2DSampleLevel(_NormalMap, sampler_NormalMap, atlasUV, mipLevel)
    );
    MaterialStoreTexSample(Parameters, normalSample, 0);
    
    // ��RGͨ����[0,1]ӳ�䵽[-1,1]
    float2 normalXY = normalSample.rg * 2.0 - 1.0;
    
    // UE4����DXT5nm���߽���
    // ͨ��X,Y�����ؽ�Z����������������
    float absX = abs(normalXY.x);
    float absY = abs(normalXY.y);
    float signX = sign(normalXY.x);
    float signY = sign(normalXY.y);
    
    float decodedX = signX * (1.0 - absY); // Local43
    float decodedY = signY * (1.0 - absX); // Local47
    float decodedZ = (1.0 - absX) - absY; // Local48
    
    // ����Z��������ѡ����뷽ʽ
    float useOriginalXY = step(0.0, decodedZ);
    float3 decodedNormal = lerp(
        float3(decodedX, decodedY, decodedZ), // Z<0ʱʹ���ؽ���XY
        float3(normalXY.x, normalXY.y, decodedZ), // Z>=0ʱʹ��ԭʼXY
        useOriginalXY
    );
    
    float3 tangentNormal = normalize(decodedNormal);
    
    // The Normal is a special case as it might have its own expressions 
    // and also be used to calculate other inputs, so perform the assignment here
    PixelMaterialInputs.Normal = tangentNormal;

    // ... (���߿ռ�ת�����뱣�ֲ���)
    float3 MaterialNormal = GetMaterialNormal(Parameters, PixelMaterialInputs);

#if MATERIAL_TANGENTSPACENORMAL
#if FEATURE_LEVEL >= FEATURE_LEVEL_SM4
    MaterialNormal = normalize(MaterialNormal);
#endif
#endif

    SHADER_PUSH_WARNINGS_STATE
    SHADER_DISABLE_WARNINGS

    // =============================================
    // ���㶥��ɫ����
    // =============================================
    
    float3 vertexColorRGB = Parameters.VertexColor.rgb;
    
    // ������ɫRGB֮�ʹ���0.02ʱ��ʹ�ö���ɫ������ʹ�ð�ɫ(1,1,1)
    // ��������"�޶���ɫ"��"�ж���ɫ"�����
    float vertexColorSum = vertexColorRGB.r + vertexColorRGB.g + vertexColorRGB.b;
    float hasVertexColor = step(0.02, vertexColorSum);
    
    // =============================================
    // ������ͼ�������������
    // =============================================
    
    MaterialFloat4 baseColor = ProcessMaterialColorTextureLookup(
        Texture2DSampleLevel(_BaseColorMap, sampler_BaseColorMap, atlasUV, mipLevel)
    );
    
    // ������ɫ����BaseColor���޶���ɫʱ����1����ԭɫ��
    float3 tintedVertexColor = lerp(float3(1.0, 1.0, 1.0), vertexColorRGB, hasVertexColor);
    baseColor.rgb *= tintedVertexColor;
    
    MaterialFloat3 emissive = ProcessMaterialColorTextureLookup(
        Texture2DSampleLevel(_EmissiveColorMap, sampler_EmissiveColorMap, atlasUV, mipLevel)
    );

    // =============================================
    // �����������
    // =============================================
    
    PixelMaterialInputs.EmissiveColor = emissive;
    PixelMaterialInputs.Opacity = 1.0;
    PixelMaterialInputs.OpacityMask = 1.0;
    PixelMaterialInputs.BaseColor = baseColor.rgb;
    PixelMaterialInputs.Metallic = normalSample.a; // �����ȴ��ڷ�����ͼAͨ��
    PixelMaterialInputs.Specular = 0.5;
    PixelMaterialInputs.Roughness = normalSample.b; // �ֲڶȴ��ڷ�����ͼBͨ��
    PixelMaterialInputs.Anisotropy = 0.0;
    PixelMaterialInputs.Normal = tangentNormal;
    PixelMaterialInputs.Tangent = float3(1.0, 0.0, 0.0);
    PixelMaterialInputs.Subsurface = 0;
    PixelMaterialInputs.AmbientOcclusion = 1.0;
    PixelMaterialInputs.Refraction = 0;
    PixelMaterialInputs.PixelDepthOffset = 0.0;
    PixelMaterialInputs.ShadingModel = 1;
    PixelMaterialInputs.FrontMaterial = GetInitialisedSubstrateData();
    PixelMaterialInputs.SurfaceThickness = 0.01;
    PixelMaterialInputs.Displacement = -1.0;

    SHADER_POP_WARNINGS_STATE

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

	MaterialFloat2 Local65 = Parameters.TexCoords[0].xy;
	OutTexCoords[0] = Local65;

}



#define UnityObjectToWorldDir TransformObjectToWorld

void SetupCommonData( int Parameters_PrimitiveId )
{
	View_MaterialTextureBilinearWrapedSampler = SamplerState_Linear_Repeat;
	View_MaterialTextureBilinearClampedSampler = SamplerState_Linear_Clamp;

	Material_Wrap_WorldGroupSettings = SamplerState_Linear_Repeat;
	Material_Clamp_WorldGroupSettings = SamplerState_Linear_Clamp;

	View.ViewSizeAndInvSize = View_BufferSizeAndInvSize;

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
	//View.PrimitiveSceneData[ PrimitiveBaseOffset + 5 ] = float4( ToUnrealPos( SHADERGRAPH_OBJECT_POSITION ), 100.0 );//ObjectWorldPosition
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 6 ] = WorldToLocal[ 0 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 7 ] = WorldToLocal[ 1 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 8 ] = WorldToLocal[ 2 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 9 ] = WorldToLocal[ 3 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 10 ] = LocalToWorld[ 0 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 11 ] = LocalToWorld[ 1 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 12 ] = LocalToWorld[ 2 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 13 ] = LocalToWorld[ 3 ];//PreviousLocalToWorld
	//View.PrimitiveSceneData[ PrimitiveBaseOffset + 18 ] = float4( ToUnrealPos( SHADERGRAPH_OBJECT_POSITION ), 0 );//ActorWorldPosition
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
	Primitive.WorldToLocal = WorldToLocal;
	Primitive.LocalToWorld = LocalToWorld;
}
#define VS_USES_UNREAL_SPACE 1

void SurfaceReplacement( Input In, out SurfaceOutputStandard o )
{
	InitializeExpressions();

	float3 Z3 = float3( 0, 0, 0 );
	float4 Z4 = float4( 0, 0, 0, 0 );

	float3 UnrealWorldPos = float3( In.worldPos.x, In.worldPos.y, In.worldPos.z );

    float3 UnrealNormal = In.worldNormal;

	FMaterialPixelParameters Parameters = (FMaterialPixelParameters)0;
#if NUM_TEX_COORD_INTERPOLATORS > 0
    #ifdef FULLSCREEN_SHADERGRAPH
		Parameters.TexCoords[ 0 ] = float2( In.uv_MainTex.x, In.uv_MainTex.y );
	#else
		Parameters.TexCoords[ 0 ] = float2( In.uv_MainTex.x, 1.0 - In.uv_MainTex.y );
	#endif
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
    o.Emission = PixelMaterialInputs.EmissiveColor.rgb * _EmissiveScale;
	o.Occlusion = PixelMaterialInputs.AmbientOcclusion;
}