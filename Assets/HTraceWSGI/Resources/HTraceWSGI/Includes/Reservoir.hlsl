#pragma once
#include "../Headers/HMain.hlsl"
#include "ScreenProbesCommon.hlsl"

H_TEXTURE(_ProbeDiffuse);

// Only Ray data
H_TEXTURE(_ReservoirAtlasRayData);
H_TEXTURE(_ReservoirAtlasRayData_Disocclusion);
H_RW_TEXTURE(uint, _ReservoirAtlasRayData_Output);

// Only Radiance data
H_TEXTURE(_ReservoirAtlasRadianceData);
H_RW_TEXTURE(uint2, _ReservoirAtlasRadianceData_Inout);
H_RW_TEXTURE(uint2, _ReservoirAtlasRadianceData_Output);

// Full reservoir with Radiance & Ray datas
H_TEXTURE(_ReservoirAtlas);
H_TEXTURE_ARRAY(_ReservoirAtlas_History);
H_RW_TEXTURE(uint4, _ReservoirAtlas_Output);
H_RW_TEXTURE(float4, _ReservoirAtlas_Output_Debug);
H_RW_TEXTURE_ARRAY(uint4, _ReservoirAtlas_ArrayOutput);

uint _UseDiffuseWeight;

// ------------------------ RESERVOIR STRUCTS -----------------------
struct RadianceData
{
    float3 Color;
    float Wsum;
    float M;
    float W;
};

struct OriginData
{
    // Empty for now, but we will need it for validation
};

struct RayData
{
    float3 OriginNormal;
    float3 Direction;
    float Distance;
};

struct Reservoir
{
    uint2 MergedCoord;
    
    RadianceData Radiance;
    RayData Ray;
};



// ------------------------ RESERVOIR PACKING FUNCTIONS-----------------------

uint2 PackRadianceData(RadianceData Radiance)
{
    uint W = f32tof16(Radiance.W);		
    uint M = f32tof16(Radiance.M);		
    uint PackedMW = (W << 16) | (M << 0);
    uint PackedColor = PackTonemappedColor24bit(Radiance.Color);
    
    return uint2(PackedColor, PackedMW);
}

uint2 PackRayData(RayData Ray)
{
    uint DirectionPacked = PackDirection24bit(Ray.Direction);
    uint DistancePacked = (f32tof16(Ray.Distance) >> 8) & 0xFF;

    uint DistanceDirectionPacked = (DistancePacked << 24) | DirectionPacked;
    uint OriginNormalPacked = PackDirection24bit(Ray.OriginNormal);

    return uint2(DistanceDirectionPacked, OriginNormalPacked);
}

void UnpackRadianceData(uint2 RadianceDataPacked, float3 Diffuse, inout RadianceData Radiance)
{
    Radiance.Color = UnpackTonemappedColor24bit(RadianceDataPacked.x);
    Radiance.W = f16tof32(RadianceDataPacked.y >> 16);
    Radiance.M = f16tof32(RadianceDataPacked.y >> 0);
    Radiance.Wsum = Radiance.W * Radiance.M * Luminance(Radiance.Color * Diffuse);
}

void UnpackRayData(uint2 RayDataPacked, inout RayData Ray)
{
    Ray.Direction = UnpackDirection24bit(RayDataPacked.x); 
    Ray.Distance = f16tof32(((RayDataPacked.x >> 24) & 0xFF) << 8);
    Ray.OriginNormal = UnpackDirection24bit(RayDataPacked.y);
}

uint PackOcclusion(float Occlusion, bool IsDisocclusion)
{
    uint OcclusionPacked = uint(Occlusion * 127.0f + 0.5f) & 0x7F;
    return (OcclusionPacked << 24) | (IsDisocclusion << 31);
}

float UnpackOcclusion(uint OcclusionPacked, out bool IsDisocclusion)
{
    IsDisocclusion = OcclusionPacked >> 31;
    return ((OcclusionPacked >> 24) & 0x7F) / 127.0f; 
}


