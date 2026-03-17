using System;
using System.Collections.Generic;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Services.VoxelCameras;
using UnityEngine;

namespace HTraceWSGI.Scripts.Data.Private
{
	internal enum OffsetAxisIndex
	{
		AxisXPos = 0,
		AxisYPos = 1,
		AxisZPos = 2,
		AxisXNeg = 3,
		AxisYNeg = 4,
		AxisZNeg = 5,
	}

	internal enum OctantIndex
	{
		None           = 0,
		OctantA        = 1,
		OctantB        = 2,
		OctantC        = 3,
		OctantD        = 4,
		DynamicObjects = 5,
	}
	
	[Serializable]
	internal static class VoxelizationRuntimeData
	{
		public static VoxelCamera             VoxelCamera           { get; set; } = null;

		public static OffsetWorldPosition OffsetWorldPosition = OffsetWorldPosition.zero;
		public static OctantIndex         OctantIndex         = OctantIndex.None;
		public static OffsetAxisIndex     OffsetAxisIndex     = OffsetAxisIndex.AxisXPos;
		public static bool                FullVoxelization;
		public static int                 TextureSwapCounter;
		public static int                 TextureOutputCounter;

		[SerializeField] private static float _prevDensityUI              = 0f;
		[SerializeField] private static int   _prevVoxelBoundsUI          = 0;
		[SerializeField] private static int   _prevOverrideBoundsHeightUI = 0;

		public static Action OnReallocTextures;
		
		public static void Initialize()
		{
			OffsetWorldPosition = OffsetWorldPosition.zero;
			OctantIndex         = OctantIndex.None;
			OffsetAxisIndex     = OffsetAxisIndex.AxisXPos;
			
			FullVoxelization     = true;
			TextureSwapCounter   = 0;
			TextureOutputCounter = 0;
		}

		public static void SetParamsForApplyButton(float prevDensityUI, int prevVoxelBoundsUI, int prevOverrideBoundsHeightUI)
		{
			_prevDensityUI              = prevDensityUI;
			_prevVoxelBoundsUI          = prevVoxelBoundsUI;
			_prevOverrideBoundsHeightUI = prevOverrideBoundsHeightUI;
		}

		public static bool CheckPrevParams(float voxelDensity, int voxelBounds, int overrideBoundsHeight)
		{
			return Mathf.Abs(voxelDensity - _prevDensityUI) > Mathf.Epsilon || voxelBounds != _prevVoxelBoundsUI || overrideBoundsHeight != _prevOverrideBoundsHeightUI;
		}
	}

	internal struct OffsetWorldPosition
	{
		public float AxisXPos;
		public float AxisYPos;
		public float AxisZPos;
		public float AxisXNeg;
		public float AxisYNeg;
		public float AxisZNeg;

		public OffsetWorldPosition(float axisXPos, float axisYPos, float axisZPos, float axisXNeg, float axisYNeg, float axisZNeg)
		{
			AxisXPos = axisXPos;
			AxisYPos = axisYPos;
			AxisZPos = axisZPos;
			AxisXNeg = axisXNeg;
			AxisYNeg = axisYNeg;
			AxisZNeg = axisZNeg;
		}

		public static OffsetWorldPosition zero
		{
			get => OffsetWorldPosition.zeroVector;
		}

		private static readonly OffsetWorldPosition zeroVector = new OffsetWorldPosition(0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
			0.0f);

		public static OffsetWorldPosition operator +(OffsetWorldPosition a, OffsetWorldPosition b)
		{
			return new OffsetWorldPosition(
				a.AxisXPos + b.AxisXPos,
				a.AxisYPos + b.AxisYPos,
				a.AxisZPos + b.AxisZPos,
				a.AxisXNeg + b.AxisXNeg,
				a.AxisYNeg + b.AxisYNeg,
				a.AxisZNeg + b.AxisZNeg
			);
		}

		public OffsetAxisIndex MaxAxisOffset()
		{
			Dictionary<OffsetAxisIndex, float> dictionary = new Dictionary<OffsetAxisIndex, float>()
			{
				{OffsetAxisIndex.AxisXPos, AxisXPos},
				{OffsetAxisIndex.AxisYPos, AxisYPos},
				{OffsetAxisIndex.AxisZPos, AxisZPos},
				{OffsetAxisIndex.AxisXNeg, AxisXNeg},
				{OffsetAxisIndex.AxisYNeg, AxisYNeg},
				{OffsetAxisIndex.AxisZNeg, AxisZNeg},
			};

			float           maxValue  = -1;
			OffsetAxisIndex axisIndex = OffsetAxisIndex.AxisXPos;
			foreach (var element in dictionary)
			{
				if (element.Value > maxValue)
				{
					axisIndex = element.Key;
					maxValue  = element.Value;
				}
			}

			return axisIndex;
		}
	}
}
