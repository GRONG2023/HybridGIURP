//pipelinedefine
#define H_HDRP

using System;
using System.Collections.Generic;
using HTraceWSGI.Scripts.Data.Private;
using HTraceWSGI.Scripts.Data.Public;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Passes.Shared;
using HTraceWSGI.Scripts.Services;
using HTraceWSGI.Scripts.Services.DirectionalShadowmap;
using HTraceWSGI.Scripts.Services.LightsCluster;
using HTraceWSGI.Scripts.Services.VoxelCameras;


using HTraceWSGI.Scripts.Passes.HDRP;
#if UNITY_EDITOR
using HTraceWSGI.Scripts.Patcher;
#endif

using UnityEngine;
using LightingSettings = HTraceWSGI.Scripts.Data.Public.LightingSettings;

#if UNITY_EDITOR
using UnityEditor;
using HTraceWSGI.Scripts.PipelinesConfigurator;
using static UnityEditor.ShaderData;
#endif

namespace HTraceWSGI.Scripts
{
    [ExecuteInEditMode, DefaultExecutionOrder(100)]
    public class HTraceWSGI : MonoBehaviour
    {

        private PrePassHDRP _prePass;
        private GameObject _prePassGameObject;
        private GBufferPassHDRP _gBufferPass;
        private GameObject _gBufferPassGameObject;
        private MotionVectorsPassHDRP _motionVectorsPass;
        private GameObject _motionVectorsPassGameObject;
        private DirectionalShadowmapPassHDRP _directionalShadowmapPass;
        private GameObject _directionalShadowmapPassGameObject;
        private VoxelizationPassHDRP _voxelizationPass;
        private GameObject _voxelizationPassGameObject;
        private SoftwareTracingPassHDRP _softwareTracingPass;
        private GameObject _softwareTracingPassGameObject;
        private FinalPassHDRP _finalDebugPass;
        private GameObject _finalDebugPassGameObject;

        private readonly HashSet<IService> _services = new HashSet<IService>();

        public GeneralSettings GeneralSettings = new GeneralSettings();
        public VoxelizationSettings VoxelizationSettings = new VoxelizationSettings();
        public LightingSettings LightingSettings = new LightingSettings();
        public ScreenSpaceLightingSettings ScreenSpaceLightingSettings = new ScreenSpaceLightingSettings();
        public ReflectionIndirectLightingSettings ReflectionIndirectLightingSettings = new ReflectionIndirectLightingSettings();

        [SerializeField]
        private DebugSettings DebugSettings = new DebugSettings();

        [SerializeField] private bool _globalSettingsTab = true;
        [SerializeField] private bool _screenSpaceLightingTab = true;
        [SerializeField] private bool _wsgiTab = true;
        [SerializeField] private bool _lightingTab = true;
        [SerializeField] private bool _reflectionsTab = true;
        [SerializeField] private bool _debugTab = true;

        [SerializeField] private bool _showVoxelParams = true;
        [SerializeField] private bool _showUpdateOptions = true;

        internal bool NeedToReallocForUI
        {
            get
            {
                return VoxelsService.Instance.NeedToReallocForUI;
            }
        }

        /// <summary>
        /// Reset Irradiance Cache
        /// </summary>
        public void ResetIrradianceCache()
        {
            SoftwareTracingShared.ClearRadianceCache = true;
        }

        /// <summary>
        /// Forced scene voxelization
        /// </summary>
        public void VoxelizeNow()
        {
            VoxelizationRuntimeData.FullVoxelization = true;
        }

        /// <summary>
        /// Apply Parameters, use only after changes setting's values in Parameters section. Do not recommend use it every frame.
        /// </summary>
        public void ApplyParameters()
        {
            VoxelizationRuntimeData.OnReallocTextures?.Invoke();
        }

