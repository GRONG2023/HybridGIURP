using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HTraceWSGI.Scripts.Services.LightsCluster
{
	[ExecuteAlways]
	[RequireComponent(typeof(Light))]
	[DisallowMultipleComponent]
	[DefaultExecutionOrder(101)] //after update services
	public class HPunctualLight : MonoBehaviour
	{
		public bool DisableShadowCulling = false;

		private Light _light;

		private bool _initialized;
		private bool _isRegistered;
		private bool _lastEnabled;
		private LightType _lastType;

		private Vector4 _boundingSphere = new(0, 0, 0, 100);

		public Light Light => _light;

		//private HDAdditionalLightData _hdAdditionalLightData; // optional in HDRP

		private void OnEnable()
		{
			Initialize();
			_lastEnabled = _light.enabled;
			_lastType    = _light.type;

			if (IsSupportedType(_light.type) && _lastEnabled)
				Register();

			UpdateShadowSettingsAndBounds();
		}

		private void Awake()
		{
			Initialize();
		}

		private void Initialize()
		{
			if (_initialized) return;

			_light = GetComponent<Light>();
			//_hdAdditionalLightData = GetComponent<HDAdditionalLightData>();
			_initialized = true;
		}

		private void LateUpdate()
		{
			if (_light == null)
				return;

			if (_light.enabled != _lastEnabled || _light.type != _lastType)
			{
				_lastEnabled = _light.enabled;
				_lastType    = _light.type;

				if (IsSupportedType(_light.type) && _light.enabled)
					Register();
				else
					Unregister();
			}

			UpdateShadowSettingsAndBounds();
		}

		private void Register()
		{
			if (_isRegistered) return;
			if (!IsSupportedType(_light.type) || !_light.enabled) return;

			if (LightsService.Instance != null)
			{
				LightsService.Instance.AddLight(_light);
				_isRegistered = true;
			}
		}

		private void Unregister()
		{
			if (!_isRegistered) return;

			if (LightsService.Instance != null && _light != null)
			{
				LightsService.Instance.RemoveLight(_light);
			}

			_isRegistered = false;
		}

		private void UpdateShadowSettingsAndBounds()
		{
			if (_light == null)
				return;

			_light.useViewFrustumForShadowCasterCull = !DisableShadowCulling;

			// Only Point/Spot are supported and use the bounding sphere override
			if (!IsSupportedType(_light.type))
			{
				_light.useBoundingSphereOverride = false;
				return;
			}

			// Vector3 pos    = _light.transform.position;
			// float   radius = Mathf.Max(1f, _light.range);
			// _light.boundingSphereOverride    = new Vector4(pos.x, pos.y, pos.z, radius);

			_light.boundingSphereOverride    = _boundingSphere;
			_light.useBoundingSphereOverride = true;
		}

		private void OnDisable()
		{
			Unregister();
		}

		private void OnDestroy()
		{
			Unregister();
		}

		private static bool IsSupportedType(LightType type)
		{
			bool isSupportedType = type == LightType.Point || type == LightType.Spot;
			if (isSupportedType == false)
			{
				Debug.LogWarning($"H Punctual Light script is assigned to an unsupported light type!");
			}
			return isSupportedType;
		}
	}
}
