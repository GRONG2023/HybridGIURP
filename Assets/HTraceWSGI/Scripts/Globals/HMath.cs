
using HTraceWSGI.Scripts.Data.Public;
using UnityEngine;

namespace HTraceWSGI.Scripts.Globals
{
	public static class HMath
	{
		/// <summary>
		/// Remap from one range to another
		/// </summary>
		/// <param name="input"></param>
		/// <param name="oldLow"></param>
		/// <param name="oldHigh"></param>
		/// <param name="newLow"></param>
		/// <param name="newHigh"></param>
		/// <returns></returns>
		public static float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
		{
			float t = Mathf.InverseLerp(oldLow, oldHigh, input);
			return Mathf.Lerp(newLow, newHigh, t);
		}

		/// <summary>
		/// Thickness value pre-calculation for GI
		/// </summary>
		/// <param name="baseThickness"></param>
		/// <param name="camera"></param>
		/// <returns></returns>
		public static Vector2 ThicknessBias(float baseThickness, Camera camera)
		{
			baseThickness = Remap(baseThickness, 0f, 1f, 0f, 0.5f);
			float n = camera.nearClipPlane;
			float f = camera.farClipPlane;
			float thicknessScale = 1.0f / (1.0f + baseThickness);
			float thicknessBias = -n / (f - n) * (baseThickness * thicknessScale);
			return new Vector2(thicknessScale, thicknessBias);
		}

		public static Vector4 ComputeViewportScaleAndLimit(Vector2Int viewportSize, Vector2Int bufferSize)
		{
			return new Vector4(ComputeViewportScale(viewportSize.x, bufferSize.x), // Scale(x)
				ComputeViewportScale(viewportSize.y, bufferSize.y), // Scale(y)
				ComputeViewportLimit(viewportSize.x, bufferSize.x), // Limit(x)
				ComputeViewportLimit(viewportSize.y, bufferSize.y)); // Limit(y)
		}

		public static float PixelSpreadTangent(float Fov, int Width, int Height)
		{
			return Mathf.Tan(Fov * Mathf.Deg2Rad * 0.5f) * 2.0f / Mathf.Min(Width, Height);
		}

		public static Vector3Int CalculateVoxelResolution(VoxelizationSettings voxelizationSettings)
		{
			Vector3Int resolutionResult = new Vector3Int();
            float t = (voxelizationSettings.VoxelDensity - 0.0f) / (1.0f - 0.0f);
            float resolution = (1.0f-t)*HConstants.ClampVoxelsResolution.x  + t*HConstants.ClampVoxelsResolution.y;
			//float resolution = HMath.Remap(voxelizationSettings.VoxelDensity, 0f, 1f, HConstants.ClampVoxelsResolution.x, HConstants.ClampVoxelsResolution.y);
			resolutionResult.x = Mathf.CeilToInt(resolution);
			resolutionResult.y = Mathf.CeilToInt(resolution);
			float targetHeight = (voxelizationSettings.OverrideBoundsHeightEnable == false ? voxelizationSettings.VoxelBounds : voxelizationSettings.OverrideBoundsHeight);
			resolutionResult.z = Mathf.CeilToInt(targetHeight / (voxelizationSettings.VoxelBounds / resolution));

            //Debug.Log("resolution = "+ resolution+ " t = "+t);
			resolutionResult.x = HMath.DevisionBy32(resolutionResult.x);
			resolutionResult.y = HMath.DevisionBy32(resolutionResult.y);
			resolutionResult.z = HMath.DevisionBy32(resolutionResult.z);

			return resolutionResult;
		}

		public static Vector3Int CalculateVoxelResolutionByVoxelSize(float voxelSize, Vector3Int realBounds)
		{
			Vector3Int resolutionResult = new Vector3Int();
			resolutionResult.x = Mathf.CeilToInt(Mathf.Clamp(realBounds.x / voxelSize, HConstants.ClampVoxelsResolution.x, HConstants.ClampVoxelsResolution.y));
			resolutionResult.y = Mathf.CeilToInt(Mathf.Clamp(realBounds.y / voxelSize, HConstants.ClampVoxelsResolution.x, HConstants.ClampVoxelsResolution.y));
			resolutionResult.z = Mathf.CeilToInt(Mathf.Clamp(realBounds.z / voxelSize, HConstants.ClampVoxelsResolution.x, HConstants.ClampVoxelsResolution.y));

			resolutionResult.x = HMath.DevisionBy32(resolutionResult.x);
			resolutionResult.y = HMath.DevisionBy32(resolutionResult.y);
			resolutionResult.z = HMath.DevisionBy32(resolutionResult.z);

			return resolutionResult;
		}

