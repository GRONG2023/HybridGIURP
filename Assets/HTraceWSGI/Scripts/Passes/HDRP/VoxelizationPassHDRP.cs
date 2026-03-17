//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Data.Private;
using HTraceWSGI.Scripts.Extensions;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Passes.Shared;
using HTraceWSGI.Scripts.Wrappers;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HTraceWSGI.Scripts.Passes.HDRP
{

        public class VoxelizationPassHDRP : ScriptableRenderPass
        {
            private static bool _initialized = false;

            public VoxelizationPassHDRP()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingGbuffer + 4;
        }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {

                if (!_initialized)
                {
                    if (VoxelizationShared.VoxelizationShader == null)
                        VoxelizationShared.VoxelizationShader = Shader.Find("Hidden/HTraceWSGI/VoxelizationHDRP");
                    if (VoxelizationShared.VoxelVisualizationMaterialHDRP == null)
                        VoxelizationShared.VoxelVisualizationMaterialHDRP = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/HTraceWSGI/VoxelVisualizationHDRP"));
                    if (VoxelizationShared.HVoxelization == null)
                        VoxelizationShared.HVoxelization = HExtensions.LoadComputeShader("Voxelization");
                    if (VoxelizationShared.VoxelVisualization == null)
                        VoxelizationShared.VoxelVisualization = HExtensions.LoadComputeShader("VoxelVisualization");

                    _initialized = true;
                }

                Allocation();
            }

            internal static void Allocation(bool onlyRelease = false)
            {
                void ReleaseBuffersAndTextures()
                {
                    //constant
                    VoxelizationShared.DummyVoxelizationTarget.HRelease();
                    VoxelizationShared.VoxelData.HRelease();

                    //partial
                    VoxelizationShared.DummyVoxelizationStaticTarget.HRelease();
                    VoxelizationShared.DummyVoxelizationDynamicTarget.HRelease();
                    VoxelizationShared.VoxelData_A.HRelease();
                    VoxelizationShared.VoxelData_B.HRelease();

                    //general
                    VoxelizationShared.VoxelPositionPyramid.HRelease();
                    VoxelizationShared.VoxelPositionIntermediate.HRelease();

                    VoxelizationShared.DummyVoxelBuffer.HRelease();

                    //Debug
                    VoxelizationShared.VoxelVisualizationRayDirections.HRelease();
                    VoxelizationShared.DebugOutput.HRelease();
                }

                if (onlyRelease)
                {
                    ReleaseBuffersAndTextures();
                    return;
                }

                ReleaseBuffersAndTextures();

                VoxelizationShared.DummyVoxelBuffer = new ComputeBuffer(1, sizeof(int));

                int voxelResX = HSettings.VoxelizationSettings.ExactData.Resolution.x;
                int voxelResY = HSettings.VoxelizationSettings.ExactData.Resolution.z;
                int voxelResZ = HSettings.VoxelizationSettings.ExactData.Resolution.y;

                VoxelizationShared.VoxelPositionPyramid.HTextureAlloc(name: "_VoxelPositionPyramid", voxelResX, voxelResY, GraphicsFormat.R8_UInt, voxelResZ, textureDimension: TextureDimension.Tex3D, useDynamicScale: false, useMipMap: true, autoGenerateMips: false);
                VoxelizationShared.VoxelPositionIntermediate.HTextureAlloc(name: "_VoxelPositionIntermediate", voxelResX / 4, voxelResY / 4, GraphicsFormat.R8_UInt, voxelResZ / 4, textureDimension: TextureDimension.Tex3D, useDynamicScale: false, autoGenerateMips: false);

                switch (HSettings.VoxelizationSettings.VoxelizationUpdateMode)
                {
                    case VoxelizationUpdateMode.Constant:
                        VoxelizationShared.DummyVoxelizationTarget.HTextureAlloc(name: "_DummyVoxelizationDynamicTarget", voxelResX * 2, voxelResZ * 2, GraphicsFormat.R8_UNorm, textureDimension: TextureDimension.Tex2D, useDynamicScale: false);
                        VoxelizationShared.VoxelData.HTextureAlloc(name: "_VoxelData", voxelResX, voxelResY, GraphicsFormat.R32_UInt, voxelResZ, textureDimension: TextureDimension.Tex3D, useDynamicScale: false, autoGenerateMips: false);
                        break;
                    case VoxelizationUpdateMode.Partial:
                        VoxelizationShared.DummyVoxelizationStaticTarget.HTextureAlloc(name: "_DummyVoxelizationStaticTarget", voxelResX, voxelResZ, GraphicsFormat.R8_UNorm, textureDimension: TextureDimension.Tex2D, useDynamicScale: false);
                        VoxelizationShared.DummyVoxelizationDynamicTarget.HTextureAlloc(name: "_DummyVoxelizationDynamicTarget", voxelResX * 2, voxelResZ * 2, GraphicsFormat.R8_UNorm, textureDimension: TextureDimension.Tex2D, useDynamicScale: false);
                        VoxelizationShared.VoxelData_A.HTextureAlloc(name: "_VoxelData_A", voxelResX, voxelResY, GraphicsFormat.R32_UInt, voxelResZ, textureDimension: TextureDimension.Tex3D, useDynamicScale: false);
                        VoxelizationShared.VoxelData_B.HTextureAlloc(name: "_VoxelData_B", voxelResX, voxelResY, GraphicsFormat.R32_UInt, voxelResZ, textureDimension: TextureDimension.Tex3D, useDynamicScale: false);
                        break;
                }

                if (HSettings.GeneralSettings.DebugModeWS != DebugModeWS.None)
                {
                    VoxelizationShared.VoxelVisualizationRayDirections.HTextureAlloc("_VoxelVisualizationRayDirections", Vector2.one, GraphicsFormat.R16G16B16A16_SFloat);
                    VoxelizationShared.DebugOutput.HTextureAlloc("_DebugOutput", Vector2.one, GraphicsFormat.B10G11R11_UFloatPack32);
                }

                VoxelizationRuntimeData.FullVoxelization = true;
            }

            public override void Execute(ScriptableRenderContext renderContext, ref RenderingData renderingData)
            {
                var camera = renderingData.cameraData.camera;
                int width = renderingData.cameraData.cameraTargetDescriptor.width;
                int height = renderingData.cameraData.cameraTargetDescriptor.height;

                if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
                    return;

                var cmd = CommandBufferPool.Get(HNames.HTRACE_VOXELIZATION_PASS_NAME);

                try
                {
                    VoxelizationRuntimeData.VoxelCamera.ExecuteUpdate(camera);

                    ReallocateConditions(width, height);

                    using (new HTraceProfilingScope(cmd, VoxelizationShared.s_VoxelizationConstantProfilingSampler))
                        VoxelizationConstant(cmd, camera, width, height, renderContext, ref renderingData);

                    VoxelizationShared.History.Update();

                    renderContext.ExecuteCommandBuffer(cmd);
                }
                finally
                {
                    CommandBufferPool.Release(cmd);
                }
            }

            private void ReallocateConditions(int width, int height)
            {
                if (VoxelizationShared.History.TracingMode != HSettings.GeneralSettings.TracingMode
                    || VoxelizationShared.History.VoxelizationUpdateMode != HSettings.VoxelizationSettings.VoxelizationUpdateMode
                    || VoxelizationShared.History.DebugMode != HSettings.GeneralSettings.DebugModeWS
                   )
                {
                    Allocation(HSettings.GeneralSettings.TracingMode == Globals.TracingMode.HardwareTracing);
                }
            }

            private static int _FrameIndex = 0;

            private void VoxelizationConstant(CommandBuffer cmd, Camera camera, int cameraWidth, int cameraHeight, ScriptableRenderContext renderContext, ref RenderingData renderingData)
            {
                if (VoxelizationRuntimeData.VoxelCamera == null)
                    return;

                _FrameIndex++;
                //if (_FrameIndex % 4 != 0)
                //    return;

                // Clear 3D textures
                CoreUtils.SetRenderTarget(cmd, VoxelizationShared.VoxelData.rt, ClearFlag.Color, Color.clear, 0, CubemapFace.Unknown, -1);
                CoreUtils.SetRenderTarget(cmd, VoxelizationShared.VoxelPositionPyramid.rt, ClearFlag.Color, Color.clear, 0, CubemapFace.Unknown, -1);
                CoreUtils.SetRenderTarget(cmd, VoxelizationShared.VoxelPositionPyramid.rt, ClearFlag.Color, Color.clear, 1, CubemapFace.Unknown, -1);
                CoreUtils.SetRenderTarget(cmd, VoxelizationShared.VoxelPositionPyramid.rt, ClearFlag.Color, Color.clear, 2, CubemapFace.Unknown, -1);
                CoreUtils.SetRenderTarget(cmd, VoxelizationShared.VoxelPositionPyramid.rt, ClearFlag.Color, Color.clear, 3, CubemapFace.Unknown, -1);
                CoreUtils.SetRenderTarget(cmd, VoxelizationShared.VoxelPositionPyramid.rt, ClearFlag.Color, Color.clear, 4, CubemapFace.Unknown, -1);
                CoreUtils.SetRenderTarget(cmd, VoxelizationShared.VoxelPositionPyramid.rt, ClearFlag.Color, Color.clear, 5, CubemapFace.Unknown, -1);
                CoreUtils.SetRenderTarget(cmd, VoxelizationShared.VoxelPositionIntermediate.rt, ClearFlag.Color, Color.clear, 0, CubemapFace.Unknown, -1);

                // Pass voxel camera pos to shaders
                cmd.SetGlobalVector(VoxelizationShared.g_VoxelCameraPos, VoxelizationRuntimeData.VoxelCamera.transform.position);

                var voxelizationCamera = VoxelizationRuntimeData.VoxelCamera.Camera;

                if (true)
                {
                    cmd.SetGlobalVector(VoxelizationShared.g_VoxelResolution, (Vector3)HSettings.VoxelizationSettings.ExactData.Resolution);
                    cmd.SetGlobalVector(VoxelizationShared.g_VoxelBounds, HSettings.VoxelizationSettings.ExactData.Bounds);
                    cmd.SetGlobalFloat(VoxelizationShared.g_VoxelPerMeter, HSettings.VoxelizationSettings.ExactData.VoxelsPerMeter);
                    cmd.SetGlobalFloat(VoxelizationShared.g_VoxelSize, HSettings.VoxelizationSettings.ExactData.VoxelSize);

                    Vector3 BoundsSwizzled = new Vector3(HSettings.VoxelizationSettings.ExactData.Bounds.x, HSettings.VoxelizationSettings.ExactData.Bounds.z, HSettings.VoxelizationSettings.ExactData.Bounds.y);
                    Bounds voxelizationAABB = new Bounds(VoxelizationRuntimeData.VoxelCamera.transform.position, BoundsSwizzled);
                    cmd.SetGlobalVector(VoxelizationShared.g_VoxelizationAABB_Min, voxelizationAABB.min);
                    cmd.SetGlobalVector(VoxelizationShared.g_VoxelizationAABB_Max, voxelizationAABB.max);
                    //Debug.Log("voxelizationAABB = "+ voxelizationAABB.min +"  "+ voxelizationAABB.max);
                    if (voxelizationCamera.farClipPlane > 0)
                    {
                        RenderVoxelsConstant(cmd, camera, cameraWidth, cameraHeight, renderContext, ref renderingData);
                    }
                }

                // Generate mip pyramid for 3D position texture
                using (new HTraceProfilingScope(cmd, VoxelizationShared.s_GeneratePositionPyramidProfilingSampler))
                {
                    // Generate 0-2 mip levels
                    cmd.SetComputeTextureParam(VoxelizationShared.HVoxelization, (int)VoxelizationShared.HVoxelizationKernel.GeneratePositionPyramid1, VoxelizationShared.g_VoxelData, VoxelizationShared.VoxelData.rt);
                    cmd.SetComputeTextureParam(VoxelizationShared.HVoxelization, (int)VoxelizationShared.HVoxelizationKernel.GeneratePositionPyramid1, VoxelizationShared._VoxelPositionPyramid_Mip0, VoxelizationShared.VoxelPositionPyramid.rt, 0);
                    cmd.SetComputeTextureParam(VoxelizationShared.HVoxelization, (int)VoxelizationShared.HVoxelizationKernel.GeneratePositionPyramid1, VoxelizationShared._VoxelPositionPyramid_Mip1, VoxelizationShared.VoxelPositionPyramid.rt, 1);
                    cmd.SetComputeTextureParam(VoxelizationShared.HVoxelization, (int)VoxelizationShared.HVoxelizationKernel.GeneratePositionPyramid1, VoxelizationShared._VoxelPositionPyramid_Mip2, VoxelizationShared.VoxelPositionPyramid.rt, 2);
                    cmd.SetComputeTextureParam(VoxelizationShared.HVoxelization, (int)VoxelizationShared.HVoxelizationKernel.GeneratePositionPyramid1, VoxelizationShared._VoxelPositionIntermediate_Output, VoxelizationShared.VoxelPositionIntermediate.rt);
                    cmd.DispatchCompute(VoxelizationShared.HVoxelization, (int)VoxelizationShared.HVoxelizationKernel.GeneratePositionPyramid1,
                        Mathf.CeilToInt(HSettings.VoxelizationSettings.ExactData.Resolution.x / 8f),
                        Mathf.CeilToInt(HSettings.VoxelizationSettings.ExactData.Resolution.z / 8f),
                        Mathf.CeilToInt(HSettings.VoxelizationSettings.ExactData.Resolution.y / 8f));

                    // Generate 3-5 mip levels
                    cmd.SetComputeTextureParam(VoxelizationShared.HVoxelization, (int)VoxelizationShared.HVoxelizationKernel.GeneratePositionPyramid2, VoxelizationShared._VoxelPositionIntermediate, VoxelizationShared.VoxelPositionIntermediate.rt);
                    cmd.SetComputeTextureParam(VoxelizationShared.HVoxelization, (int)VoxelizationShared.HVoxelizationKernel.GeneratePositionPyramid2, VoxelizationShared._VoxelPositionPyramid_Mip3, VoxelizationShared.VoxelPositionPyramid.rt, 3);
                    cmd.SetComputeTextureParam(VoxelizationShared.HVoxelization, (int)VoxelizationShared.HVoxelizationKernel.GeneratePositionPyramid2, VoxelizationShared._VoxelPositionPyramid_Mip4, VoxelizationShared.VoxelPositionPyramid.rt, 4);
                    cmd.SetComputeTextureParam(VoxelizationShared.HVoxelization, (int)VoxelizationShared.HVoxelizationKernel.GeneratePositionPyramid2, VoxelizationShared._VoxelPositionPyramid_Mip5, VoxelizationShared.VoxelPositionPyramid.rt, 5);
                    cmd.DispatchCompute(VoxelizationShared.HVoxelization, (int)VoxelizationShared.HVoxelizationKernel.GeneratePositionPyramid2,
                        Mathf.CeilToInt(HSettings.VoxelizationSettings.ExactData.Resolution.x / 32f),
                        Mathf.CeilToInt(HSettings.VoxelizationSettings.ExactData.Resolution.z / 32f),
                        Mathf.CeilToInt(HSettings.VoxelizationSettings.ExactData.Resolution.y / 32f));
                }

                // Pass voxelized textures to shaders in the Main Pass
                cmd.SetGlobalTexture(VoxelizationShared.g_VoxelPositionPyramid, VoxelizationShared.VoxelPositionPyramid.rt);
                cmd.SetGlobalTexture(VoxelizationShared.g_VoxelData, VoxelizationShared.VoxelData.rt);

            VoxelizationRuntimeData.FullVoxelization = false;
            }

            private void RenderVoxelsConstant(CommandBuffer cmd, Camera camera, int cameraWidth, int cameraHeight, ScriptableRenderContext renderContext, ref RenderingData renderingData)
            {
                // Cache main camera matrices
                var viewMatrixCached = camera.worldToCameraMatrix;
                var projectionMatrixCached = camera.projectionMatrix;

                var voxelizationCamera = VoxelizationRuntimeData.VoxelCamera.Camera;

                cmd.SetViewProjectionMatrices(voxelizationCamera.worldToCameraMatrix, voxelizationCamera.projectionMatrix);

                cmd.ClearRandomWriteTargets();
                cmd.SetRandomWriteTarget(1, VoxelizationShared.VoxelData.rt);
                cmd.SetRandomWriteTarget(2, VoxelizationShared.DummyVoxelBuffer, false);

                // 提交已积累的cmd，确保UAV和矩阵设置在Cull之前生效
                renderContext.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                if (voxelizationCamera.TryGetCullingParameters(out ScriptableCullingParameters voxelizationCullingParams))
                {
                    voxelizationCullingParams.cullingOptions = CullingOptions.None;
                    voxelizationCullingParams.isOrthographic = true;

                    LODParameters lodParameters = voxelizationCullingParams.lodParameters;
                    lodParameters.cameraPosition = voxelizationCamera.transform.position;
                    lodParameters.isOrthographic = true;
                    lodParameters.orthoSize = 0;
                    voxelizationCullingParams.lodParameters = lodParameters;

                    var cullingResults = renderContext.Cull(ref voxelizationCullingParams);

                    LayerMask voxelizationLayer = HSettings.VoxelizationSettings.VoxelizationMask;
                    RenderTexture voxelizationRenderTarget = VoxelizationShared.DummyVoxelizationTarget.rt;
                    int voxelizationShaderPass = 0;

                    Shader.EnableKeyword(VoxelizationShared.CONSTANT_VOXELIZATION);
                    Shader.DisableKeyword(VoxelizationShared.PARTIAL_VOXELIZATION);
                    Shader.DisableKeyword(VoxelizationShared.DYNAMIC_VOXELIZATION);

                    var maximumLODLevelBackup = QualitySettings.maximumLODLevel;
                    var lodBiasBackup = QualitySettings.lodBias;
                    QualitySettings.SetLODSettings(1, HSettings.VoxelizationSettings.LODMax, false);

                    RenderVoxels(cmd, renderContext, cullingResults, voxelizationCamera, voxelizationRenderTarget, voxelizationLayer, overriderShader: VoxelizationShared.VoxelizationShader, overrideMaterial: null, voxelizationShaderPass);

                    QualitySettings.SetLODSettings(lodBiasBackup, maximumLODLevelBackup, false);

                    cmd.ClearRandomWriteTargets();
                }

                cmd.SetViewProjectionMatrices(viewMatrixCached, projectionMatrixCached);
            }

            private static ShaderTagId[] VoxelizationTags = null;
            private static ShaderTagId[] HTraceTags = null;

            private void RenderVoxels(CommandBuffer cmd, ScriptableRenderContext renderContext, CullingResults cullingResults, Camera voxelizationCamera, RenderTexture renderTarget, LayerMask layerMask, Shader overriderShader = null, Material overrideMaterial = null, int shaderPass = 0)
            {
                cmd.SetRenderTarget(renderTarget.colorBuffer, renderTarget.depthBuffer);
                cmd.ClearRenderTarget(true, true, Color.clear);

                renderContext.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                using (new HTraceProfilingScope(cmd, VoxelizationShared.s_RenderVoxelsProfilingSampler))
                {
                    if (VoxelizationTags == null)
                        VoxelizationTags = new ShaderTagId[]
                        {
                        new ShaderTagId("UniversalGBuffer"),
                        };

                    if (HTraceTags == null)
                        HTraceTags = new ShaderTagId[]
                        {
                        new ShaderTagId(HNames.HTRACE_VOXELIZATION_SHADER_TAG_ID)
                        };

                    var renderList = new UnityEngine.Rendering.RendererUtils.RendererListDesc(VoxelizationTags, cullingResults, voxelizationCamera)
                    {
                        rendererConfiguration = PerObjectData.None,
                        renderQueueRange = RenderQueueRange.opaque,
                        sortingCriteria = SortingCriteria.OptimizeStateChanges,
                        layerMask = layerMask,
                        overrideShader = overriderShader,
                        overrideMaterial = overrideMaterial,
                        overrideShaderPassIndex = shaderPass,
                    };
                    CoreUtils.DrawRendererList(renderContext, cmd, renderContext.CreateRendererList(renderList));

                    var renderListProcedural = new UnityEngine.Rendering.RendererUtils.RendererListDesc(HTraceTags, cullingResults, voxelizationCamera)
                    {
                        rendererConfiguration = PerObjectData.None,
                        renderQueueRange = RenderQueueRange.opaque,
                        sortingCriteria = SortingCriteria.OptimizeStateChanges,
                        overrideShader = null,
                        overrideMaterial = null,
                        layerMask = layerMask,
                    };
                    CoreUtils.DrawRendererList(renderContext, cmd, renderContext.CreateRendererList(renderListProcedural));

                    renderContext.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                }
            }

            internal static void VisualizeVoxels(CommandBuffer cmd, Camera camera, int cameraWidth, int cameraHeight, ScriptableRenderContext renderContext)
            {
                VoxelizationShared.VoxelVisualization.EnableKeyword(VoxelizationShared.VISUALIZE_OFF);
                VoxelizationShared.VoxelVisualization.DisableKeyword(VoxelizationShared.VISUALIZE_LIGHTING);
                VoxelizationShared.VoxelVisualization.DisableKeyword(VoxelizationShared.VISUALIZE_COLOR);

                if (HSettings.LightingSettings.EvaluatePunctualLights)
                    VoxelizationShared.VoxelVisualization.EnableKeyword(VoxelizationShared.EVALUATE_PUNCTUAL_LIGHTS);
                else
                    VoxelizationShared.VoxelVisualization.DisableKeyword(VoxelizationShared.EVALUATE_PUNCTUAL_LIGHTS);

                if (HSettings.GeneralSettings.DebugModeWS == DebugModeWS.VoxelizedLighting || HSettings.GeneralSettings.DebugModeWS == DebugModeWS.VoxelizedColor)
                {
                    using (new HTraceProfilingScope(cmd, VoxelizationShared.s_VisualizeVoxelsProfilingSampler))
                    {
                        if (HSettings.GeneralSettings.DebugModeWS == DebugModeWS.VoxelizedLighting)
                        {
                            VoxelizationShared.VoxelVisualization.EnableKeyword(VoxelizationShared.VISUALIZE_LIGHTING);
                            VoxelizationShared.VoxelVisualization.DisableKeyword(VoxelizationShared.VISUALIZE_COLOR);
                            VoxelizationShared.VoxelVisualization.DisableKeyword(VoxelizationShared.VISUALIZE_OFF);
                        }

                        if (HSettings.GeneralSettings.DebugModeWS == DebugModeWS.VoxelizedColor)
                        {
                            VoxelizationShared.VoxelVisualization.EnableKeyword(VoxelizationShared.VISUALIZE_COLOR);
                            VoxelizationShared.VoxelVisualization.DisableKeyword(VoxelizationShared.VISUALIZE_LIGHTING);
                            VoxelizationShared.VoxelVisualization.DisableKeyword(VoxelizationShared.VISUALIZE_OFF);
                        }

                        Vector2Int runningRes = new Vector2Int(cameraWidth, cameraHeight);

                        int fullResX_8 = Mathf.CeilToInt((float)runningRes.x / 8);
                        int fullResY_8 = Mathf.CeilToInt((float)runningRes.y / 8);

                        var debugCameraFrustum = HMath.ComputeFrustumCorners(camera);

                        VoxelizationShared.VoxelVisualizationMaterialHDRP.SetMatrix(HShaderParams._DebugCameraFrustum, debugCameraFrustum);
                        cmd.Blit(null, VoxelizationShared.VoxelVisualizationRayDirections.rt, VoxelizationShared.VoxelVisualizationMaterialHDRP, 0);

                        cmd.SetComputeTextureParam(VoxelizationShared.VoxelVisualization, (int)VoxelizationShared.VoxelVisualizationKernel.VisualizeVoxels, HShaderParams._DebugRayDirection, VoxelizationShared.VoxelVisualizationRayDirections.rt);
                        cmd.SetComputeTextureParam(VoxelizationShared.VoxelVisualization, (int)VoxelizationShared.VoxelVisualizationKernel.VisualizeVoxels, HShaderParams._Visualization_Output, VoxelizationShared.DebugOutput.rt);
                        cmd.SetComputeIntParam(VoxelizationShared.VoxelVisualization, HShaderParams._MultibounceMode, (int)HSettings.GeneralSettings.Multibounce);
                        cmd.DispatchCompute(VoxelizationShared.VoxelVisualization, (int)VoxelizationShared.VoxelVisualizationKernel.VisualizeVoxels, fullResX_8, fullResY_8, 1);

                        if (HSettings.GeneralSettings.DebugModeWS != DebugModeWS.None)
                            cmd.SetGlobalTexture(HShaderParams.g_HTraceBufferGI, VoxelizationShared.DebugOutput.rt);
                    }
                }
            }

            public void Cleanup()
            {
                _initialized = false;

                //constant
                VoxelizationShared.DummyVoxelizationTarget.HRelease();
                VoxelizationShared.VoxelData.HRelease();

                //partial
                VoxelizationShared.DummyVoxelizationStaticTarget.HRelease();
                VoxelizationShared.DummyVoxelizationDynamicTarget.HRelease();
                VoxelizationShared.VoxelData_A.HRelease();
                VoxelizationShared.VoxelData_B.HRelease();

                //general
                VoxelizationShared.VoxelPositionPyramid.HRelease();
                VoxelizationShared.VoxelPositionIntermediate.HRelease();

                VoxelizationShared.DummyVoxelBuffer.HRelease();

                //Debug
                VoxelizationShared.VoxelVisualizationRayDirections.HRelease();
                VoxelizationShared.DebugOutput.HRelease();
            }
        }
}