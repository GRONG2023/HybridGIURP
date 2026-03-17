using HTraceWSGI.Scripts.Data.Private;
using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Wrappers;
using UnityEngine;

namespace HTraceWSGI.Scripts.Passes.Shared
{
	internal static class VoxelizationShared
	{
		internal enum HVoxelizationKernel
		{
			GeneratePositionPyramid1 = 0,
			GeneratePositionPyramid2 = 1,
			ClearPositionPyramid = 2,
			CopyData = 3,
		}

		internal enum VoxelVisualizationKernel
		{
			VisualizeVoxels = 0,
		}

		#region Shaders Properties ID

		//globals
		internal static readonly int g_VoxelCameraPos       = Shader.PropertyToID("_VoxelCameraPos");
		internal static readonly int g_VoxelCameraPosActual = Shader.PropertyToID("_VoxelCameraPosActual");
		internal static readonly int g_VoxelResolution      = Shader.PropertyToID("_VoxelResolution");
		internal static readonly int g_VoxelBounds          = Shader.PropertyToID("_VoxelBounds");
		internal static readonly int g_VoxelPerMeter        = Shader.PropertyToID("_VoxelPerMeter");
		internal static readonly int g_VoxelSize            = Shader.PropertyToID("_VoxelSize");
		internal static readonly int g_VoxelizationAABB_Min = Shader.PropertyToID("_VoxelizationAABB_Min");
		internal static readonly int g_VoxelizationAABB_Max = Shader.PropertyToID("_VoxelizationAABB_Max");
		internal static readonly int g_VoxelData            = Shader.PropertyToID("_VoxelData");
		internal static readonly int g_VoxelPositionPyramid = Shader.PropertyToID("_VoxelPositionPyramid");
		
		// Clipmap globals
		internal static readonly int g_EnableClipmaps            = Shader.PropertyToID("_EnableClipmaps");
		internal static readonly int g_VoxelClipmapResolution    = Shader.PropertyToID("_VoxelClipmapResolution");
		internal static readonly int g_VoxelClipmapBounds        = Shader.PropertyToID("_VoxelClipmapBounds");
		internal static readonly int g_VoxelClipmapPerMeter      = Shader.PropertyToID("_VoxelClipmapPerMeter");
		internal static readonly int g_VoxelClipmapSize          = Shader.PropertyToID("_VoxelClipmapSize");
		internal static readonly int g_VoxelClipmapData          = Shader.PropertyToID("_VoxelClipmapData");
		internal static readonly int g_VoxelClipmapPositionPyramid = Shader.PropertyToID("_VoxelClipmapPositionPyramid");
        internal static readonly int g_VoxelizationClipmapAABB_Min = Shader.PropertyToID("_VoxelizationClipmapAABB_Min");
        internal static readonly int g_VoxelizationClipmapAABB_Max = Shader.PropertyToID("_VoxelizationClipmapAABB_Max");

        //locals
        internal static readonly int _VoxelPositionPyramid_Mip0        = Shader.PropertyToID("_VoxelPositionPyramid_MIP0");
		internal static readonly int _VoxelPositionPyramid_Mip1        = Shader.PropertyToID("_VoxelPositionPyramid_MIP1");
		internal static readonly int _VoxelPositionPyramid_Mip2        = Shader.PropertyToID("_VoxelPositionPyramid_MIP2");
		internal static readonly int _VoxelPositionIntermediate_Output = Shader.PropertyToID("_VoxelPositionIntermediate_Output");
		internal static readonly int _VoxelPositionIntermediate        = Shader.PropertyToID("_VoxelPositionIntermediate");
		internal static readonly int _VoxelPositionPyramid_Mip3        = Shader.PropertyToID("_VoxelPositionPyramid_MIP3");
		internal static readonly int _VoxelPositionPyramid_Mip4        = Shader.PropertyToID("_VoxelPositionPyramid_MIP4");
		internal static readonly int _VoxelPositionPyramid_Mip5        = Shader.PropertyToID("_VoxelPositionPyramid_MIP5");

		//spatial specific below


		//locals
		internal static readonly int _OctantCopyOffset                 = Shader.PropertyToID("_OctantCopyOffset");
		internal static readonly int _VoxelOffset                      = Shader.PropertyToID("_VoxelOffset");
		internal static readonly int _VoxelData_A                      = Shader.PropertyToID("_VoxelData_A");
		internal static readonly int _VoxelData_B                      = Shader.PropertyToID("_VoxelData_B");