		public static float CalculateVoxelSizeInCM_UI(int bounds, float density)
		{
			float resolution = Mathf.CeilToInt(bounds / (bounds / HMath.Remap(density, 0f, 1f, HConstants.ClampVoxelsResolution.x, HConstants.ClampVoxelsResolution.y)));
			return bounds / resolution * 100f; //100 -> cm
		}

		public static float TexturesSizeInMB_UI(int voxelBounds, float density, bool overrideGroundEnable, int GroundLevel)
		{
			float resolution = voxelBounds / (voxelBounds / HMath.Remap(density, 0f, 1f, HConstants.ClampVoxelsResolution.x, HConstants.ClampVoxelsResolution.y));
			float voxelSize = voxelBounds / resolution;
			float textureResolution = resolution * resolution;
			textureResolution *= overrideGroundEnable == true ? (GroundLevel / voxelSize) : resolution;
			float colorMemorySize = textureResolution * 32 / (1024 * 1024 * 8);
			float positionMemorySize = (textureResolution * 32 / (1024 * 1024 * 8)) + (textureResolution * 8 / (1024 * 1024 * 8));

			return colorMemorySize + positionMemorySize;
		}

		public static float TexturesSizeInMB_UI(Vector3Int voxelsRelosution, VoxelizationUpdateMode voxelizationUpdateMode)
		{
			float textureResolution = voxelsRelosution.x * voxelsRelosution.y * voxelsRelosution.z;
			float textureDataMemorySize = textureResolution * 32 / (1024 * 1024 * 8); //32 bits
			float textureOccupancyMemorySize = (textureResolution * 8 / (1024 * 1024 * 8)); //8 bits
			textureOccupancyMemorySize *= 1.33f; //mipmaps
			float textureIntermediateMemorySize = ((textureResolution / (4^3)) * 8 / (1024 * 1024 * 8)); //8 bits

			if (voxelizationUpdateMode == VoxelizationUpdateMode.Partial)
				textureDataMemorySize *= 2f;

			return textureDataMemorySize + textureOccupancyMemorySize + textureIntermediateMemorySize;
		}

		public static Vector3Int CalculateVoxelResolution_UI(int voxelBounds, float density, bool overrideGroundEnable, int GroundLevel)
		{
			Vector3Int resolutionResult = new Vector3Int();

            float t = (density - 0.0f) / (1.0f - 0.0f);
            float resolution = (1.0f - t) * HConstants.ClampVoxelsResolution.x + t * HConstants.ClampVoxelsResolution.y;

            //float resolution = HMath.Remap(density, 0f, 1f, HConstants.ClampVoxelsResolution.x, HConstants.ClampVoxelsResolution.y);
			resolutionResult.x = Mathf.CeilToInt(resolution);
			resolutionResult.y = Mathf.CeilToInt(resolution);

			float height = (overrideGroundEnable == false ? voxelBounds : GroundLevel);
			resolutionResult.z = Mathf.CeilToInt(height / (voxelBounds / resolution));

			resolutionResult.x = HMath.DevisionBy32(resolutionResult.x);
			resolutionResult.y = HMath.DevisionBy32(resolutionResult.y);
			resolutionResult.z = HMath.DevisionBy32(resolutionResult.z);

            return resolutionResult;
		}

		public static Vector3 Truncate(this Vector3 input, int digits)
		{
			return new Vector3(input.x.RoundTail(digits), input.y.RoundTail(digits), input.z.RoundTail(digits));
		}

		public static Vector3 Ceil(this Vector3 input, int digits)
		{
			return new Vector3(input.x.RoundToCeilTail(digits), input.y.RoundToCeilTail(digits), input.z.RoundToCeilTail(digits));
		}

		public static float RoundTail(this float value, int digits)
		{
			float mult = Mathf.Pow(10.0f, digits);
			float result = Mathf.Round(mult * value) / mult;
			return result;
		}

		public static float RoundToCeilTail(this float value, int digits)
		{
			float mult = Mathf.Pow(10.0f, digits);
			float result = Mathf.Ceil(mult * value) / mult;
			return result;
		}

