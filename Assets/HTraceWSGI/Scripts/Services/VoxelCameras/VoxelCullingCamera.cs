//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Data.Private;
using UnityEngine;

using UnityEngine.Rendering.Universal;


namespace HTraceWSGI.Scripts.Services.VoxelCameras
{	
	[ExecuteInEditMode] 
	internal class VoxelCullingCamera : MonoBehaviour
	{
		public Camera Camera
		{
			get { return _camera; }
		}

		private Camera _camera;

		public void Initialize()
		{
			CreateCamera();
			SetParams();
		}

		public void UpdateCamera()
		{
			transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		}

		public void ExecuteUpdate()
		{
			CullingTransformCamera();
			_camera.hideFlags    = HSettings.DebugSettings.ShowBowels ? HideFlags.None : HideFlags.HideInHierarchy;
		}

		// Size = height / 2
		// Aspect = width / height
		//
		// height = 2f * size;
		// width = height * aspect;

		private void CullingTransformCamera()
		{
			transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

			float scale            = VoxelizationRuntimeData.OctantIndex == OctantIndex.DynamicObjects ? 2f : 1f;
			bool  isDynamicObjects = VoxelizationRuntimeData.OctantIndex == OctantIndex.DynamicObjects;
			switch (VoxelizationRuntimeData.OffsetAxisIndex)
			{
				case OffsetAxisIndex.AxisXPos:
					transform.localEulerAngles = new Vector3(0,                                          90f, 90);
					transform.localPosition    = new Vector3(-HSettings.VoxelizationSettings.ExactData.Bounds.x / 4f, 0,   0);

					_camera.farClipPlane     = isDynamicObjects ? HSettings.VoxelizationSettings.ExactData.Bounds.x : VoxelizationRuntimeData.OffsetWorldPosition.AxisXPos;
					_camera.orthographicSize = HSettings.VoxelizationSettings.ExactData.Bounds.z * scale / 4f;
					_camera.aspect           = HSettings.VoxelizationSettings.ExactData.Bounds.x * scale / 4f / _camera.orthographicSize;
					MoveAxisX(1f);
					break;
				case OffsetAxisIndex.AxisXNeg:
					transform.localEulerAngles = new Vector3(0,                                         -90f, 90);
					transform.localPosition    = new Vector3(HSettings.VoxelizationSettings.ExactData.Bounds.x / 4f, 0,    0);

					_camera.farClipPlane     = isDynamicObjects ? HSettings.VoxelizationSettings.ExactData.Bounds.x : VoxelizationRuntimeData.OffsetWorldPosition.AxisXNeg;
					_camera.orthographicSize = HSettings.VoxelizationSettings.ExactData.Bounds.z * scale / 4f;
					_camera.aspect           = HSettings.VoxelizationSettings.ExactData.Bounds.x * scale / 4f / _camera.orthographicSize;
					MoveAxisX(-1f);
					break;
				case OffsetAxisIndex.AxisYPos:
					transform.localEulerAngles = new Vector3(-180, 0, 0);
					transform.localPosition    = new Vector3(0,    0, HSettings.VoxelizationSettings.ExactData.Bounds.z / 4f);

					_camera.farClipPlane     = isDynamicObjects ? HSettings.VoxelizationSettings.ExactData.Bounds.z : VoxelizationRuntimeData.OffsetWorldPosition.AxisYPos;
					_camera.orthographicSize = HSettings.VoxelizationSettings.ExactData.Bounds.x * scale / 4f;
					_camera.aspect           = 1f; //always quad
					MoveAxisY(1f);
					break;
				case OffsetAxisIndex.AxisYNeg:
					transform.localEulerAngles = new Vector3(0, 0, -180f);
					transform.localPosition    = new Vector3(0, 0, -HSettings.VoxelizationSettings.ExactData.Bounds.z / 4f);

					_camera.farClipPlane     = isDynamicObjects ? HSettings.VoxelizationSettings.ExactData.Bounds.z : VoxelizationRuntimeData.OffsetWorldPosition.AxisYNeg;
					_camera.orthographicSize = HSettings.VoxelizationSettings.ExactData.Bounds.x * scale / 4f;
					_camera.aspect           = 1f; //always quad
					MoveAxisY(-1f);
					break;
				case OffsetAxisIndex.AxisZPos:
					transform.localEulerAngles = new Vector3(90f, 0,                                         0);
					transform.localPosition    = new Vector3(0,   HSettings.VoxelizationSettings.ExactData.Bounds.x / 4f, 0);

					_camera.farClipPlane     = isDynamicObjects ? HSettings.VoxelizationSettings.ExactData.Bounds.x : VoxelizationRuntimeData.OffsetWorldPosition.AxisZPos;
					_camera.orthographicSize = HSettings.VoxelizationSettings.ExactData.Bounds.z * scale / 4f;
					_camera.aspect           = HSettings.VoxelizationSettings.ExactData.Bounds.x * scale / 4f / _camera.orthographicSize;
					MoveAxisZ(1f);
					break;
				case OffsetAxisIndex.AxisZNeg:
					transform.localEulerAngles = new Vector3(-90f, 0,                                          0);
					transform.localPosition    = new Vector3(0,    -HSettings.VoxelizationSettings.ExactData.Bounds.x / 4f, 0);

					_camera.farClipPlane     = isDynamicObjects ? HSettings.VoxelizationSettings.ExactData.Bounds.x : VoxelizationRuntimeData.OffsetWorldPosition.AxisZNeg;
					_camera.orthographicSize = HSettings.VoxelizationSettings.ExactData.Bounds.z * scale / 4f;
					_camera.aspect           = HSettings.VoxelizationSettings.ExactData.Bounds.x * scale / 4f / _camera.orthographicSize;
					MoveAxisZ(-1f);
					break;
			}
		}

