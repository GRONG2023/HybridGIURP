//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Data.Public;
using UnityEngine;
using LightingSettings = HTraceWSGI.Scripts.Data.Public.LightingSettings;

namespace HTraceWSGI.Scripts.Data.Private
{
	internal static class HSettings
	{
		[SerializeField]
		internal static GeneralSettings GeneralSettings;
		[SerializeField]
		internal static ScreenSpaceLightingSettings ScreenSpaceLightingSettings;
		[SerializeField]
		internal static DebugSettings DebugSettings;
		[SerializeField]
		internal static VoxelizationSettings VoxelizationSettings;
		[SerializeField]
		internal static LightingSettings LightingSettings;
		[SerializeField]
		internal static ReflectionIndirectLightingSettings ReflectionIndirectLightingSettings;
	}
}
