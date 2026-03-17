//pipelinedefine
#define H_HDRP


using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HTraceWSGI.Scripts.Globals
{
	public enum DebugModeWS
	{
		None = 0,
		MainBuffers,
		GlobalIllumination,
		GeometryNormals,
		Shadowmap,
		VoxelizedColor,
		VoxelizedLighting,
		LightClusterColor,
		LightClusterHeatmap,
	}
	
	public enum HBuffer
	{
		Multi,
		Depth,
		Diffuse,
		Normal,
		MotionMask,
		MotionVectors,
	}

	public enum VoxelizationUpdateMode
	{
		Constant = 0,
		Partial
	}
	
	public enum ShadowmapUpdateMode
	{
		Default = 0,
		TimeSliced,
	}
	
	public enum IndirectEvaluationMethod
	{
		None = 0,
		Tracing,
		Approximation
	}

	public enum SpatialRadius
	{
		None = 0,
		Medium,
		Wide
	}

	public enum RayCountMode
	{
		Performance = 0,
		Quality,
		Cinematic
	}

	public enum HInjectionPoint
	{
		//AfterOpaqueDepthAndNormal = RenderPassEvent.AfterOpaqueDepthAndNormal,
		AfterOpaqueDepthAndNormal = RenderPassEvent.AfterRenderingPrePasses,
		//BeforeTransparent = RenderPassEvent.BeforeTransparent,
		BeforeTransparent = RenderPassEvent.BeforeRenderingTransparents,
		//BeforePostProcess = RenderPassEvent.BeforePostProcess,
		BeforePostProcess = RenderPassEvent.BeforeRenderingPostProcessing,
	}

	public enum Multibounce
	{
		None = 0,
		IrradianceCache = 1,
		AdaptiveProbeVolumes,
	}
	
	public enum TracingMode
	{
		SoftwareTracing = 0,
		HardwareTracing = 1,
	}

	public enum DebugType
	{
		Log,
		Warning,
		Error,
	}
}
