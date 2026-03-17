//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Data.Private;
using UnityEngine;
using UnityEngine.Rendering;

namespace HTraceWSGI.Scripts.Passes.Shared
{
	public class VoxelizationFunctionsShared
	{
		//globals
		internal static readonly int g_OffsetAxisIndex      = Shader.PropertyToID("_OffsetAxisIndex");
		internal static readonly int g_AxisOffset           = Shader.PropertyToID("_AxisOffset");
		internal static readonly int g_CullingTrim          = Shader.PropertyToID("_CullingTrim");
		internal static readonly int g_OctantOffset         = Shader.PropertyToID("_OctantOffset");
		internal static readonly int g_CullingTrimAxis      = Shader.PropertyToID("_CullingTrimAxis");

	}
}