		private void MoveAxisX(float sign)
		{
			Vector3 finalPos = Vector3.zero;
			switch (VoxelizationRuntimeData.OctantIndex)
			{
				case OctantIndex.OctantA:
					finalPos.x += -sign * HSettings.VoxelizationSettings.ExactData.Bounds.x / 4;
					finalPos.y += -HSettings.VoxelizationSettings.ExactData.Bounds.y / 4;
					finalPos.z += HSettings.VoxelizationSettings.ExactData.Bounds.z / 4;
					break;
				case OctantIndex.OctantB:
					finalPos.x += -sign * HSettings.VoxelizationSettings.ExactData.Bounds.x / 4;
					finalPos.y += HSettings.VoxelizationSettings.ExactData.Bounds.y / 4;
					finalPos.z += HSettings.VoxelizationSettings.ExactData.Bounds.z / 4;
					break;
				case OctantIndex.OctantC:
					finalPos.x += -sign * HSettings.VoxelizationSettings.ExactData.Bounds.x / 4;
					finalPos.y += -HSettings.VoxelizationSettings.ExactData.Bounds.y / 4;
					finalPos.z += -HSettings.VoxelizationSettings.ExactData.Bounds.z / 4;
					break;
				case OctantIndex.OctantD:
					finalPos.x += -sign * HSettings.VoxelizationSettings.ExactData.Bounds.x / 4;
					finalPos.y += HSettings.VoxelizationSettings.ExactData.Bounds.y / 4;
					finalPos.z += -HSettings.VoxelizationSettings.ExactData.Bounds.z / 4;
					break;
				case OctantIndex.DynamicObjects:
				case OctantIndex.None:
					finalPos.x += -sign * HSettings.VoxelizationSettings.ExactData.Bounds.x / 4;
					break;
			}

			transform.localPosition += finalPos;
		}

