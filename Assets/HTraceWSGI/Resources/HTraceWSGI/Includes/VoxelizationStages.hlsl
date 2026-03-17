#pragma once

#define ATTRIBUTES_NEED_NORMAL
#define ATTRIBUTES_NEED_TEXCOORD0
#define ATTRIBUTES_NEED_TEXCOORD1
#define ATTRIBUTES_NEED_COLOR

#define EXTRA_ATTRIBUTES 1

#define EVALUATE_TVE 0
#define EVALUATE_LIT 1
#define EVALUATE_EMISSION 1
#define EVALUATE_UNLIT 0
#define EVALUATE_TERRAIN 0
#define EVALUATE_SPEEDTREE 0
#define EVALUATE_LAYERED_LIT 0

#define AXIS_X 0
#define AXIS_Y 1
#define AXIS_Z 2

#include "VoxelizationCommon.hlsl"
#include "VoxelMaterialEvaluation.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/VaryingMesh.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/Shaders/LitGBufferPass.hlsl"

H_RW_TEXTURE3D(uint, _VoxelColor);

int _OffsetAxisIndex;
int2 _CullingTrim;
int2 _CullingTrimAxis;
float3 _AxisOffset;
float3 _OctantOffset;
float3 _VoxelCameraPosActual;
float _EmissiveScale;

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 texcoord : TEXCOORD0;
    float2 texcoord1 : TEXCOORD1;
    float2 staticLightmapUV : TEXCOORD2;
    float2 dynamicLightmapUV : TEXCOORD3;
    float4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


// ------------------------ SHARED STRUCTS ------------------------
struct VertexToGeometry
{
    float4 PositionCS      : POSITION;
    float4 TextureCoords   : TEXCOORD0;

    #ifdef PARTIAL_VOXELIZATION
    int CullingTest        : TEXCOORD1;
    #endif
    
    #if EXTRA_ATTRIBUTES
    float3 PositionWS      : TEXCOORD2;
    float3 PivotWS         : TEXCOORD3;
    float4 VertexColor     : COLOR0;
    #endif
};

struct GeometryToFragment
{
    float4 PositionCS      : POSITION;
    float4 TextureCoords   : TEXCOORD0;
    float Axis             : TEXCOORD1;

    #if EXTRA_ATTRIBUTES
    float3 PositionWS      : TEXCOORD2;
    float3 NormalWS        : TEXCOORD3;
    float3 PivotWS         : TEXCOORD4;
    float4 VertexColor     : COLOR0;
    #endif
};


// ------------------------ SHARED FUNCTIONS ------------------------
float3 SwizzleAxis(float3 Position, uint Axis)
{
    uint a = Axis + 1;
    float3 p = Position;
    Position.x = p[(0 + a) % 3];
    Position.y = p[(1 + a) % 3];
    Position.z = p[(2 + a) % 3];

    return Position;
}

float3 RestoreAxis(float3 Position, uint Axis)
{
    uint a = 2 - Axis;
    float3 p = Position;
    Position.x = p[(0 + a) % 3];
    Position.y = p[(1 + a) % 3];
    Position.z = p[(2 + a) % 3]; 
    
    return Position;
}


// --- Vertex Stage ---
VertexToGeometry VoxelizationVert(Attributes inputMesh)
{
    VertexToGeometry Output;

    float3 PositionWS;
    
    inputMesh.positionOS *= _EmissiveScale;
 
    float3 PivotWS = GetAbsolutePositionWS(float3(UNITY_MATRIX_M[0].w, UNITY_MATRIX_M[1].w, UNITY_MATRIX_M[2].w));
    
    // Process instancing
 
    PositionWS = mul(UNITY_MATRIX_M, float4(inputMesh.positionOS.xyz, 1.0)).xyz;

    Output.PositionCS = TransformWorldToHClip(PositionWS);
    

    // Output uv channels
    Output.TextureCoords = float4(inputMesh.texcoord, inputMesh.texcoord1);

    #ifdef UNITY_REVERSED_Z
    Output.PositionCS.z = mad(Output.PositionCS.z, -2.0, 1.0);
    #endif

    #if EXTRA_ATTRIBUTES
    Output.PositionWS = PositionWS;
    Output.PivotWS = PivotWS;
    //Alternative if unity complains about using the matrix
    //Output.PivotWS =  GetAbsolutePositionWS((TransformObjectToWorld(float3(0, 0, 0))));
    Output.VertexColor = inputMesh.color;
    #endif
    
    return Output;
}

