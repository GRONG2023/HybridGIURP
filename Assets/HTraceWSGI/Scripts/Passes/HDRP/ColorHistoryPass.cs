//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Passes.Shared;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HTraceWSGI.Scripts.Passes.HDRP
{
    /// <summary>
    /// ColorHistoryPass - Manages the color history buffer (previous frame)
    /// This pass copies the current camera color buffer to a history RT for temporal effects
    /// </summary>
    internal class ColorHistoryPass : ScriptableRenderPass
    {
        private bool _initialized = false;
        
        public ColorHistoryPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingDeferredLights;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!_initialized)
            {
                AllocateColorHistory();
                
                
                _initialized = true;
            }
        }

        public override void Execute(ScriptableRenderContext renderContext, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            
            // Skip for preview and reflection cameras
            if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
                return;

            var cmd = CommandBufferPool.Get("ColorHistory Pass");

            try
            {
                // Get current camera color buffer
                var renderer = renderingData.cameraData.renderer as UniversalRenderer;
                if (renderer == null)
                    return;

                RTHandle cameraColorBuffer = renderer.cameraColorTargetHandle;
                
                // Debug: Print camera color buffer info
                if (cameraColorBuffer != null && cameraColorBuffer.rt != null)
                {
                  
                }
                else
                {
                    Debug.LogError("[ColorHistoryPass] CameraColorBuffer or its RT is NULL!");
                    return;
                }
                
                // Copy current frame to history buffer
                CopyColorToHistory(cmd, cameraColorBuffer);
                
                renderContext.ExecuteCommandBuffer(cmd);
            }
            finally
            {
                CommandBufferPool.Release(cmd);
            }
        }

        /// <summary>
        /// Allocates the color history render texture
        /// </summary>
        private void AllocateColorHistory()
        {
            if (SoftwareTracingShared.ColorPreviousFrame.rt != null)
                return;

            // Allocate with full resolution and mipmap for screen space tracing
            SoftwareTracingShared.ColorPreviousFrame.HTextureAlloc(
                "_ColorPreviousFrame", 
                Vector2.one, 
                GraphicsFormat.B10G11R11_UFloatPack32, 
                useMipMap: true
            );
            
        }

        /// <summary>
        /// Copies the current camera color to the history buffer
        /// </summary>
        private void CopyColorToHistory(CommandBuffer cmd, RTHandle source)
        {
            if (SoftwareTracingShared.ColorPreviousFrame.rt == null)
            {
                Debug.LogWarning("[ColorHistoryPass] ColorPreviousFrame.rt is NULL, cannot copy!");
                return;
            }

            if (source == null)
            {
                return;
            }

            cmd.Blit(source, SoftwareTracingShared.ColorPreviousFrame.rt);
            
            // Generate mipmaps for screen space tracing
            // cmd.GenerateMips(SoftwareTracingShared.ColorPreviousFrame.rt);
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Cleanup()
        {
            if (SoftwareTracingShared.ColorPreviousFrame.rt != null)
            {
                SoftwareTracingShared.ColorPreviousFrame.HRelease();
            }

            _initialized = false;
        }
    }
}