        private void OnEnable()
        {
            HSettings.GeneralSettings = GeneralSettings;
            HSettings.VoxelizationSettings = VoxelizationSettings;
            HSettings.LightingSettings = LightingSettings;
            HSettings.ScreenSpaceLightingSettings = ScreenSpaceLightingSettings;
            HSettings.ReflectionIndirectLightingSettings = ReflectionIndirectLightingSettings;
            HSettings.DebugSettings = DebugSettings;

            VoxelizationRuntimeData.OnReallocTextures += () =>
            {
                HSettings.VoxelizationSettings.UpdateData();
                VoxelizationPassHDRP.Allocation();
                SoftwareTracingPassHDRP.AllocationHashBuffers();
                VoxelizationRuntimeData.FullVoxelization = true;
                VoxelizationRuntimeData.TextureSwapCounter = 0;
                VoxelizationRuntimeData.TextureOutputCounter = 0;
            };

#if UNITY_EDITOR
            HPipelinesConfigurator.AlwaysIncludedShaders();

            //HPatcher.RenderPipelineRuntimeResourcesPatch(true);
#endif

            VoxelizationRuntimeData.Initialize(); // must be before RegisterService, because needed to reset OctantIndex  
            RegisterServices(); // must be before InitComponentsBirp, because needed Initialize ExactData

            Shader.EnableKeyword(HNames.KEYWORD_SWITCHER);

            VoxelizationRuntimeData.OnReallocTextures?.Invoke();

        }



        private void Update()
        {
            foreach (var service in _services)
            {
                service.Update();
            }

#if UNITY_EDITOR
            if (gameObject.name != HNames.ASSET_NAME) gameObject.name = HNames.ASSET_NAME;
#endif
            //if (HRenderer.PipelineSupportsSSGI == true)
            Shader.EnableKeyword(HNames.KEYWORD_SWITCHER);

        }

        private void RegisterServices()
        {
            VoxelsService.Instance.Initialize(gameObject.layer);
            _services.Add(VoxelsService.Instance);
            DirectionalShadowmapService.Instance.Initialize(gameObject.layer);
            _services.Add(DirectionalShadowmapService.Instance);
            LightsService.Instance.Initialize();
            _services.Add(LightsService.Instance);

        }

        private void cleanPass()
        {
            if (_prePassGameObject != null)
            {
                DestroyImmediate(_prePassGameObject);
                _prePassGameObject = null;
            }

            if (_gBufferPassGameObject != null)
            {
                DestroyImmediate(_gBufferPassGameObject);
                _gBufferPassGameObject = null;
            }

            if (_motionVectorsPassGameObject != null)
            {

                DestroyImmediate(_motionVectorsPassGameObject);
                _motionVectorsPassGameObject = null;
            }

            if (_directionalShadowmapPassGameObject != null)
            {
                DestroyImmediate(_directionalShadowmapPassGameObject);
                _directionalShadowmapPassGameObject = null;
            }

            if (_voxelizationPassGameObject != null)
            {
                DestroyImmediate(_voxelizationPassGameObject);
                _voxelizationPassGameObject = null;
            }

            if (_softwareTracingPassGameObject != null)
            {
                DestroyImmediate(_softwareTracingPassGameObject);
                _softwareTracingPassGameObject = null;
            }

            if (_finalDebugPassGameObject != null)
            {
                // 找到对应的 GameObject 并销毁
                DestroyImmediate(_finalDebugPassGameObject);
                _finalDebugPassGameObject = null;
            }
        }

        private void OnDisable()
        {

            cleanPass();


            foreach (var service in _services)
            {
                service.Cleanup();
            }
            _services.Clear();

            VoxelizationRuntimeData.OnReallocTextures = null;

            Shader.DisableKeyword(HNames.KEYWORD_SWITCHER);

        }


        #region UTILITIES --------------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR

        private void OnTransformChildrenChanged()
        {
            foreach (Transform child in this.transform)
            {
                if (child.name == HNames.HTRACE_PRE_PASS_NAME ||
                    child.name == HNames.HTRACE_MV_PASS_NAME ||
                    child.name == HNames.HTRACE_GBUFFER_PASS_NAME ||
                    child.name == HNames.HTRACE_SHADOWMAP_PASS_NAME ||
                    child.name == HNames.HTRACE_VOXELIZATION_PASS_NAME ||
                    child.name == HNames.HTRACE_LIGHT_CLUSTER_PASS_NAME ||
                    child.name == HNames.HTRACE_SOFTWARE_TRACING_PASS_NAME ||
                    child.name == HNames.HTRACE_HARDWARE_TRACING_PASS_NAME ||
                    child.name == HNames.HTRACE_FINAL_PASS_NAME ||
                    child.name == HNames.HTRACE_VOXEL_CAMERA_NAME)
                    continue;

                child.parent = null;
                Debug.Log($"Cann't add a \"{child.name}\" gameobject to HTraceWSGI.");
            }
        }
#endif

        #endregion UTILITIES --------------------------------------------------------------------------------------------------------------------------

    }
}