// ------------------------ RESERVOIR FUNCTIONS -----------------------
float3 GetReservoirDiffuse(uint2 pixCoord)
{
    float3 DiffuseBuffer = _UseDiffuseWeight ? H_LOAD(_ProbeDiffuse, pixCoord).xyz : 1.0f;

    if (DiffuseBuffer.x + DiffuseBuffer.y + DiffuseBuffer.z == 0)
        DiffuseBuffer = float3(0.05, 0.05, 0.05);

    return DiffuseBuffer;
}

// Reservoir update
bool ReservoirUpdate(uint2 SampleCoord, float3 SampleColor, float SampleW, float SampleM, inout Reservoir Reservoir, inout uint Random)
{
    float RandomValue = UintToFloat01(Hash1Mutate(Random));
    
    Reservoir.Radiance.Wsum += SampleW;
    Reservoir.Radiance.M += SampleM;
    
    if (RandomValue < SampleW / Reservoir.Radiance.Wsum)
    {
        Reservoir.Radiance.Color = SampleColor;
        Reservoir.MergedCoord = SampleCoord;
        
        return true;
    }
    
    return false;
}

// Reservoir update with RayData
bool ReservoirUpdate(uint2 SampleCoord, float3 SampleColor, float SampleW, float SampleM, RayData SampleRay, inout Reservoir Reservoir, inout uint Random)
{
    float RandomValue = UintToFloat01(Hash1Mutate(Random));
    
    Reservoir.Radiance.Wsum += SampleW;
    Reservoir.Radiance.M += SampleM;
    
    if (RandomValue < SampleW / Reservoir.Radiance.Wsum)
    {   
        Reservoir.Radiance.Color = SampleColor;
        Reservoir.MergedCoord = SampleCoord;
        
        Reservoir.Ray.OriginNormal = SampleRay.OriginNormal;
        Reservoir.Ray.Direction = SampleRay.Direction;
        Reservoir.Ray.Distance = SampleRay.Distance;
        
        return true;
    }
    
    return false;
}

// Merges central reservoir with a temporal neighbour (Radiance & Ray datas are exhanged) loaded externally
bool ReservoirMergeTemporal(uint2 SampleCoord, uint4 SampleReservoirPacked, uint ArrayIndex, float SampleWeight, float3 Diffuse, inout uint Random, inout Reservoir Reservoir)
{
    RadianceData SampleRadiance;
    OriginData SampleOrigin;
    RayData SampleRay;
   
    UnpackRadianceData(SampleReservoirPacked.xy, Diffuse, SampleRadiance);
    UnpackRayData(SampleReservoirPacked.zw, SampleRay);

    SampleRadiance.Wsum *= SampleWeight;
    SampleRadiance.M *= SampleWeight;
    
    return ReservoirUpdate(SampleCoord, SampleRadiance.Color, SampleRadiance.Wsum, SampleRadiance.M, SampleRay, Reservoir, Random);
}

// Merges central reservoir with a spatial neighbour (only RadianceData is exchanged) loaded externally
bool ReservoirMergeSpatial(uint2 SampleCoord, uint2 SampleReservoirPacked, float SampleWeight, float3 Diffuse, inout Reservoir Reservoir, inout uint Random)
{
    RadianceData SampleRadiance;
    UnpackRadianceData(SampleReservoirPacked, Diffuse, SampleRadiance);

    SampleRadiance.Wsum *= SampleWeight;
    SampleRadiance.M *= SampleWeight;
    
    return ReservoirUpdate(SampleCoord, SampleRadiance.Color, SampleRadiance.Wsum, SampleRadiance.M, Reservoir, Random);
}


// Empty RadianceData initialization
void RadianceDataInitialize(out RadianceData Radiance)
{
    Radiance.Color = 0;
    Radiance.Wsum = 0;
    Radiance.M = 0;
    Radiance.W = 0;
}

