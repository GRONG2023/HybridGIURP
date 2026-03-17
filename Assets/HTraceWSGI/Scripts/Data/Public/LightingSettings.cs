using System;
using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using UnityEngine;

namespace HTraceWSGI.Scripts.Data.Public
{
	[Serializable]
	public class LightingSettings
	{
		/// <summary>
		/// Main shadow-casting Directional light. It is usually set up automatically.
		/// </summary>
		/// <Docs><see href="https://ipgames.gitbook.io/htrace-wsgi/settings-and-properties">More information</see></Docs>
		public Light DirectionalLight;

		[SerializeField]
		private float _expandShadowmap = 1f;

		/// <summary>
		/// Controls the area covered by the custom directional shadowmap. The shadowmap is used to evaluate direct lighting and shadowing at hit points of world-space rays.
		/// </summary>
		/// <value>[0.0;3.0]</value>
		/// <Docs><see href="https://ipgames.gitbook.io/htrace-wsgi/settings-and-properties">More information</see></Docs>
		[HExtensions.HRangeAttribute(1.0f, 3.0f)]
		public float ExpandShadowmap
		{
			get { return _expandShadowmap; }
			set
			{
				if (Mathf.Abs(value - _expandShadowmap) < Mathf.Epsilon)
					return;

				_expandShadowmap = HExtensions.Clamp(value, typeof(LightingSettings), nameof(LightingSettings.ExpandShadowmap));
			}
		}
		
		[SerializeField]
		private float _shadowmapRange = 100f;

		[HExtensions.HRangeAttribute(10f, 500f)]
		public float ShadowmapRange
		{
			get { return _shadowmapRange; }
			set
			{
				if (Mathf.Abs(value - _shadowmapRange) < Mathf.Epsilon)
					return;

				_shadowmapRange = HExtensions.Clamp(value, typeof(LightingSettings), nameof(LightingSettings.ShadowmapRange));
			}
		}

		public ShadowmapUpdateMode ShadowmapUpdateMode = ShadowmapUpdateMode.Default;

		// ------------------------------ Light Cluster ------------------------------

		[SerializeField]
		public bool EvaluatePunctualLights = true;

		[SerializeField]
		private int _lightClusterCellLightCount = 8;

		[HExtensions.HRangeAttribute(2, 16)]
		public int LightClusterCellLightCount
		{
			get { return _lightClusterCellLightCount; }
			set
			{
				int roundedValue = Mathf.RoundToInt((float)value / 2) * 2;

				roundedValue = HExtensions.Clamp(roundedValue, typeof(LightingSettings), nameof(LightingSettings.LightClusterCellLightCount));

				if (roundedValue == _lightClusterCellLightCount)
					return;

				_lightClusterCellLightCount = roundedValue;
			}
		}

		[SerializeField]
		private int _lightClusterCellDensity = 16;

		[HExtensions.HRangeAttribute(16, 64)]
		public int LightClusterCellDensity
		{
			get { return _lightClusterCellDensity; }
			set
			{
				int roundedValue = Mathf.RoundToInt((float)value / 4) * 4;

				roundedValue = HExtensions.Clamp(roundedValue, typeof(LightingSettings), nameof(LightingSettings.LightClusterCellDensity));

				if (roundedValue == _lightClusterCellDensity)
					return;

				_lightClusterCellDensity = roundedValue;
			}
		}

		[SerializeField]
		private int _lightClusterRange = 100;

		[HExtensions.HRangeAttribute(10, 500)]
		public int LightClusterRange
		{
			get { return _lightClusterRange; }
			set
			{
				if (value == _lightClusterRange)
					return;

				_lightClusterRange = HExtensions.Clamp(value, typeof(LightingSettings), nameof(LightingSettings.LightClusterRange));
			}
		}
	}
}
