//Common.hlsl in Core RP
#pragma once


// Intrinsic isnan can't be used because it require /Gic to be enabled on fxc that we can't do. So use AnyIsNan instead
bool IsNaN(float x)
{
	return (asuint(x) & 0x7FFFFFFF) > 0x7F800000;
}

bool AnyIsNaN(float2 v)
{
	return (IsNaN(v.x) || IsNaN(v.y));
}

bool AnyIsNaN(float3 v)
{
	return (IsNaN(v.x) || IsNaN(v.y) || IsNaN(v.z));
}

bool AnyIsNaN(float4 v)
{
	return (IsNaN(v.x) || IsNaN(v.y) || IsNaN(v.z) || IsNaN(v.w));
}

bool IsInf(float x)
{
	return (asuint(x) & 0x7FFFFFFF) == 0x7F800000;
}

bool AnyIsInf(float2 v)
{
	return (IsInf(v.x) || IsInf(v.y));
}

bool AnyIsInf(float3 v)
{
	return (IsInf(v.x) || IsInf(v.y) || IsInf(v.z));
}

bool AnyIsInf(float4 v)
{
	return (IsInf(v.x) || IsInf(v.y) || IsInf(v.z) || IsInf(v.w));
}
