#ifndef LIGHT_CLUSTER_INCLUDED
#define LIGHT_CLUSTER_INCLUDED

uint g_HeatmapDebug;
uint g_MaxLightsPerCell;
uint g_SceneLightsBufferSize;
uint3 g_LightClusterDimensions;

float3 g_LightCluterCellSize;
float3 g_MinLightClusterPosition;
float3 g_MaxLightClusterPosition;

bool IsInsideLightCluster(float3 PositionWS)
{
	return all(PositionWS >= g_MinLightClusterPosition && PositionWS <= g_MaxLightClusterPosition);
}

uint GetFlattenedIndex(uint3 Index3D)
{
	return Index3D.x + (Index3D.y * g_LightClusterDimensions.x) + (Index3D.z * g_LightClusterDimensions.x * g_LightClusterDimensions.y);
}

bool ClusterCellSphereIntersection(float3 SphereCenterPosition, float SphereRadius, float3 CellMinCorner, float3 CellMaxCorner)
{
    float ClosestX = max(CellMinCorner.x, min(SphereCenterPosition.x, CellMaxCorner.x));
    float ClosestY = max(CellMinCorner.y, min(SphereCenterPosition.y, CellMaxCorner.y));
    float ClosestZ = max(CellMinCorner.z, min(SphereCenterPosition.z, CellMaxCorner.z));

    float DistanceSquared = 
        (ClosestX - SphereCenterPosition.x) * (ClosestX - SphereCenterPosition.x) +
        (ClosestY - SphereCenterPosition.y) * (ClosestY - SphereCenterPosition.y) +
        (ClosestZ - SphereCenterPosition.z) * (ClosestZ - SphereCenterPosition.z);

    return DistanceSquared < SphereRadius * SphereRadius;
}

bool ClusterCellConeIntersection(
    float3 ConeTipPosition,          // Tip of the cone (spotlight position)
    float3 ConeDirection,            // Normalized direction vector
    float ConeAngleDegrees,          // Full cone angle in degrees
    float ConeLength,                // Cone length
    float3 CellMinCorner,            // AABB minimum corner
    float3 CellMaxCorner             // AABB maximum corner
)
{
    // Check if cone tip is inside the AABB
    if (all(ConeTipPosition >= CellMinCorner) && all(ConeTipPosition <= CellMaxCorner))
        return true;

    // Generate all 8 corners of the AABB
    float3 CellCorners[8];
    
    UNITY_UNROLL
    for (int i = 0; i < 8; i++)
    {
        CellCorners[i] = float3(
            (i & 1) ? CellMaxCorner.x : CellMinCorner.x,
            (i & 2) ? CellMaxCorner.y : CellMinCorner.y,
            (i & 4) ? CellMaxCorner.z : CellMinCorner.z
        );
    }

    float HalfAngleRadians = radians(ConeAngleDegrees * 0.5);
    float CosHalfAngle = cos(HalfAngleRadians);
    float CosHalfAngleSquared = CosHalfAngle * CosHalfAngle;
    float ConeLengthSquared = ConeLength * ConeLength;

    // Check each corner
    UNITY_UNROLL
    for (int Index = 0; Index < 8; Index++)
    {
        float3 VectorToCorner = CellCorners[Index] - ConeTipPosition;
        float DistanceSquared = dot(VectorToCorner, VectorToCorner);

        float ProjectionOnConeDirection = dot(VectorToCorner, ConeDirection);
        float ProjectionSquared = ProjectionOnConeDirection * ProjectionOnConeDirection;

        if (DistanceSquared > ConeLengthSquared) continue;                           // Outside cone range
        if (ProjectionOnConeDirection <= 0) continue;                                // Behind cone
        if (ProjectionSquared >= DistanceSquared * CosHalfAngleSquared) return true; // Inside cone
    }

    return false;
}

#endif // LIGHT_CLUSTER_INCLUDED
