#if UNITY_EDITOR
using UnityEditor;

namespace HTraceWSGI.Scripts.Editor
{
	[InitializeOnLoad]
	public static class SceneViewDrawModeTracker
	{
		private const string Shaded = "Shaded";
		private const string Wireframe = "Wireframe";
		private const string ShadedWireframe = "Shaded Wireframe";
		private const string Unlit = "Unlit";

		private static bool s_isShaded = true;
		private static bool s_isWireframe = true;
		private static bool s_isShadedWireframe = true;
		private static bool s_isUnlit = true;

		public static bool IsShaded          => s_isShaded;
		public static bool IsWireframe       => s_isWireframe;
		public static bool IsShadedWireframe => s_isShadedWireframe;
		public static bool IsUnlit           => s_isUnlit;

		static SceneViewDrawModeTracker()
		{
			SceneView.duringSceneGui += OnSceneGUI;

		}

		static void OnSceneGUI(SceneView view)
		{
			s_isShaded          = (view.cameraMode.name == Shaded);
			s_isWireframe       = (view.cameraMode.name == Wireframe);
			s_isShadedWireframe = (view.cameraMode.name == ShadedWireframe);
			s_isUnlit           = (view.cameraMode.name == Unlit);
		}
	}
}
#endif
