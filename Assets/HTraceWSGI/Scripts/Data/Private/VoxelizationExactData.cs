//pipelinedefine
#define H_HDRP

using System;
using HTraceWSGI.Scripts.Data.Public;
using HTraceWSGI.Scripts.Globals;
using UnityEngine;

namespace HTraceWSGI.Scripts.Data.Private
{
	[Serializable]
	internal class VoxelizationExactData
	{
		public Vector3Int Resolution;
		public Vector3 Bounds;
		
		// Clipmap data
		public Vector3Int ClipmapResolution;
		public Vector3 ClipmapBounds;
		public float ClipmapVoxelSize;
		public float ClipmapVoxelsPerMeter;

		public float VoxelSize
		{
			get { return Bounds.x / Resolution.x; }
		}

		public float VoxelsPerMeter
		{
			get { return Resolution.x / Bounds.x; }
		}


		public void UpdateData(VoxelizationSettings voxelizationSettings)
		{
			// Calculate base voxel level (Level 0)
			Vector3Int targetResolution = HMath.CalculateVoxelResolution(voxelizationSettings);
			Vector3Int realBounds = new Vector3Int(voxelizationSettings.VoxelBounds, voxelizationSettings.VoxelBounds,
				voxelizationSettings.OverrideBoundsHeightEnable == false ? voxelizationSettings.VoxelBounds : voxelizationSettings.OverrideBoundsHeight);
			float realVoxelSize  = (float)realBounds.x / targetResolution.x;
			float exactVoxelSize = realVoxelSize.RoundToCeilTail(2);
			
			//float maxVoxelSize = 0.16f; // default case
			float maxVoxelSize = 0.5f; // default case
			if (HConfig.MAX_VOXEL_BOUNDS != 80) //not default case
			{
				maxVoxelSize = ((float)realBounds.x / targetResolution.x).RoundToCeilTail(2);
			}
			exactVoxelSize = Mathf.Clamp(exactVoxelSize, 0.08f, maxVoxelSize); // we want voxel size between 8 and 16 cm

			var resultResolution = HMath.CalculateVoxelResolutionByVoxelSize(exactVoxelSize, realBounds);
            Resolution = resultResolution;
            //Debug.Log("targetResolution = " + targetResolution+ "  resultResolution = "+ resultResolution+ " realBounds = "+ realBounds+ " exactVoxelSize = "+ exactVoxelSize);
			Bounds     = new Vector3(resultResolution.x * exactVoxelSize, resultResolution.y * exactVoxelSize, resultResolution.z * exactVoxelSize);
			
		}
	}
}
