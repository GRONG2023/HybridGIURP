//pipelinedefine
#define H_HDRP

using System;
using HTraceWSGI.Scripts.Globals;
using UnityEngine;

namespace HTraceWSGI.Scripts.Data.Public
{
	[Serializable]
	internal class DebugSettings
	{
		[SerializeField]
		private bool _enableDebug = true;

		public bool EnableDebug
		{
			get { return _enableDebug; }
			set { _enableDebug = value; }
		}
		public bool AttachToSceneCamera  = true;
		
		//Devs:
		public Camera CameraForTests;
		public bool EnableCamerasVisualization = false;
		public bool ShowBowels  = false;
		public bool ShowFullDebugLog = false;

		public bool TestCheckbox = false;

		public RenderTexture   RenderTexture;
		public LayerMask       HTraceLayer     = ~0;
		public HInjectionPoint HInjectionPoint = HInjectionPoint.AfterOpaqueDepthAndNormal;
	}
}
