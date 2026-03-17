using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HTraceWSGI.Scripts.Services.LightsCluster
{
	[ExecuteAlways]
	internal sealed class LightsService : IService
	{
		private struct LightInfo
		{
			public Vector4 Position;
			//public HDAdditionalLightData HDData;
		}

		private static LightsService _instance;

		public static LightsService Instance
		{
			get
			{
				if (_instance == null)
					_instance = new LightsService();
				return _instance;
			}
		}
		//public int                         LightsCount => _lights.Count;

		private readonly Dictionary<Light, LightInfo> _lights = new Dictionary<Light, LightInfo>(10);

		public void Initialize()
		{
			HPunctualLight[] findObjectsByType = Object.FindObjectsByType<HPunctualLight>(FindObjectsSortMode.None);
			foreach (var light in findObjectsByType)
			{
				if (light.enabled == true)
					AddLight(light.Light);
			}
		}

		public void AddLight(Light light)
		{
			if (light == null || !light.enabled)
				return;

			if (light.type != LightType.Point && light.type != LightType.Spot)
				return;

			if (_lights.ContainsKey(light))
				return;

			//var hdData = light.GetComponent<HDAdditionalLightData>();
			//if (hdData == null)
			//	return;

			//var lightInfo = new LightInfo
			//{
			//	Position = Vector4.zero,
			//	HDData    = hdData
			//};

			//_lights[light] = lightInfo;
		}

		public void RemoveLight(Light light)
		{
			if (light == null)
				return;

			_lights.Remove(light);
		}

		public void Update()
		{
			var keys = _lights.Keys.ToArray();

			for (int i = 0; i < keys.Length; i++)
			{
				var key = keys[i];

				if (!key || !key.enabled || (key.type != LightType.Point && key.type != LightType.Spot))
				{
					_lights.Remove(key);
					continue;
				}

				var lightInfo = _lights[key];
				lightInfo.Position = key.transform.position;
				_lights[key]       = lightInfo;
			}
		}

		public IEnumerable<Vector4> GetFilteredLights(Camera camera)
		{
			if (camera == null)
				yield break;

			var cameraPosition = camera.transform.position;

			foreach (var kvp in _lights)
			{
				var light = kvp.Key;
				var lightInfo = kvp.Value;

				if (!light || !light.enabled)
					continue;

				var distance = Vector3.Distance(cameraPosition, light.transform.position);

				//if (distance <= lightInfo.HDData.fadeDistance)
				//{
				//	yield return lightInfo.Position;
				//}
			}
		}

		public void Cleanup()
		{
			_lights.Clear();
		}
	}
}
