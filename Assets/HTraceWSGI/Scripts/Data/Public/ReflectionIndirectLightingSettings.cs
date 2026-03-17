using System;
using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using UnityEngine;

namespace HTraceWSGI.Scripts.Data.Public
{
	[Serializable]
	public class ReflectionIndirectLightingSettings
	{
		[SerializeField]
		public IndirectEvaluationMethod IndirectEvaluationMethod = IndirectEvaluationMethod.None;
		
		[SerializeField]
		private float _rayBias = 0.5f;
		
		[HExtensions.HRange(0.0f,1.0f)]
		public float RayBias
		{
			get => _rayBias;
			set
			{
				if (Mathf.Abs(value - _rayBias) < Mathf.Epsilon)
					return;

				_rayBias = HExtensions.Clamp(value, typeof(ReflectionIndirectLightingSettings), nameof(ReflectionIndirectLightingSettings.RayBias));
			}
		}
		
		[SerializeField]
		private float _maxRayLength = 100f;
		
		[HExtensions.HRange(0.0f,100.0f)]
		public float MaxRayLength
		{
			get => _maxRayLength;
			set
			{
				if (Mathf.Abs(value - _maxRayLength) < Mathf.Epsilon)
					return;

				_maxRayLength = HExtensions.Clamp(value, typeof(ReflectionIndirectLightingSettings), nameof(ReflectionIndirectLightingSettings.MaxRayLength));
			}
		}
		
		[SerializeField]
		public SpatialRadius SpatialRadius = SpatialRadius.Medium;
		
		[SerializeField]
		private float _jitterRadius = 0.5f;
		
		[HExtensions.HRange(0.0f,1.0f)]
		public float JitterRadius
		{
			get => _jitterRadius;
			set
			{
				if (Mathf.Abs(value - _jitterRadius) < Mathf.Epsilon)
					return;

				_jitterRadius = HExtensions.Clamp(value, typeof(ReflectionIndirectLightingSettings), nameof(ReflectionIndirectLightingSettings.JitterRadius));
			}
		}

		[SerializeField]
		public bool TemporalJitter = true;

		[SerializeField]
		public bool OcclusionCheck = false;
	}
}