// Empty RayData initialization
void RayDataInitialize(out RayData Ray)
{
    Ray.OriginNormal = 0;
    Ray.Direction = 0;
    Ray.Distance = 0;
}

// Empty reservoir initialization
void ReservoirInitialize(uint2 Coord, out Reservoir Reservoir)
{
    Reservoir.MergedCoord = Coord;
    
    RadianceDataInitialize(Reservoir.Radiance);
    RayDataInitialize(Reservoir.Ray);
}




// ------------------------ COMMON VARIABLES -----------------------
int _PersistentHistorySamples;


// ------------------------ TEMPORAL REPROJECTION STRUCTS -----------------------
struct CurrentFrameData
{
    float3  Normal;
    float3  WorldPos;
    float   DepthRaw;
    float   AligmentZ;
    float   DepthLinear;
    bool    MovingPixel;
};

struct PrevFrameData
{
    float3  Normal;
    float3  WorldPos;
    float   DepthLinear;
};


// ------------------------ TEMPORAL REPROJECTION FUNCTIONS -----------------------
int GetHistoryIndex(int Index)
{
    Index += 1;
    
    int HistoryIndex = uint(_HFrameCount) % _PersistentHistorySamples - Index;
    
    if (HistoryIndex < 0)
        HistoryIndex = _PersistentHistorySamples - abs(HistoryIndex);

    return HistoryIndex;
}


float3 DirectClipToAABB(float3 History, float3 Min, float3 Max)
{
    float3 Center  = 0.5 * (Max + Min);
    float3 Extents = 0.5 * (Max - Min);
    
    float3 Offset = History - Center;
    float3 Vunit = Offset.xyz / Extents.xyz;
    float3 AbsUnit = abs(Vunit);
    float MaxUnit = max(max(AbsUnit.x, AbsUnit.y), AbsUnit.z);

    if (MaxUnit > 1.0) return Center + (Offset / MaxUnit);
    else  return History;
}


float DisocclusionDetection(CurrentFrameData CurrentData, PrevFrameData PrevData, bool MovingIntersection, out float RelaxedWeight, out float DisocclusionWeight)
{
    RelaxedWeight = 1;
    DisocclusionWeight = 1;
    
    float PlaneMultiplier = CurrentData.MovingPixel ? 100.0f : 100000.0f; //TODO: make it 5000 for the editor window
    float DepthMultiplier = CurrentData.MovingPixel ? 20.0f : 1.0f;
    
    // Depth-based rejection with an adaptive threshold
    float DepthThreshold = lerp(1e-2f, 1e-1f, CurrentData.AligmentZ);
    if (abs((PrevData.DepthLinear - CurrentData.DepthLinear) / CurrentData.DepthLinear) >= DepthThreshold * DepthMultiplier)
    {
        if (CurrentData.DepthLinear > PrevData.DepthLinear)
            DisocclusionWeight = 0;
    
        RelaxedWeight = 0;
        return 0.0f;
    }
    
    // Plane-based rejection
    float PlaneDistance = abs(dot(PrevData.WorldPos - CurrentData.WorldPos, CurrentData.Normal));
    float RelativeDepthDifference = PlaneDistance / CurrentData.DepthLinear;
    if (exp2(-PlaneMultiplier * (RelativeDepthDifference * RelativeDepthDifference )) < 0.1f)
    {
        RelaxedWeight = 0;
        return 0.0f;
    }
    
    // Normal-based rejection
    if (CurrentData.DepthLinear > PrevData.DepthLinear)
    {
        if (saturate(dot(CurrentData.Normal, PrevData.Normal)) < 0.75)
        {
            RelaxedWeight = 0;
            return  0.0f;
        }
    }
    else 
    {
        if (saturate(dot(CurrentData.Normal, PrevData.Normal)) < 0.75)
            return  0.0f;
    }

    return 1.0f;
}