		private void MoveAxisY(float sign)
		{
			Vector3 finalPos = Vector3.zero;
			switch (VoxelizationRuntimeData.OctantIndex)
			{
				case OctantIndex.OctantA:
					finalPos.x += HSettings.VoxelizationSettings.ExactData.Bounds.x / 4;
					finalPos.y += -HSettings.VoxelizationSettings.ExactData.Bounds.y / 4;
					finalPos.z += sign * HSettings.VoxelizationSettings.ExactData.Bounds.z / 4;
					break;
				case OctantIndex.OctantB:
					finalPos.x += HSettings.VoxelizationSettings.ExactData.Bounds.x / 4;
					finalPos.y += HSettings.VoxelizationSettings.ExactData.Bounds.y / 4;
					finalPos.z += sign * HSettings.VoxelizationSettings.ExactData.Bounds.z / 4;
					break;
				case OctantIndex.OctantC:
					finalPos.x += -HSettings.VoxelizationSettings.ExactData.Bounds.x / 4;
					finalPos.y += -HSettings.VoxelizationSettings.ExactData.Bounds.y / 4;
					finalPos.z += sign * HSettings.VoxelizationSettings.ExactData.Bounds.z / 4;
					break;
				case OctantIndex.OctantD:
					finalPos.x += -HSettings.VoxelizationSettings.ExactData.Bounds.x / 4;
					finalPos.y += HSettings.VoxelizationSettings.ExactData.Bounds.y / 4;
					finalPos.z += sign * HSettings.VoxelizationSettings.ExactData.Bounds.z / 4;
					break;
				case OctantIndex.DynamicObjects:
		
					finalPos.z += sign * HSettings.VoxelizationSettings.ExactData.Bounds.z / 4;
					break;
			}

			transform.localPosition += finalPos;
		}

		private void MoveAxisZ(float sign)
		{
			Vector3 finalPos = Vector3.zero;

			switch (VoxelizationRuntimeData.OctantIndex)
			{
				case OctantIndex.OctantA:
					finalPos.x += -HSettings.VoxelizationSettings.ExactData.Bounds.x / 4;
					finalPos.y += sign * HSettings.VoxelizationSettings.ExactData.Bounds.y / 4;
					finalPos.z += HSettings.VoxelizationSettings.ExactData.Bounds.z / 4;
					break;
				case OctantIndex.OctantB:
					finalPos.x += HSettings.VoxelizationSettings.ExactData.Bounds.x / 4;
					finalPos.y += sign * HSettings.VoxelizationSettings.ExactData.Bounds.y / 4;
					finalPos.z += HSettings.VoxelizationSettings.ExactData.Bounds.z / 4;
					break;
				case OctantIndex.OctantC:
					finalPos.x += -HSettings.VoxelizationSettings.ExactData.Bounds.x / 4;
					finalPos.y += sign * HSettings.VoxelizationSettings.ExactData.Bounds.y / 4;
					finalPos.z += -HSettings.VoxelizationSettings.ExactData.Bounds.z / 4;
					break;
				case OctantIndex.OctantD:
					finalPos.x += HSettings.VoxelizationSettings.ExactData.Bounds.x / 4;
					finalPos.y += sign * HSettings.VoxelizationSettings.ExactData.Bounds.y / 4;
					finalPos.z += -HSettings.VoxelizationSettings.ExactData.Bounds.z / 4;
					break;
				case OctantIndex.DynamicObjects:
				
					finalPos.y += sign * HSettings.VoxelizationSettings.ExactData.Bounds.y / 4;
					break;
			}

			transform.localPosition += finalPos;
		}

		private void SetParams()
		{
			_camera.cullingMask      = ~0; //voxelizationData.VoxelizationMask;
			_camera.orthographic     = true;
			_camera.farClipPlane     = 1;
			_camera.nearClipPlane    = 0;
			_camera.orthographicSize = 0;
			_camera.aspect           = 1;
		}

