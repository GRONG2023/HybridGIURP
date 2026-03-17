//pipelinedefine
#define H_HDRP

using System.Collections.Generic;
using System.Linq;
using HTraceWSGI.Scripts.Data.Private;
using HTraceWSGI.Scripts.Globals;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HTraceWSGI.Scripts.Services.DirectionalShadowmap
{
	[ExecuteInEditMode]
	internal class HTraceDirectionalCamera : MonoBehaviour
	{
		private const float SQRT_OF_3 = 1.732f;

		public int ShadowResolution = 2048;

		private Camera                      _directionalCamera;
		private DirectionalShadowmapService _directionalShadowmapService;

		private Vector3    _rememberPos;
		private Quaternion _rememberRot;
		private Light      _directionalLight;
		private bool       _needToRenderVoxels;

		public Camera GetDirectionalCamera
		{
			get { return _directionalCamera; }
		}

		public Camera Initialize(DirectionalShadowmapService directionalShadowmapService)
		{
			_directionalShadowmapService   = directionalShadowmapService;

			IEnumerable<Light> lights = Object.FindObjectsOfType<Light>()
				.Where(lightComp => lightComp.type == LightType.Directional)
				.ToList();

			if (HSettings.LightingSettings.DirectionalLight != null)
				_directionalLight = HSettings.LightingSettings.DirectionalLight;

			if (_directionalLight == null && lights.Any())
			{
				_directionalLight = lights.FirstOrDefault(lightComp => lightComp.gameObject.activeSelf == true);
				if (_directionalLight == null)
					_directionalLight = lights.First();

				HSettings.LightingSettings.DirectionalLight = _directionalLight;
			}

			gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			CreateDirectionalCamera();

			return _directionalCamera;
		}

		private void CreateDirectionalCamera()
		{
			_directionalCamera = gameObject.GetComponentInChildren<Camera>();
			if (_directionalCamera == null)
			{
				GameObject cameraGo = new GameObject("Directional Camera");
				cameraGo.transform.parent = this.gameObject.transform;
				cameraGo.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
				_directionalCamera           = cameraGo.AddComponent<Camera>();
				_directionalCamera.hideFlags = HSettings.DebugSettings.ShowBowels ? HideFlags.None : HideFlags.HideInHierarchy;
				
			}
			
			_directionalCamera.enabled = false;
			_directionalCamera.orthographic = true;
			_directionalCamera.allowMSAA = false;
			_directionalCamera.cullingMask = ~0;
		}

		public void ExecuteUpdate()
		{
			UpdateCamera();
			SetParams();
		}

		private void UpdateCamera()
		{
			if (HSettings.LightingSettings.DirectionalLight == null)
				return;
            //transform.position = _voxelCamera.transform.position - HResources.VoxelizationData.DirectionalLight.transform.forward * _voxelCamera.orthographicSize * SQRT_OF_3;
            //transform.rotation = HResources.VoxelizationData.DirectionalLight.transform.rotation;

            transform.parent = VoxelizationRuntimeData.VoxelCamera.Camera.transform;

            bool isTranslateNeeded = true;

			if (isTranslateNeeded)
			{
                var voxelCamera = VoxelizationRuntimeData.VoxelCamera.Camera;
                transform.position = voxelCamera.transform.position - HSettings.LightingSettings.DirectionalLight.transform.forward * voxelCamera.orthographicSize * SQRT_OF_3;

                transform.rotation = HSettings.LightingSettings.DirectionalLight.transform.rotation;
				_rememberPos       = transform.position;
				_rememberRot       = transform.rotation;
			}
			else
			{
				transform.position = _rememberPos;
				transform.rotation = _rememberRot;
			}
			
			_directionalCamera.transform.localPosition = Vector3.zero;

			//_fakeDirectionalLight.SetShadowResolution(ShadowResolution);

			//_cameraFakeDirLight.transform.localPosition = Vector3.zero;
			//_cameraFakeDirLight.transform.localPosition -= Vector3.forward * _voxelCamera.orthographicSize * SQRT_OF_3;
		}

		private void SetParams()
		{
			float scale = 1f;
			
			float value = VoxelizationRuntimeData.VoxelCamera.Camera.orthographicSize * SQRT_OF_3 * HSettings.LightingSettings.ExpandShadowmap;

			_directionalCamera.farClipPlane     = 1f * 2 * value;
			_directionalCamera.nearClipPlane    = 0f;
			_directionalCamera.orthographicSize = value / scale;
			_directionalCamera.aspect           = 1;

			//Debug.Log("SetParams£º"+ value+ " ExpandShadowmap = " + HSettings.LightingSettings.ExpandShadowmap+ "  orthographicSize = " + VoxelizationRuntimeData.VoxelCamera.Camera.orthographicSize);
		}

		private void OctantTransformCamera()
		{
			_directionalCamera.transform.SetPositionAndRotation(_rememberPos, _rememberRot);
			
			Vector3 finalLocalPos = Vector3.zero;
			float   sizeOrtho     = _directionalCamera.orthographicSize;
			switch (VoxelizationRuntimeData.OctantIndex)
			{
				case OctantIndex.OctantA:
					finalLocalPos += sizeOrtho * -_directionalCamera.transform.right;
					finalLocalPos += sizeOrtho * _directionalCamera.transform.up;
					break;
				case OctantIndex.OctantB:
					finalLocalPos += sizeOrtho * _directionalCamera.transform.right;
					finalLocalPos += sizeOrtho * _directionalCamera.transform.up;
					break;
				case OctantIndex.OctantC:
					finalLocalPos += sizeOrtho * -_directionalCamera.transform.right;
					finalLocalPos += sizeOrtho * -_directionalCamera.transform.up;
					break;
				case OctantIndex.OctantD:
					finalLocalPos += sizeOrtho * _directionalCamera.transform.right;
					finalLocalPos += sizeOrtho * -_directionalCamera.transform.up;
					break;
				case OctantIndex.DynamicObjects:
					break;
			}

			_directionalCamera.transform.position += finalLocalPos;
		}

		private void Update()
		{
			if (_directionalShadowmapService == null || _directionalShadowmapService.PingDirLight(this))
			{
				DestroyImmediate(this.gameObject);
			}
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if (HSettings.DebugSettings.EnableCamerasVisualization == false)
				return;

			var color = Gizmos.color;
			Gizmos.color = new Color(1, 0.92f, 0.016f, 0.2f);

			Vector3 posOffset = _directionalCamera.transform.forward * _directionalCamera.farClipPlane / 2;
			Vector3 position  = _directionalCamera.transform.position + posOffset;

			Matrix4x4 originalMatrix = Gizmos.matrix;
			Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
			rotationMatrix = Matrix4x4.TRS(position, _directionalCamera.transform.rotation, _directionalCamera.transform.lossyScale);
			Gizmos.matrix  = rotationMatrix;

			// Size = height / 2
			// Aspect = width / height
			//
			// height = 2f * size;
			// width = height * aspect;
			Vector3 size = new Vector3(2f * _directionalCamera.orthographicSize, 2f * _directionalCamera.orthographicSize * _directionalCamera.aspect, _directionalCamera.farClipPlane);

			Gizmos.DrawCube(Vector3.zero, size);

			Gizmos.matrix = originalMatrix;
			Gizmos.color  = color;
		}
#endif
	}
}