bool GetReprojectionCoord(int2 pixCoord, float2 MotionVectors, out float4 BilinearWeights, out int2 ReprojectionCoord)
{
    float2 ReprojectionCoordNDC = (pixCoord.xy + 0.5f) - MotionVectors * floor(_ScreenSize.xy / _ProbeSize);
    ReprojectionCoord = ReprojectionCoordNDC - 0.5f;

    float UVx = frac(ReprojectionCoordNDC.x + 0.5f);
    float UVy = frac(ReprojectionCoordNDC.y + 0.5f);

    BilinearWeights.x = (1.0f - UVx) * (1.0f - UVy);
    BilinearWeights.y = (UVx) * (1.0f - UVy);
    BilinearWeights.z = (1.0f - UVx) * (UVy);
    BilinearWeights.w = (UVx) * (UVy);

    if (any(ReprojectionCoord * _ProbeSize >= _ScreenSize.xy) || any(ReprojectionCoordNDC < 0))
    {
        BilinearWeights = float4(0,0,0,0);
        return false;
    }

    return true;
}


bool GetReprojectionWeights(H_TEXTURE_ARRAY(_HistoryBuffer), CurrentFrameData CurrentData, int2 ReprojectionCoord, uint ArrayIndex, inout float4 Weights, inout float4 RelaxedWeights)
{
    PrevFrameData PrevData00, PrevData01, PrevData10, PrevData11;
    
    UnpackWorldPosNormal(asuint(H_LOAD_ARRAY(_HistoryBuffer, ReprojectionCoord + int2(0, 0), ArrayIndex)), PrevData00.WorldPos, PrevData00.Normal); 
    UnpackWorldPosNormal(asuint(H_LOAD_ARRAY(_HistoryBuffer, ReprojectionCoord + int2(1, 0), ArrayIndex)), PrevData10.WorldPos, PrevData10.Normal); 
    UnpackWorldPosNormal(asuint(H_LOAD_ARRAY(_HistoryBuffer, ReprojectionCoord + int2(0, 1), ArrayIndex)), PrevData01.WorldPos, PrevData01.Normal); 
    UnpackWorldPosNormal(asuint(H_LOAD_ARRAY(_HistoryBuffer, ReprojectionCoord + int2(1, 1), ArrayIndex)), PrevData11.WorldPos, PrevData11.Normal); 

    PrevData00.DepthLinear = H_LINEAR_EYE_DEPTH(H_GET_RELATIVE_POSITION_WS(PrevData00.WorldPos), H_MATRIX_V);
    PrevData10.DepthLinear = H_LINEAR_EYE_DEPTH(H_GET_RELATIVE_POSITION_WS(PrevData10.WorldPos), H_MATRIX_V);
    PrevData01.DepthLinear = H_LINEAR_EYE_DEPTH(H_GET_RELATIVE_POSITION_WS(PrevData01.WorldPos), H_MATRIX_V);
    PrevData11.DepthLinear = H_LINEAR_EYE_DEPTH(H_GET_RELATIVE_POSITION_WS(PrevData11.WorldPos), H_MATRIX_V);
    
    float4 BilinearWeights = Weights;
    
    float4 DisocclusionWeights;
    Weights.x *= DisocclusionDetection(CurrentData, PrevData00, false, RelaxedWeights.x, DisocclusionWeights.x);
    Weights.y *= DisocclusionDetection(CurrentData, PrevData10, false, RelaxedWeights.y, DisocclusionWeights.y);
    Weights.z *= DisocclusionDetection(CurrentData, PrevData01, false, RelaxedWeights.z, DisocclusionWeights.z);
    Weights.w *= DisocclusionDetection(CurrentData, PrevData11, false, RelaxedWeights.w, DisocclusionWeights.w);

    return any(DisocclusionWeights <= 0) ? true : false;
}
