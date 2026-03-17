//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Data.Private;
using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HTraceWSGI.Scripts.Services.VoxelCameras
{
	[ExecuteInEditMode]
	internal class VoxelCamera : MonoBehaviour
	{
		public Camera Camera
		{
			get { return _camera; }
		}

		private Camera                  _camera;
		private VoxelsService           _voxelsService;

		private bool    _dirtyBounds = true;
		private Vector3 _prevVoxelBounds;
		private Vector3 _rememberPos;

		public void Initialize(VoxelsService voxelsService)
		{
			_voxelsService           = voxelsService;
			CreateCamera();
			ExecuteUpdate(null);
			VoxelizationRuntimeData.OnReallocTextures += UpdateCameraFromUI;

			_prevVoxelBounds = HSettings.VoxelizationSettings.ExactData.Bounds;
		}

		public void ExecuteUpdate(Camera camera)
		{
            //Debug.Log("ExecuteUpdate _prevVoxelBounds = " + _prevVoxelBounds + "  " + HSettings.VoxelizationSettings.ExactData.Bounds+ "  _dirtyBounds = "+ _dirtyBounds);
            if (_dirtyBounds == true)
			{
				_dirtyBounds     = false;
				_prevVoxelBounds = HSettings.VoxelizationSettings.ExactData.Bounds;
			}
            //Debug.Log("ExecuteUpdate2 _prevVoxelBounds = " + _prevVoxelBounds+"  "+ HSettings.VoxelizationSettings.ExactData.Bounds);
            _camera.cullingMask      = ~0; //voxelizationData.VoxelizationMask;
			_camera.orthographic     = true;
			_camera.farClipPlane     = .5f * HSettings.VoxelizationSettings.ExactData.Bounds.z;
			_camera.nearClipPlane    = -.5f * HSettings.VoxelizationSettings.ExactData.Bounds.z;
			_camera.orthographicSize = .5f * _prevVoxelBounds.x;
			_camera.aspect           = (.5f * _prevVoxelBounds.x) / (.5f * _prevVoxelBounds.x);
			_camera.hideFlags        = HSettings.DebugSettings.ShowBowels ? HideFlags.None : HideFlags.HideInHierarchy;
			
			//HData.VoxelizationData.ExactData.PreviousVoxelCameraPosition = new Vector3(_camera.transform.position.x, _camera.transform.position.y, _camera.transform.position.z);

			AttachedCameraTranslate(camera, false);
		}

        public void ClipmapExecuteUpdate(Camera camera)
        {
            if (_dirtyBounds == true)
            {
                _dirtyBounds = false;
                _prevVoxelBounds = HSettings.VoxelizationSettings.ExactData.Bounds*2;
            }

            _camera.cullingMask = ~0; //voxelizationData.VoxelizationMask;
            _camera.orthographic = true;
            _camera.farClipPlane = .5f * HSettings.VoxelizationSettings.ExactData.Bounds.z*2;
            _camera.nearClipPlane = -.5f * HSettings.VoxelizationSettings.ExactData.Bounds.z*2;
            _camera.orthographicSize = .5f * _prevVoxelBounds.x;
            _camera.aspect = (.5f * _prevVoxelBounds.x) / (.5f * _prevVoxelBounds.x);
            _camera.hideFlags = HSettings.DebugSettings.ShowBowels ? HideFlags.None : HideFlags.HideInHierarchy;

            //HData.VoxelizationData.ExactData.PreviousVoxelCameraPosition = new Vector3(_camera.transform.position.x, _camera.transform.position.y, _camera.transform.position.z);

            AttachedCameraTranslate(camera, true);
        }

        private void AttachedCameraTranslate(Camera camera, bool isClipmap)
		{
			bool attachToSceneCamera = camera != null && camera.cameraType == CameraType.SceneView && HSettings.DebugSettings.AttachToSceneCamera == true;

			if (HSettings.VoxelizationSettings.AttachTo == null && attachToSceneCamera == false)
			{
				Debug.Log("AttachedCameraTranslate = "+ attachToSceneCamera);
				GroundLevelTranslate();
				_camera.transform.position = _camera.transform.position.OptimizeForVoxelization(HSettings.VoxelizationSettings.ExactData.VoxelSize);
				return;
			}

            Transform attachToTransform = HSettings.VoxelizationSettings.AttachTo;
#if UNITY_EDITOR
			if (attachToSceneCamera)
			{
				attachToTransform = SceneView.lastActiveSceneView.camera.transform;
			}
#endif
			_camera.transform.parent      =  attachToTransform;
			_camera.transform.rotation    =  Quaternion.identity;
			_camera.transform.eulerAngles += new Vector3(-90f, 0, 180f);


            _camera.transform.localPosition = Vector3.zero;

            CenterShiftTranslate(attachToTransform);
            GroundLevelTranslate();
            if (isClipmap)
            {
                _camera.transform.position = _camera.transform.position.OptimizeForVoxelization(HSettings.VoxelizationSettings.ExactData.VoxelSize * 2.0f);
            }
            else
            {
                _camera.transform.position = _camera.transform.position.OptimizeForVoxelization(HSettings.VoxelizationSettings.ExactData.VoxelSize);
            }

        }

		private OffsetAxisIndex CalculateOffsetPositionAndTargetAxis()
		{
			Vector3 offsetWorldPosition = _rememberPos;
			_rememberPos = _camera.transform.position;

			offsetWorldPosition = _rememberPos - offsetWorldPosition;

			VoxelizationRuntimeData.OffsetWorldPosition = new OffsetWorldPosition(
				VoxelizationRuntimeData.OffsetAxisIndex == OffsetAxisIndex.AxisXPos ? 0.0f : VoxelizationRuntimeData.OffsetWorldPosition.AxisXPos,
				VoxelizationRuntimeData.OffsetAxisIndex == OffsetAxisIndex.AxisYPos ? 0.0f : VoxelizationRuntimeData.OffsetWorldPosition.AxisYPos,
				VoxelizationRuntimeData.OffsetAxisIndex == OffsetAxisIndex.AxisZPos ? 0.0f : VoxelizationRuntimeData.OffsetWorldPosition.AxisZPos,
				VoxelizationRuntimeData.OffsetAxisIndex == OffsetAxisIndex.AxisXNeg ? 0.0f : VoxelizationRuntimeData.OffsetWorldPosition.AxisXNeg,
				VoxelizationRuntimeData.OffsetAxisIndex == OffsetAxisIndex.AxisYNeg ? 0.0f : VoxelizationRuntimeData.OffsetWorldPosition.AxisYNeg,
				VoxelizationRuntimeData.OffsetAxisIndex == OffsetAxisIndex.AxisZNeg ? 0.0f : VoxelizationRuntimeData.OffsetWorldPosition.AxisZNeg
			);

			VoxelizationRuntimeData.OffsetWorldPosition += new OffsetWorldPosition(
				offsetWorldPosition.x > Mathf.Epsilon ? offsetWorldPosition.x : 0f,
				offsetWorldPosition.y > Mathf.Epsilon ? offsetWorldPosition.y : 0f,
				offsetWorldPosition.z > Mathf.Epsilon ? offsetWorldPosition.z : 0f,
				offsetWorldPosition.x < Mathf.Epsilon ? -offsetWorldPosition.x : 0f,
				offsetWorldPosition.y < Mathf.Epsilon ? -offsetWorldPosition.y : 0f,
				offsetWorldPosition.z < Mathf.Epsilon ? -offsetWorldPosition.z : 0f
			);

			return VoxelizationRuntimeData.OffsetWorldPosition.MaxAxisOffset();
		}

		private void CenterShiftTranslate(Transform attachToTransform)
		{
			if (attachToTransform.GetComponent<Camera>() && Mathf.Abs(HSettings.VoxelizationSettings.CenterShift) > 0.01f)
			{
				var forward = attachToTransform.forward;
				_camera.transform.position += new Vector3(forward.x, 0f, forward.z) * HSettings.VoxelizationSettings.CenterShift;
			}
		}

		private void GroundLevelTranslate()
		{
			float height = HSettings.VoxelizationSettings.ExactData.Bounds.z;
			if (HSettings.VoxelizationSettings.GroundLevelEnable == true && (_camera.transform.position.y - height / 2) < HSettings.VoxelizationSettings.GroundLevel)
			{
				_camera.transform.position = new Vector3(_camera.transform.position.x, HSettings.VoxelizationSettings.GroundLevel + height / 2,
					_camera.transform.position.z);
			}
		}

		private void UpdateCameraFromUI()
		{
			_dirtyBounds = true;
		}

		private void CreateCamera()
		{
			if (_camera == null)
			{
				_camera              = gameObject.AddComponent<Camera>();
				_camera.aspect       = 1f;
				_camera.orthographic = true;
				_camera.enabled      = false;
				_camera.clearFlags   = CameraClearFlags.Nothing;
				_camera.allowMSAA    = false;
				_camera.hideFlags    = HSettings.DebugSettings.ShowBowels ? HideFlags.None : HideFlags.HideInHierarchy;
			}
		}

		private void Update()
		{	
			if (_voxelsService == null || _voxelsService.PingVoxelsHandler(this))
			{
				DestroyImmediate(this.gameObject);
			}
		}

		private void OnDestroy()
		{
			VoxelizationRuntimeData.OnReallocTextures -= UpdateCameraFromUI;
		}
	}
}
