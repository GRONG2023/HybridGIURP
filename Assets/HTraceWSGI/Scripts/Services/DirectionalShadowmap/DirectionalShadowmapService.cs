using HTraceWSGI.Scripts.Data.Private;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HTraceWSGI.Scripts.Services.DirectionalShadowmap
{
	[ExecuteAlways]
	internal class DirectionalShadowmapService : IService
	{
		public HTraceDirectionalCamera DirectionalCamera;

		private static DirectionalShadowmapService _instance;
		
		public static DirectionalShadowmapService Instance
		{
			get
			{
				if (_instance == null)
					_instance = new DirectionalShadowmapService();
				return _instance;
			}
		}
		
		public void Initialize(int layer)
		{
			CreateHTraceCameraDirectional(layer);
		}
		
		private void CreateHTraceCameraDirectional(int layer)
		{
			if (DirectionalCamera != null)
			{
				DirectionalCamera.Initialize(this);
				return;
			}

			GameObject cameraFromDirLightGo = new GameObject("HTraceDirectionalCameraHandler");
			cameraFromDirLightGo.layer     = layer;
			cameraFromDirLightGo.hideFlags = HSettings.DebugSettings.ShowBowels ? HideFlags.None : HideFlags.HideAndDontSave;
			DirectionalCamera  = cameraFromDirLightGo.AddComponent<HTraceDirectionalCamera>();

			DirectionalCamera.Initialize(this);
		}
		
		

		public bool PingDirLight(HTraceDirectionalCamera camera)
		{
			return DirectionalCamera != camera;
		}

		public void Update()
		{
			
		}

		public void Cleanup()
		{
			if (DirectionalCamera != null)
				Object.DestroyImmediate(DirectionalCamera.gameObject);
		}
	}
}
