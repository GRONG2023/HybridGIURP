using HTraceWSGI.Scripts.Data.Private;
using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using UnityEngine;

namespace HTraceWSGI.Scripts.Services.VoxelCameras
{
	[ExecuteAlways]
	public class VoxelsService : IService
	{
		private static VoxelsService _instance;
		
		public static VoxelsService Instance
		{
			get
			{
				if (_instance == null)
					_instance = new VoxelsService();
				return _instance;
			}
		}
		
		private bool  _initialized;
		
		//Debug fields
		internal Bounds BoundsGizmo;

		private Transform              _prevAttachTo;
		private int _prevLodMax;

		internal bool NeedToReallocForUI
		{
			get { return _needToReallocForUI; }
		}

		//UI fields for Apply Params button
		private bool  _needToReallocForUI = false;
		private float _prevDensityUI;
		private int   _prevVoxelBoundsUI;
		private int   _prevOverrideBoundsHeightUI;

		internal void Initialize(int layer)
		{
			CreateVoxelCamera(layer);

			VoxelizationRuntimeData.FullVoxelization  =  false;

			if (HSettings.VoxelizationSettings.AttachTo == null && Camera.main != null)
				HSettings.VoxelizationSettings.AttachTo = Camera.main.transform;
			_prevAttachTo = HSettings.VoxelizationSettings.AttachTo;

			_initialized = true;
		}

		public void Update()
		{
			if (!_initialized || HSettings.VoxelizationSettings == null)
				return;

			CheckBounds();
			HSettings.VoxelizationSettings.UpdateData();
			CheckPrevValues();
		}

		public void Cleanup()
		{
			if (VoxelizationRuntimeData.VoxelCamera != null)
				Object.DestroyImmediate(VoxelizationRuntimeData.VoxelCamera.gameObject);
			
			_initialized = false;
		}

		public Bounds GetVoxelCameraBounds()
		{
			Vector3 boundCenter = VoxelizationRuntimeData.VoxelCamera.transform.position;

			float height = HSettings.VoxelizationSettings.OverrideBoundsHeightEnable == false ? HSettings.VoxelizationSettings.VoxelBounds : HSettings.VoxelizationSettings.OverrideBoundsHeight;
			if (HSettings.VoxelizationSettings.GroundLevelEnable == true && (VoxelizationRuntimeData.VoxelCamera.transform.position.y - height / 2) < HSettings.VoxelizationSettings.GroundLevel)
			{
				boundCenter = new Vector3(VoxelizationRuntimeData.VoxelCamera.transform.position.x, HSettings.VoxelizationSettings.GroundLevel + height / 2, VoxelizationRuntimeData.VoxelCamera.transform.position.z);
			}

			BoundsGizmo.center = boundCenter;

			BoundsGizmo.size = new Vector3(
				HSettings.VoxelizationSettings.ExactData.Bounds.x,
				HSettings.VoxelizationSettings.ExactData.Bounds.z,
				HSettings.VoxelizationSettings.ExactData.Bounds.y);

			return BoundsGizmo;
		}

		private void CheckBounds()
		{
			if (VoxelizationRuntimeData.CheckPrevParams(HSettings.VoxelizationSettings.VoxelDensity, HSettings.VoxelizationSettings.VoxelBounds, HSettings.VoxelizationSettings.OverrideBoundsHeight))
			{
				_needToReallocForUI = Time.frameCount > 3; // hack for enter and exit in Play mode
				VoxelizationRuntimeData.SetParamsForApplyButton(HSettings.VoxelizationSettings.VoxelDensity, HSettings.VoxelizationSettings.VoxelBounds, HSettings.VoxelizationSettings.OverrideBoundsHeight);
			}
			else
			{
				_needToReallocForUI = false;
			}
		}

		private void CheckPrevValues()
		{
			if (HSettings.VoxelizationSettings.AttachTo != _prevAttachTo)
			{
				_prevAttachTo = HSettings.VoxelizationSettings.AttachTo;
				//VoxelizationRuntimeData.OnReallocTextures?.Invoke(); // why is it here? enought:
				VoxelizationRuntimeData.FullVoxelization = true;
			}
			if (HSettings.VoxelizationSettings.LODMax != _prevLodMax)
			{
				_prevLodMax = HSettings.VoxelizationSettings.LODMax;
				VoxelizationRuntimeData.FullVoxelization = true;
			}
		}

		private void CreateVoxelCamera(int layer)
		{
			if (VoxelizationRuntimeData.VoxelCamera != null)
			{
				VoxelizationRuntimeData.VoxelCamera.Initialize(this);
				return;
			}

			GameObject cameraGO = new GameObject(HNames.HTRACE_VOXEL_CAMERA_NAME);
			cameraGO.layer     = layer;
			cameraGO.hideFlags = HSettings.DebugSettings.ShowBowels ? HideFlags.None : HideFlags.HideAndDontSave;
			// cameraGO.transform.parent = Camera.main.transform;
			// cameraGO.transform.localPosition = Vector3.zero;
			VoxelizationRuntimeData.VoxelCamera = cameraGO.AddComponent<VoxelCamera>();
			VoxelizationRuntimeData.VoxelCamera.Initialize(this);
		}

		internal bool PingVoxelsHandler(VoxelCamera voxelCamera)
		{
			return VoxelizationRuntimeData.VoxelCamera != voxelCamera;
		}
	}
}