		#endregion Shaders Properties ID

		internal static ProfilingSamplerHTrace s_VoxelizationConstantProfilingSampler = new ProfilingSamplerHTrace("Voxelization Constant");
		internal static ProfilingSamplerHTrace s_VoxelizationPartialProfilingSampler  = new ProfilingSamplerHTrace("Voxelization Partial");
		internal static ProfilingSamplerHTrace s_RenderVoxelsProfilingSampler         = new ProfilingSamplerHTrace("Render Voxels");

		internal static ProfilingSamplerHTrace s_ClearVoxelTexturesProfilingSampler      = new ProfilingSamplerHTrace("Clear Voxel Textures");
		internal static ProfilingSamplerHTrace s_GeneratePositionPyramidProfilingSampler = new ProfilingSamplerHTrace("Generate Position Pyramid");
		internal static ProfilingSamplerHTrace s_VisualizeVoxelsProfilingSampler         = new ProfilingSamplerHTrace("Visualize Voxels",               parentName: HNames.HTRACE_VOXELIZATION_PASS_NAME, priority: 0);
		//Spatial specific below
		internal static ProfilingSamplerHTrace s_CopyVoxelsProfilingSampler      = new ProfilingSamplerHTrace("Copy Voxels");

		// Shaders & Materials
		internal static Shader        VoxelizationShader;
		internal static ComputeShader HVoxelization;
		internal static ComputeShader VoxelVisualization;
		internal static Material      VoxelVisualizationMaterialHDRP;
		internal static Material      RayDirectionsVisualizationBIRP;

		// Buffers & Textures
		internal static ComputeBuffer DummyVoxelBuffer;

		internal static RTWrapper DummyVoxelizationTarget   = new RTWrapper();
		internal static RTWrapper VoxelPositionPyramid      = new RTWrapper();
		internal static RTWrapper VoxelPositionIntermediate = new RTWrapper();
		internal static RTWrapper VoxelData                 = new RTWrapper();

        // Clipmap textures
        internal static RTWrapper VoxelClipmapPositionPyramid = new RTWrapper();
        internal static RTWrapper VoxelClipmapPositionIntermediate = new RTWrapper();
        internal static RTWrapper VoxelClipmapData = new RTWrapper();

        //Spatial specific below
        internal static RTWrapper DummyVoxelizationStaticTarget  = new RTWrapper();
		internal static RTWrapper DummyVoxelizationDynamicTarget = new RTWrapper();
		internal static RTWrapper VoxelData_A                    = new RTWrapper();
		internal static RTWrapper VoxelData_B                    = new RTWrapper();
		

		// DEBUG RT
		internal static RTWrapper VoxelVisualizationRayDirections = new RTWrapper();
		internal static RTWrapper DebugOutput                     = new RTWrapper();

		internal static string VISUALIZE_OFF = "VISUALIZE_OFF";
		internal static string VISUALIZE_LIGHTING = "VISUALIZE_LIGHTING";
		internal static string VISUALIZE_COLOR = "VISUALIZE_COLOR";
		internal static string EVALUATE_PUNCTUAL_LIGHTS = "EVALUATE_PUNCTUAL_LIGHTS";
		internal static string DYNAMIC_VOXELIZATION = "DYNAMIC_VOXELIZATION";
		internal static string PARTIAL_VOXELIZATION = "PARTIAL_VOXELIZATION";
		internal static string CONSTANT_VOXELIZATION = "CONSTANT_VOXELIZATION";

		internal struct HistoryData : IHistoryData
		{
			public DebugModeWS DebugMode;
			public TracingMode TracingMode;
			public VoxelizationUpdateMode VoxelizationUpdateMode;
			public Vector3 VoxelCameraPosition; //update it directly
			public bool EnableClipmaps;

			public void Update()
			{
				DebugMode              = HSettings.GeneralSettings.DebugModeWS;
				TracingMode            = HSettings.GeneralSettings.TracingMode;
				VoxelizationUpdateMode = HSettings.VoxelizationSettings.VoxelizationUpdateMode;
				EnableClipmaps         = HSettings.VoxelizationSettings.EnableClipmaps;
			}
		}

		internal static HistoryData History = new HistoryData();
	}
}