		private void CreateCamera()
		{
			if (_camera == null)
			{
				_camera              = gameObject.AddComponent<Camera>();
				_camera.aspect       = 1f;
				_camera.orthographic = true;
				_camera.enabled      = false;
				_camera.hideFlags    = HSettings.DebugSettings.ShowBowels ? HideFlags.None : HideFlags.HideInHierarchy;

				//var HDCameraData = gameObject.AddComponent<HDAdditionalCameraData>();
				//HDCameraData.customRenderingSettings = true;
				//FrameSettings frameSettings = HDCameraData.renderingPathCustomFrameSettings;
				//HDCameraData.renderingPathCustomFrameSettings = frameSettings;
			}
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if (HSettings.DebugSettings.EnableCamerasVisualization == false)
				return;

			var color = Gizmos.color;

			Vector3 position = Vector3.zero;
			Vector3 size     = Vector3.zero;

			switch (VoxelizationRuntimeData.OffsetAxisIndex)
			{
				case OffsetAxisIndex.AxisXPos:
					position     = _camera.transform.position + new Vector3(-VoxelizationRuntimeData.OffsetWorldPosition.AxisXPos / 2f, 0, 0);
					size         = new Vector3(VoxelizationRuntimeData.OffsetWorldPosition.AxisXPos, 2f * _camera.orthographicSize, 2f * _camera.orthographicSize * _camera.aspect);
					Gizmos.color = new Color(1, 0, 0, 0.3f);
					break;
				case OffsetAxisIndex.AxisXNeg:
					position     = _camera.transform.position + new Vector3(VoxelizationRuntimeData.OffsetWorldPosition.AxisXNeg / 2f, 0, 0);
					size         = new Vector3(VoxelizationRuntimeData.OffsetWorldPosition.AxisXNeg, 2f * _camera.orthographicSize, 2f * _camera.orthographicSize * _camera.aspect);
					Gizmos.color = new Color(1, 0, 0, 0.3f);
					break;
				case OffsetAxisIndex.AxisYPos:
					position     = _camera.transform.position + new Vector3(0f, -VoxelizationRuntimeData.OffsetWorldPosition.AxisYPos / 2f, 0);
					size         = new Vector3(2f * _camera.orthographicSize, VoxelizationRuntimeData.OffsetWorldPosition.AxisYPos, 2f * _camera.orthographicSize * _camera.aspect);
					Gizmos.color = new Color(0, 1, 0, 0.3f);
					break;
				case OffsetAxisIndex.AxisYNeg:
					position     = _camera.transform.position + new Vector3(0f, VoxelizationRuntimeData.OffsetWorldPosition.AxisYPos / 2f, 0);
					size         = new Vector3(2f * _camera.orthographicSize, VoxelizationRuntimeData.OffsetWorldPosition.AxisYPos, 2f * _camera.orthographicSize * _camera.aspect);
					Gizmos.color = new Color(0, 1, 0, 0.3f);
					break;
				case OffsetAxisIndex.AxisZPos:
					position     = _camera.transform.position + new Vector3(0, 0, -VoxelizationRuntimeData.OffsetWorldPosition.AxisZPos / 2f);
					size         = new Vector3(2f * _camera.orthographicSize * _camera.aspect, 2f * _camera.orthographicSize, VoxelizationRuntimeData.OffsetWorldPosition.AxisZPos);
					Gizmos.color = new Color(0, 0, 1, 0.3f);
					break;
				case OffsetAxisIndex.AxisZNeg:
					position     = _camera.transform.position + new Vector3(0, 0, VoxelizationRuntimeData.OffsetWorldPosition.AxisZNeg / 2f);
					size         = new Vector3(2f * _camera.orthographicSize * _camera.aspect, 2f * _camera.orthographicSize, VoxelizationRuntimeData.OffsetWorldPosition.AxisZNeg);
					Gizmos.color = new Color(0, 0, 1, 0.3f);
					break;
			}

			Gizmos.DrawCube(position, size);

			Gizmos.color = color;
		}

#endif
	}
}