		public static int CalculateStepCountSSGI(float giRadius, float giAccuracy)
		{
			if (giRadius <= 25.0f)
			{
				//5 -> 16, 10 -> 20, 25 -> 25
				return Mathf.FloorToInt((-0.0233f * giRadius * giRadius + 1.15f * giRadius + 10.833f) * giAccuracy);
			}

			//50 -> 35, 100 -> 50, 150 -> 64
			return Mathf.FloorToInt((-0.0002f * giRadius * giRadius + 0.33f * giRadius + 19f) * giAccuracy);
		}

		public static Vector2Int CalculateDepthPyramidResolution(Vector2Int screenResolution, int lowestMipLevel)
		{
			int lowestMipScale = (int)Mathf.Pow(2.0f, lowestMipLevel);
			Vector2Int lowestMipResolutiom = new Vector2Int(Mathf.CeilToInt( (float)screenResolution.x / (float)lowestMipScale), 
				Mathf.CeilToInt( (float)screenResolution.y / (float)lowestMipScale));

			Vector2Int paddedDepthPyramidResolution = lowestMipResolutiom * lowestMipScale;
			return paddedDepthPyramidResolution; 
		}
		public static Matrix4x4 ComputeFrustumCorners(Camera cam)
		{
			Transform cameraTransform = cam.transform;
            
			Vector3[] frustumCorners = new Vector3[4];
			cam.CalculateFrustumCorners(new Rect(0, 0, 1 / cam.rect.xMax, 1 / cam.rect.yMax), cam.farClipPlane, cam.stereoActiveEye, frustumCorners);

			Vector3 topLeft     = cameraTransform.TransformVector(frustumCorners[0]);
			Vector3 bottomLeft  = cameraTransform.TransformVector(frustumCorners[1]);
			Vector3 bottomRight = cameraTransform.TransformVector(frustumCorners[2]); 

			Matrix4x4 frustumVectorsArray = Matrix4x4.identity;
			frustumVectorsArray.SetRow(0, bottomLeft);
			frustumVectorsArray.SetRow(1, bottomLeft + (bottomRight - bottomLeft) * 2);
			frustumVectorsArray.SetRow(2, bottomLeft + (topLeft - bottomLeft) * 2);

			return frustumVectorsArray;
		}
		
		public static Matrix4x4 Compute4FrustumCorners(Camera cam)
		{
			Transform cameraTransform = cam.transform;

			Vector3[] frustumCorners = new Vector3[4];  
			cam.CalculateFrustumCorners(new Rect(0, 0, 1 / cam.rect.xMax, 1 / cam.rect.yMax), cam.farClipPlane, cam.stereoActiveEye, frustumCorners);

			// Transform to world space
			Vector3 topLeft     = cameraTransform.TransformVector(frustumCorners[0]);
			Vector3 bottomLeft  = cameraTransform.TransformVector(frustumCorners[1]);
			Vector3 bottomRight = cameraTransform.TransformVector(frustumCorners[2]);
			Vector3 topRight    = cameraTransform.TransformVector(frustumCorners[3]);

			Matrix4x4 frustum = new Matrix4x4();
			frustum.SetRow(0, bottomLeft);
			frustum.SetRow(1, topLeft);
			frustum.SetRow(2, topRight);
			frustum.SetRow(3, bottomRight);

			return frustum;
		}

		//internal static Vector3 OptimizeForVoxelization(this Vector3 position, VoxelizationExactData exactData)
		internal static Vector3 OptimizeForVoxelization(this Vector3 position, float VoxelSize)
		{
            //Debug.Log("position = " + position);
			Vector3 newPosition = new Vector3(Mathf.Round(position.x / VoxelSize) * VoxelSize,
				Mathf.Round(position.y / VoxelSize) * VoxelSize,
				Mathf.Round(position.z / VoxelSize) * VoxelSize);
            //Debug.Log("position222 = " + newPosition);
            return newPosition;
		}

		private static int DevisionBy32(int value)
		{
			return value % 32 == 0 ? value : DevisionBy32(value + 1);
		}

		private static int DevisionBy(int value, int devisionValue)
		{
			return value % devisionValue == 0 ? value : DevisionBy(value + 1, devisionValue);
		}

		private static float ComputeViewportScale(int viewportSize, int bufferSize)
		{
			float rcpBufferSize = 1.0f / bufferSize;

			// Scale by (vp_dim / buf_dim).
			return viewportSize * rcpBufferSize;
		}

		private static float ComputeViewportLimit(int viewportSize, int bufferSize)
		{
			float rcpBufferSize = 1.0f / bufferSize;

			// Clamp to (vp_dim - 0.5) / buf_dim.
			return (viewportSize - 0.5f) * rcpBufferSize;
		}
	}
}