// --- Geometry Stage ---
[maxvertexcount(3)]
void VoxelizationGeom(triangle VertexToGeometry i[3], inout TriangleStream<GeometryToFragment> Stream)
{
    // If all 3 vertices are behind culling camera - early out
    #ifdef PARTIAL_VOXELIZATION
    if (i[0].CullingTest + i[1].CullingTest + i[2].CullingTest == 3)
        return;
    #endif
    
    float3 Normal = normalize(abs(cross(i[1].PositionCS.xyz - i[0].PositionCS.xyz, i[2].PositionCS.xyz - i[0].PositionCS.xyz)));
    
    uint Axis = AXIS_Z;
    if  (Normal.x > Normal.y && Normal.x > Normal.z)
         Axis = AXIS_X;
    else if (Normal.y > Normal.x && Normal.y > Normal.z)
         Axis = AXIS_Y;
    
    [unroll]
    for (int j = 0; j < 3; j++)
    {
        GeometryToFragment Output;

        Output.PositionCS = float4(SwizzleAxis(i[j].PositionCS.xyz, Axis), 1); 

        #ifdef UNITY_UV_STARTS_AT_TOP
        Output.PositionCS.y = -Output.PositionCS.y;
        #endif
        
        #ifdef UNITY_REVERSED_Z
        Output.PositionCS.z = mad(Output.PositionCS.z, 0.5, 0.5);
        #endif
        
        Output.TextureCoords = i[j].TextureCoords;
        Output.Axis = Axis;
        
        #if EXTRA_ATTRIBUTES
        Output.VertexColor = i[j].VertexColor;
        Output.PositionWS = i[j].PositionWS;
        Output.PivotWS = i[j].PivotWS;
        Output.NormalWS = Normal;
        #endif
        
        Stream.Append(Output);
    }
}

// --- Fragment Stage ---
float VoxelizationFrag(GeometryToFragment Input) : SV_TARGET
{
    float VoxelRes = _VoxelResolution.x;

    #ifndef PARTIAL_VOXELIZATION
    VoxelRes = _VoxelResolution.x * 2;
    #endif
    
    float3 VoxelPos = float3(Input.PositionCS.x, Input.PositionCS.y, Input.PositionCS.z * VoxelRes);
    VoxelPos = RestoreAxis(VoxelPos, Input.Axis);
    
    // Modify Axes for non-cubic bounds
    VoxelPos.xyz = VoxelPos.xzy;
    VoxelPos.y *= (_VoxelBounds.z / _VoxelBounds.y);
    VoxelPos.xz = VoxelRes - VoxelPos.xz;

    // Calculate octants for the first 8 bits
    uint3 VoxelPosInt = floor(VoxelPos);
    uint BitShift = (1 * (VoxelPosInt.x % 2)) + (2 * (VoxelPosInt.y % 2)) + (4 * (VoxelPosInt.z % 2));//2x2x2  转为 0到7
    uint OctantBits = (1 << BitShift) << 24; //32位中，最高的8位存储BitShift即 24到31
    int StaticBitFlag = 1 << 23;//第23位设置为1

    int3 VoxelPosRounded = floor(VoxelPos / 2);

    // Fill inout Surface Data with input information
    VoxelSurfaceData SurfaceData = (VoxelSurfaceData)0;
    SurfaceData.TexCoord0 = Input.TextureCoords.xy;
    SurfaceData.TexCoord1 = Input.TextureCoords.zw;
    
    #if EXTRA_ATTRIBUTES
    SurfaceData.VertexColor = Input.VertexColor;
    SurfaceData.PositionWS = Input.PositionWS;
    SurfaceData.NormalWS = Input.NormalWS;
    SurfaceData.PivotWS = Input.PivotWS;
    #endif
    
    // Evaluate all material attributes
    EvaluateSurfaceColor(SurfaceData);

    if (!SurfaceData.IsEmissive)
    {
        SurfaceData.Color = ClampDiffuseColor(SurfaceData.Color);
    }
            
    if (SurfaceData.Alpha == 1)//这个和材质的透明度不一样
        OctantBits = 0;
    
    //SurfaceData.Color = Input.VertexColor;
    // Pack color for the last 24 bits
    uint PackedColor = PackVoxelColor(SurfaceData.Color, SurfaceData.IsEmissive);

    uint OriginalValue;
    InterlockedOr(_VoxelColor[VoxelPosRounded],  StaticBitFlag | OctantBits, OriginalValue);//存储31到23位
    //后面的位数分别存储RGB和IsEmissive
    InterlockedMax(_VoxelColor[VoxelPosRounded], StaticBitFlag | PackedColor | (OriginalValue & 0xFF000000) | OctantBits); //OriginalValue & 0xFF000000提取高8位的数据

    return 0.0f;
}
