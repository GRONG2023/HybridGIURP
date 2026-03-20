using HTraceWSGI.Scripts.Passes.HDRP;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HybridGIFeature : ScriptableRendererFeature
{

    private PrePassHDRP _prePass;
    private GBufferPassHDRP _gBufferPass;
    private MotionVectorsPassHDRP _motionVectorsPass;
    private DirectionalShadowmapPassHDRP _directionalShadowmapPass;
    private VoxelizationPassHDRP _voxelizationPass;
    private SoftwareTracingPassHDRP _softwareTracingPass;
    private ColorHistoryPass _colorHistoryPass;
    private FinalPassHDRP _finalDebugPass;

    public bool  useColorPreviousFrame = false;

    /// <inheritdoc/>
    public override void Create()
    {
        _prePass = new PrePassHDRP();
        _gBufferPass = new GBufferPassHDRP();
        _motionVectorsPass = new MotionVectorsPassHDRP();
        _directionalShadowmapPass = new DirectionalShadowmapPassHDRP();
        _voxelizationPass = new VoxelizationPassHDRP();
        _softwareTracingPass = new SoftwareTracingPassHDRP();
        _colorHistoryPass = new ColorHistoryPass();
        _finalDebugPass = new FinalPassHDRP();

        // Configures where the render pass should be injected.
        _prePass.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer;
        _gBufferPass.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer + 1;
        _motionVectorsPass.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer + 2;
        _directionalShadowmapPass.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer + 3;
        _voxelizationPass.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer + 4;
        _softwareTracingPass.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer + 5;
        _colorHistoryPass.renderPassEvent = RenderPassEvent.AfterRenderingDeferredLights;
        _finalDebugPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        _softwareTracingPass.useColorPreviousFrame = useColorPreviousFrame;

        renderer.EnqueuePass(_prePass);
        renderer.EnqueuePass(_gBufferPass);
        renderer.EnqueuePass(_motionVectorsPass);
        renderer.EnqueuePass(_directionalShadowmapPass);
        renderer.EnqueuePass(_voxelizationPass);
        renderer.EnqueuePass(_softwareTracingPass);
        renderer.EnqueuePass(_colorHistoryPass);
        renderer.EnqueuePass(_finalDebugPass);
    }

    protected override void Dispose(bool disposing)
    {
        if (_prePass != null)
        {
            _prePass.Cleanup();
        }

        if (_gBufferPass != null)
        {
            _gBufferPass.Cleanup();
        }
        if (_motionVectorsPass != null)
        {
            _motionVectorsPass.Cleanup();
        }
        if (_directionalShadowmapPass != null)
        {
            _directionalShadowmapPass.Cleanup();
        }
        if (_voxelizationPass != null)
        {
            _voxelizationPass.Cleanup();
        }
        if (_softwareTracingPass != null)
        {
            _softwareTracingPass.Cleanup();
        }
        if (_colorHistoryPass != null)
        {
            _colorHistoryPass.Cleanup();
        }
        if (_finalDebugPass != null)
        {
            _finalDebugPass.Cleanup();
        }
    }
}


