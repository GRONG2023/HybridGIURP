//pipelinedefine
#define H_HDRP

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace HTraceWSGI.Scripts.Globals
{
	internal static class HConstants
	{
		internal const int HASH_STORAGE_SIZE          = 512000 * 2;
		internal const int HASH_UPDATE_FRACTION       = 10;
		internal const int PERSISTENT_HISTORY_SAMPLES = 4;
		internal const int OCTAHEDRAL_SIZE          = 4;
		
		//Voxelization
		internal static readonly Vector2 ShadowmapResolution = new Vector2(1024, 1024);
		internal const int OCTANTS_FRAMES_LENGTH = 5;
		internal const int MAX_LOD_LEVEL = 10;

		internal static Vector2 ClampVoxelsResolution = new Vector2(64f, 256f); //min - 64 VoxelResolution, max - 512 VoxelResolution
	}
}
