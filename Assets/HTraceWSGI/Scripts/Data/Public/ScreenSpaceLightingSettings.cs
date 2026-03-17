using System;
using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using UnityEngine;

namespace HTraceWSGI.Scripts.Data.Public
{
	[Serializable]
	public class ScreenSpaceLightingSettings
	{
		/// <summary>
		/// Allows to evaluate lighting at the hit points of screen-space rays instead of relying solely on the previous Color Buffer.
		/// </summary>
		/// <Docs><see href="https://ipgames.gitbook.io/htrace-wsgi/settings-and-properties">More information</see></Docs>
		public bool EvaluateHitLighting  = false;

		/// <summary>
		/// Enable directional screen space occlusion.
		/// </summary>
		/// <Docs><see href="https://ipgames.gitbook.io/htrace-wsgi/settings-and-properties">More information</see></Docs>
		public bool DirectionalOcclusion = true;
		
		[SerializeField]
		private float _occlusionIntensity = 0.25f;

		/// <summary>
		/// Occlusion Intensity
		/// </summary>
		/// <value>[0.0;1.0]</value>
		/// <Docs><see href="https://ipgames.gitbook.io/htrace-wsgi/settings-and-properties">More information</see></Docs>
		[HExtensions.HRange(0.0f,1.0f)]
		public float OcclusionIntensity
		{
			get => _occlusionIntensity;    
			set
			{
				if (Mathf.Abs(value - _occlusionIntensity) < Mathf.Epsilon)
					return;

				_occlusionIntensity = HExtensions.Clamp(value, typeof(ScreenSpaceLightingSettings), nameof(ScreenSpaceLightingSettings.OcclusionIntensity));
			}
		}

	}
}
