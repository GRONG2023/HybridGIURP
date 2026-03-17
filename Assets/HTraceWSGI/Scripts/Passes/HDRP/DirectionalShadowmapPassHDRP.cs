//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Data.Private;
using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Services.DirectionalShadowmap;
using HTraceWSGI.Scripts.Wrappers;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HTraceWSGI.Scripts.Passes.HDRP
{
    public class DirectionalShadowmapPassHDRP : ScriptableRenderPass
    {
        private enum HShadowmapKernel
        {
            ShadowmapMerge = 0,
        }

        // Shader properties
        internal static readonly int g_DirLightMatrix = Shader.PropertyToID("g_DirLightMatrix");
        internal static readonly int g_DirLightPlanes = Shader.PropertyToID("g_DirLightPlanes");
        internal static readonly int g_HTraceShadowmap = Shader.PropertyToID("g_HTraceShadowmap");

        internal static readonly int _DirectionalShadowmapStatic = Shader.PropertyToID("_DirectionalShadowmapStatic");
        internal static readonly int _Shadowmap = Shader.PropertyToID("_Shadowmap");
        internal static readonly int _Shadowmap_Output = Shader.PropertyToID("_Shadowmap_Output");
        internal static readonly int _OctantShadowOffset = Shader.PropertyToID("_OctantShadowOffset");

        // Samplers
        internal static readonly ProfilingSamplerHTrace s_RenderShadowmapProfilingSampler = new ProfilingSamplerHTrace("Render Shadowmap", parentName: HNames.HTRACE_SHADOWMAP_PASS_NAME, priority: 0);
        internal static readonly ProfilingSamplerHTrace s_RenderShadowmapDrawRendererListProfilingSampler = new ProfilingSamplerHTrace("Render Shadowmap DrawRendererList");
        internal static readonly ProfilingSamplerHTrace s_MergeShadowmapStaticProfilingSampler = new ProfilingSamplerHTrace("Merge Shadowmap Static");


        // Textures
        internal static RTWrapper DirectionalDepthTarget = new RTWrapper();
        internal static RTWrapper DirectionalDepthTargetCombined = new RTWrapper();
        internal static RTWrapper DirectionalDepthTargetStatic = new RTWrapper();
        internal static RTWrapper DirectionalShadowmapStatic = new RTWrapper();

        internal struct HistoryData : IHistoryData
        {
            public VoxelizationUpdateMode VoxelizationUpdateMode;

            public void Update()
            {
                VoxelizationUpdateMode = HSettings.VoxelizationSettings.VoxelizationUpdateMode;
            }
        }

        internal static HistoryData History = new HistoryData();

        private bool _initialized = false;

        public DirectionalShadowmapPassHDRP()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingGbuffer + 3;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!_initialized)
            {
                Allocation();
                _initialized = true;
            }
        }

        private static void Allocation(bool onlyRelease = false)
        {
            void ReleaseTextures()
            {
                DirectionalDepthTarget.HRelease();
                DirectionalDepthTargetCombined.HRelease();
                DirectionalDepthTargetStatic.HRelease();
                DirectionalShadowmapStatic.HRelease();
            }

            if (onlyRelease)
            {
                ReleaseTextures();
                return;
            }

            ReleaseTextures();

            DirectionalDepthTarget.HTextureAlloc(name: "_DirectionalDepthTargetCombined", (int)HConstants.ShadowmapResolution.x, (int)HConstants.ShadowmapResolution.y, GraphicsFormat.R8_SNorm, enableRandomWrite: false, textureDimension: TextureDimension.Tex2D, depthBufferBits: 32, useDynamicScale: false);
        }

        public override void Execute(ScriptableRenderContext renderContext, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            int width = renderingData.cameraData.cameraTargetDescriptor.width;
            int height = renderingData.cameraData.cameraTargetDescriptor.height;

            if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
                return;

            var cmd = CommandBufferPool.Get(HNames.HTRACE_SHADOWMAP_PASS_NAME);

            try
            {
                ReallocateConditions(width, height);

                DirectionalShadowmapService.Instance.DirectionalCamera.ExecuteUpdate();

                using (new HTraceProfilingScope(cmd, s_RenderShadowmapProfilingSampler))
                    ExecuteConstant(cmd, camera, width, height, renderContext, renderingData);

                History.Update();

                renderContext.ExecuteCommandBuffer(cmd);
            }
            finally
            {
                CommandBufferPool.Release(cmd);
            }
        }

        private void ReallocateConditions(int width, int height)
        {
            if (History.VoxelizationUpdateMode != HSettings.VoxelizationSettings.VoxelizationUpdateMode)
            {
                Allocation();
            }
        }

        internal static void ExecuteConstant(CommandBuffer cmd, Camera camera, int cameraWidth, int cameraHeight, ScriptableRenderContext renderContext, RenderingData renderingData)
        {
            // Cache main camera matrices
            var viewMatrixCached = camera.worldToCameraMatrix;
            var projectionMatrixCached = camera.projectionMatrix;

            var voxelizationCamera = VoxelizationRuntimeData.VoxelCamera.Camera;
            var directionalLightCamera = DirectionalShadowmapService.Instance.DirectionalCamera.GetDirectionalCamera;
            cmd.SetViewProjectionMatrices(directionalLightCamera.worldToCameraMatrix, directionalLightCamera.projectionMatrix);

            // 提交cmd确保矩阵设置生效
            renderContext.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // URP 下最简单稳定的方案
            //directionalLightCamera.enabled = true;  // 保持开启

            //var additionalData = directionalLightCamera.GetUniversalAdditionalCameraData();
            //additionalData.renderType = CameraRenderType.Overlay; // 不做主渲染
            //                                                      // 或者
            //directionalLightCamera.cullingMask = 0; // 啥都不渲染

            //// 打印所有参数排查
            //Debug.Log($"enabled: {directionalLightCamera.enabled}");
            //Debug.Log($"near: {directionalLightCamera.nearClipPlane}");
            //Debug.Log($"far: {directionalLightCamera.farClipPlane}");
            //Debug.Log($"ortho: {directionalLightCamera.orthographic}");
            //Debug.Log($"orthoSize: {directionalLightCamera.orthographicSize}");
            //Debug.Log($"fov: {directionalLightCamera.fieldOfView}");
            //Debug.Log($"aspect: {directionalLightCamera.aspect}");

            if (directionalLightCamera.TryGetCullingParameters(out ScriptableCullingParameters shadowCullingParams))
            {
                shadowCullingParams.cullingOptions = CullingOptions.None;
                shadowCullingParams.isOrthographic = true;

                LODParameters lodParameters = shadowCullingParams.lodParameters;
                lodParameters.cameraPosition = voxelizationCamera.transform.position;
                lodParameters.isOrthographic = true;
                lodParameters.orthoSize = 0;
                shadowCullingParams.lodParameters = lodParameters;

                var cullingResults = renderContext.Cull(ref shadowCullingParams);

                LayerMask shadowmapLayer = HSettings.VoxelizationSettings.VoxelizationMask;
                var shadowmapRenderTarget = DirectionalDepthTarget;
                ClearFlag clearDepthFlag = ClearFlag.Depth;

                var viewMatrix = directionalLightCamera.worldToCameraMatrix;
                var projectionMatrix = GL.GetGPUProjectionMatrix(directionalLightCamera.projectionMatrix, false);

                cmd.SetGlobalMatrix(g_DirLightMatrix, projectionMatrix * viewMatrix);
                cmd.SetGlobalVector(g_DirLightPlanes, new Vector2(directionalLightCamera.nearClipPlane, directionalLightCamera.farClipPlane));

                var maximumLODLevelBackup = QualitySettings.maximumLODLevel;
                var lodBiasBackup = QualitySettings.lodBias;
                QualitySettings.SetLODSettings(1, HSettings.VoxelizationSettings.LODMax, false);

                RenderShadowmap(cmd, renderContext, cullingResults, directionalLightCamera, shadowmapRenderTarget.rt, shadowmapRenderTarget.rt, shadowmapLayer,
                    overrideMaterial: null, clearFlag: clearDepthFlag, useShadowCasterPass: true);

                QualitySettings.SetLODSettings(lodBiasBackup, maximumLODLevelBackup, false);
            }

                cmd.SetGlobalTexture(g_HTraceShadowmap, DirectionalDepthTarget.rt);
            cmd.SetViewProjectionMatrices(viewMatrixCached, projectionMatrixCached);
        }

        private static ShaderTagId[] ShaderForwardTags = null;
        private static ShaderTagId[] ShadowCasterTags = null;

        private static void RenderShadowmap(CommandBuffer cmd, ScriptableRenderContext renderContext, CullingResults cullingResults, Camera voxelizationCamera, RenderTexture colorTarget, RenderTexture depthTarget, LayerMask layerMask,
            Shader overriderShader = null, Material overrideMaterial = null, int shaderPass = 0, ClearFlag clearFlag = ClearFlag.None, bool useShadowCasterPass = false)
        {
            if (clearFlag != ClearFlag.None)
                CoreUtils.SetRenderTarget(cmd, colorTarget, depthTarget, clearFlag);

            // 提交SetRenderTarget
            renderContext.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            using (new HTraceProfilingScope(cmd, s_RenderShadowmapDrawRendererListProfilingSampler))
            {
                if (ShaderForwardTags == null)
                    ShaderForwardTags = new ShaderTagId[]
                    {
                        new ShaderTagId("UniversalForward"),
                        new ShaderTagId("UniversalGBuffer"),
                        new ShaderTagId("SRPDefaultUnlit"),
                    };

                if (ShadowCasterTags == null)
                    ShadowCasterTags = new ShaderTagId[]
                    {
                        new ShaderTagId("ShadowCaster")
                    };

                useShadowCasterPass = true;

                var renderList = new UnityEngine.Rendering.RendererUtils.RendererListDesc(useShadowCasterPass == false ? ShaderForwardTags : ShadowCasterTags, cullingResults, voxelizationCamera)
                {
                    rendererConfiguration = PerObjectData.None,
                    renderQueueRange = RenderQueueRange.opaque,
                    sortingCriteria = SortingCriteria.OptimizeStateChanges,
                    overrideShader = overriderShader,
                    overrideMaterial = useShadowCasterPass ? null : overrideMaterial,
                    overrideShaderPassIndex = shaderPass,
                    layerMask = layerMask,
                };

                CoreUtils.DrawRendererList(renderContext, cmd, renderContext.CreateRendererList(renderList));

                renderContext.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }
        }

        public void Cleanup()
        {
            Allocation(true);
            _initialized = false;
        }
    }
}