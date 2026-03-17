//pipelinedefine
#define H_HDRP

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.PipelinesConfigurator;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = System.Object;

using UnityEngine.Rendering.Universal;

namespace HTraceWSGI.Scripts.Patcher
{
	internal static class HPatcher
	{
		private static bool TryGetValue(object obj, string fieldName, out object value)
		{
			value = null;
			if (obj == null)
			{
				HExtensions.DebugPrint(DebugType.Error, $"obj is null, fieldName: {fieldName}");
				return false;
			}
			FieldInfo fi = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (fi == null)
			{
				HExtensions.DebugPrint(DebugType.Error, $"FieldInfo is null, fieldName: {fieldName}");
				return false;
			}
			value = fi.GetValue(obj);
			return value != null;
		}
		
		private static bool IsGlobalSettingsWithHtraceResources()
		{
			RenderPipelineGlobalSettings globalSettings = GraphicsSettings.GetSettingsForRenderPipeline<UniversalRenderPipeline>();
			if (globalSettings == null)
			{
				HExtensions.DebugPrint(DebugType.Error, "GlobalSettings for HDRenderPipeline not found.");
				return false;
			}
			
#if  UNITY_6000_0_OR_NEWER
			if (!TryGetValue(globalSettings, "m_Settings", out var mSettings) ||
			    !TryGetValue(mSettings, "m_SettingsList", out var settingsList) ||
			    !TryGetValue(settingsList, "m_List", out var rawList) ||
			    rawList is not IList list)
			{
				HExtensions.DebugPrint(DebugType.Error, "List with shaders in RenderPipelineGlobalSettings not found.");
				return false;
			}
			
			ComputeShader ssgiCs  = null;
			ComputeShader rtDeferCs = null;
			
			foreach (var item in list)
			{
				if (item == null) continue;

				var t = item.GetType().Name;
				if (t.IndexOf("HDRenderPipelineRuntimeShaders", StringComparison.OrdinalIgnoreCase) >= 0 &&
				    TryGetValue(item, "m_ScreenSpaceGlobalIlluminationCS", out var ssgiCompute))
					ssgiCs = ssgiCompute as ComputeShader;

				if (t.IndexOf("HDRPRayTracingResources", StringComparison.OrdinalIgnoreCase) >= 0 &&
				    TryGetValue(item, "m_DeferredRayTracingCS", out var raytracingDeferredCompute))
					rtDeferCs = raytracingDeferredCompute as ComputeShader;
			}
#else
			if (!TryGetValue(globalSettings, "m_RenderPipelineResources", out var hdrpRes) ||
			    !TryGetValue(hdrpRes, "shaders",                         out var shaders) ||
			    !TryGetValue(shaders, "screenSpaceGlobalIlluminationCS", out var ssgiObj))
				return false;

			if (!TryGetValue(globalSettings, "m_RenderPipelineRayTracingResources", out var rtRes) ||
			    !TryGetValue(rtRes,          "deferredRaytracingCS",                out var deferObj))
				return false;

			var ssgiCs    = ssgiObj as ComputeShader;
			var rtDeferCs = deferObj as ComputeShader;
#endif
			
			return ssgiCs  != null &&
			       rtDeferCs != null &&
			       ssgiCs.name.IndexOf("HTrace", StringComparison.OrdinalIgnoreCase)  >= 0 &&
			       rtDeferCs.name.IndexOf("HTrace", StringComparison.OrdinalIgnoreCase) >= 0;
		}
		
		//Project settings - Global Settings - Resources
		public static void RenderPipelineRuntimeResourcesPatch(bool forceReplace = false)
		{
			if (IsGlobalSettingsWithHtraceResources() == true)
				return;

			CreateRpResourcesFolders();

		}
		

		private static void CreateRpResourcesFolders()
		{
			if (!Directory.Exists(Path.Combine(ConfiguratorUtils.GetHTraceFolderPath(), "RP Resources")))
			{
				Directory.CreateDirectory(Path.Combine(ConfiguratorUtils.GetHTraceFolderPath(), "RP Resources"));
				AssetDatabase.Refresh();
			}
			
			if (!Directory.Exists(Path.Combine(ConfiguratorUtils.GetHTraceFolderPath(), "RP Resources", "HDRP")))
			{
				Directory.CreateDirectory(Path.Combine(ConfiguratorUtils.GetHTraceFolderPath(), "RP Resources", "HDRP"));
				AssetDatabase.Refresh();
			}
			
			// if (!Directory.Exists(Path.Combine(ConfiguratorUtils.GetHTraceFolderPath(), "RP Resources", "URP")))
			// {
			// 	Directory.CreateDirectory(Path.Combine(ConfiguratorUtils.GetHTraceFolderPath(), "RP Resources", "URP"));
			// 	AssetDatabase.Refresh();
			// }
			
			// if (!Directory.Exists(Path.Combine(ConfiguratorUtils.GetHTraceFolderPath(), "RP Resources", "BIRP")))
			// {
			// 	Directory.CreateDirectory(Path.Combine(ConfiguratorUtils.GetHTraceFolderPath(), "RP Resources", "BIRP"));
			// 	AssetDatabase.Refresh();
			// }
		}

	}
}
#endif

