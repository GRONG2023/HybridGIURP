//pipelinedefine
#define H_HDRP

#if UNITY_EDITOR

using System;
using HTraceWSGI.Scripts.Data.Private;
using HTraceWSGI.Scripts.Editor.WindowsAndMenu;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Services.VoxelCameras;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace HTraceWSGI.Scripts.Editor
{
    [CustomEditor(typeof(Scripts.HTraceWSGI))]
    internal class HTraceWSGIEditor : UnityEditor.Editor
    {
        SerializedProperty _globalSettingsTab;
        SerializedProperty _ssLightingTab;
        SerializedProperty _wsgiTab;
        SerializedProperty _reflectionsTab;
        SerializedProperty _lightingTab;
        SerializedProperty _debugTab;

        SerializedProperty _showVoxelParams;
        SerializedProperty _showUpdateOptions;

        private AnimBool AnimBoolGeneralTab;
        private AnimBool AnimBoolWSGITab;
        private AnimBool AnimBoolLightingTab;
        private AnimBool AnimBoolSsLightingTab;
        private AnimBool AnimBoolReflIndirLightingTab;
        private AnimBool AnimBoolDebugTab;
        private AnimBool AnimBoolEMPTY;

        SerializedProperty GeneralSettings;
        SerializedProperty LightingSettings;
        SerializedProperty VoxelizationSettings;
        SerializedProperty ScreenSpaceLightingSettings;
        SerializedProperty ReflectionIndirectLightingSettings;
        SerializedProperty DebugSettings;

        // Debug Tab
        SerializedProperty DebugModeWS;
        SerializedProperty HBuffer;
        SerializedProperty VolumetricDebug;
        SerializedProperty MainCamera;
        SerializedProperty AttachToSceneCamera;



        // General Tab
        SerializedProperty RayCountMode;
        SerializedProperty RayLength;
        SerializedProperty Multibounce;
        SerializedProperty Tracing;

        // Lighting Tab
        SerializedProperty DirectionalLight;
        SerializedProperty ExpandShadowmap;
        SerializedProperty ShadowmapRange;
        SerializedProperty ShadowmapUpdateMode;


        // Voxelization Tab
        SerializedProperty VoxelizationMask;
        SerializedProperty VoxelizationUpdateMode;
        SerializedProperty AttachTo;
        SerializedProperty LodMax;
        SerializedProperty InstancedTerrains;

        SerializedProperty CenterShift;
        SerializedProperty VoxelDensity;
        SerializedProperty VoxelBounds;
        SerializedProperty ClipmapRange;
        SerializedProperty OverrideBoundsHeightEnable;
        SerializedProperty OverrideBoundsHeight;
        SerializedProperty GroundLevelEnable;
        SerializedProperty GroundLevel;

        //Update Options
        SerializedProperty CulledObjectsMask;
        SerializedProperty ExpandCullFov;
        SerializedProperty ExpandCullRadius;
        SerializedProperty DynamicObjectsMask;

        SerializedProperty ExactBounds;
        SerializedProperty ExactResolution;

        // Screen space lighting Tab
        SerializedProperty EvaluateHitLighting;
        SerializedProperty DirectionalOcclusion;
        SerializedProperty OcclusionIntensity;

        private bool _showStatistic;

        private string _isScreenSpaceShadowsDisabledMessage = "Screen Space Shadows must be active for Hit Lighting Evaluation!\nYou can enable it in HDRP Asset.\nProject Settings - Quality - HDRP - Lighting - Shadows - Screen Scape Shadows";
        private string _isDynamicRescaleDisabledMessage = "Dynamic Rescale must be not active for Punctual Light Shadows!\nYou can disable it in HDRP Asset.\nProject Settings - Quality - HDRP - Lighting - Shadows - Punctual Light Shadows - Light Atlas - Dynamic Rescale";

        private void OnEnable()
        {
            PropertiesRelative();

            AnimBoolGeneralTab = new AnimBool(_globalSettingsTab.boolValue);
            AnimBoolGeneralTab.valueChanged.RemoveAllListeners();
            AnimBoolGeneralTab.valueChanged.AddListener(Repaint);

            AnimBoolSsLightingTab = new AnimBool(_ssLightingTab.boolValue);
            AnimBoolSsLightingTab.valueChanged.RemoveAllListeners();
            AnimBoolSsLightingTab.valueChanged.AddListener(Repaint);

            AnimBoolReflIndirLightingTab = new AnimBool(_reflectionsTab.boolValue);
            AnimBoolReflIndirLightingTab.valueChanged.RemoveAllListeners();
            AnimBoolReflIndirLightingTab.valueChanged.AddListener(Repaint);

            AnimBoolWSGITab = new AnimBool(_wsgiTab.boolValue);
            AnimBoolWSGITab.valueChanged.RemoveAllListeners();
            AnimBoolWSGITab.valueChanged.AddListener(Repaint);

            AnimBoolLightingTab = new AnimBool(_lightingTab.boolValue);
            AnimBoolLightingTab.valueChanged.RemoveAllListeners();
            AnimBoolLightingTab.valueChanged.AddListener(Repaint);

            AnimBoolDebugTab = new AnimBool(_debugTab.boolValue);
            AnimBoolDebugTab.valueChanged.RemoveAllListeners();
            AnimBoolDebugTab.valueChanged.AddListener(Repaint);

            AnimBoolEMPTY = new AnimBool(false);
        }

        //https://docs.unity3d.com/ScriptReference/IMGUI.Controls.PrimitiveBoundsHandle.DrawHandle.html
        private readonly BoxBoundsHandle _boundsHandle = new BoxBoundsHandle();

        protected virtual void OnSceneGUI()
        {
            if (VoxelizationRuntimeData.VoxelCamera == null) //when disabled HTrace component it's null
                return;

            if (VoxelsService.Instance?.BoundsGizmo == null) // it may not created yet
                return;

            Bounds voxelCameraBounds = VoxelsService.Instance.GetVoxelCameraBounds();
            _boundsHandle.center = voxelCameraBounds.center;
            _boundsHandle.size = voxelCameraBounds.size;

            _boundsHandle.handleColor = Color.clear;
            // draw the handle
            _boundsHandle.DrawHandle();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            UpdateStandartStyles();
            // base.OnInspectorGUI();
            //return;

            AnimBoolEMPTY = new AnimBool(false);
            Scripts.HTraceWSGI trgt = (Scripts.HTraceWSGI)target;

            Color standartBackgroundColor = GUI.backgroundColor;
            Color standartColor = GUI.color;

            using (new HEditorUtils.FoldoutScope(AnimBoolGeneralTab, out var shouldDraw, HEditorStyles.GlobalSettingsContent.text))
            {
                _globalSettingsTab.boolValue = shouldDraw;
                if (shouldDraw)
                {
                    EditorGUILayout.PropertyField(DebugModeWS, HEditorStyles.DebugModeContent);
                    if ((DebugModeWS)DebugModeWS.enumValueIndex == Scripts.Globals.DebugModeWS.MainBuffers)
                    {
                        EditorGUILayout.PropertyField(HBuffer, HEditorStyles.HBuffer);
                    }

                    if ((DebugModeWS)DebugModeWS.enumValueIndex == Scripts.Globals.DebugModeWS.LightClusterHeatmap || (DebugModeWS)DebugModeWS.enumValueIndex == Scripts.Globals.DebugModeWS.LightClusterColor)
                        EditorGUILayout.PropertyField(VolumetricDebug, HEditorStyles.VolumetricDebug);

                    EditorGUILayout.Space(5f);

                    EditorGUILayout.PropertyField(RayCountMode, HEditorStyles.RayCountModeContent);
                    RayLength.intValue = EditorGUILayout.IntSlider(HEditorStyles.RayLengthContent, RayLength.intValue, 0, 100);
                    EditorGUILayout.PropertyField(Multibounce, HEditorStyles.MultibounceContent);
                }
            }


            using (new HEditorUtils.FoldoutScope(AnimBoolLightingTab, out var shouldDraw, HEditorStyles.Lighting.text))
            {
                _lightingTab.boolValue = shouldDraw;
                if (shouldDraw)
                {
                    EditorGUILayout.PropertyField(DirectionalLight, HEditorStyles.DirectionalLightContent);

                    if (DirectionalLight.objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("Directional Light is not set", MessageType.Error);
                    }

                    if (Tracing.enumValueIndex == (int)Scripts.Globals.TracingMode.SoftwareTracing)
                    {
                        EditorGUILayout.Slider(ExpandShadowmap, 1.0f, 3.0f, HEditorStyles.ExpandShadowmapContent);
                    }

                    if (Tracing.enumValueIndex == (int)Scripts.Globals.TracingMode.HardwareTracing)
                    {
                        EditorGUILayout.Slider(ShadowmapRange, 10f, 500f, HEditorStyles.ShadowmapRange);
                        EditorGUILayout.PropertyField(ShadowmapUpdateMode, HEditorStyles.ShadowmapUpdateMode);
                    }

                    EditorGUILayout.Space(3f);

                }

            }

            if (trgt.NeedToReallocForUI == true)
            {
                GUI.backgroundColor = HEditorStyles.warningBackgroundColor;
                //GUI.color           = HEditorStyles.warningColor;
            }

            using (new HEditorUtils.FoldoutScope(AnimBoolWSGITab, out var shouldDraw, HEditorStyles.VoxelizationContent.text))
            {
                _wsgiTab.boolValue = shouldDraw;

                GUI.backgroundColor = standartBackgroundColor;
                //GUI.color           = standartColor;
                if (shouldDraw)
                {
                    EditorGUILayout.PropertyField(VoxelizationMask, HEditorStyles.VoxelizationMaskContent);
                    EditorGUILayout.PropertyField(VoxelizationUpdateMode, HEditorStyles.VoxelizationUpdateTypeContent);

                    EditorGUILayout.PropertyField(AttachTo, HEditorStyles.AttachToContent);

                    if (AttachTo.objectReferenceValue != null)
                    {
                        if (((Transform)AttachTo.objectReferenceValue).gameObject.GetComponent<Camera>() != null)
                        {
                            EditorGUILayout.Slider(CenterShift, -VoxelBounds.intValue * 0.5f, VoxelBounds.intValue * 0.5f, HEditorStyles.CenterShiftContent);
                            CenterShift.floatValue = Mathf.Clamp(CenterShift.floatValue, -VoxelBounds.intValue * 0.5f, VoxelBounds.intValue * 0.5f);
                        }
                    }

                    if (AttachTo.objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("Set object to follow voxelization camera", MessageType.Error);
                    }
                    LodMax.intValue = EditorGUILayout.IntSlider(HEditorStyles.MaximumLodContent, LodMax.intValue, 0, HConstants.MAX_LOD_LEVEL);

                    EditorGUILayout.Space(3f);

                    if (trgt.NeedToReallocForUI == true)
                    {
                        GUI.backgroundColor = HEditorStyles.warningBackgroundColor;
                        GUI.color = HEditorStyles.warningColor;
                    }

                    _showVoxelParams.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(_showVoxelParams.boolValue, "Parameters");
                    GUI.backgroundColor = standartBackgroundColor;
                    GUI.color = standartColor;

                    if (_showVoxelParams.boolValue)
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.Slider(VoxelDensity, 0.0f, 1.0f, HEditorStyles.VoxelDensityContent);

                        VoxelBounds.intValue = EditorGUILayout.IntSlider(HEditorStyles.VoxelBoundsContent, VoxelBounds.intValue, 5, HConfig.MAX_VOXEL_BOUNDS);

                        ClipmapRange.intValue = EditorGUILayout.IntSlider(HEditorStyles.ClipmapRangeContent, ClipmapRange.intValue, HConfig.MAX_VOXEL_BOUNDS, HConfig.MAX_VOXEL_BOUNDS_CLIPMAP);

                        EditorGUILayout.BeginHorizontal();
                        //EditorGUILayout.PropertyField(OverrideBoundsHeightEnable, HEditorStyles.OverrideBoundsHeightEnableContent);
                        OverrideBoundsHeightEnable.boolValue = EditorGUILayout.ToggleLeft(
                            OverrideBoundsHeightEnable.boolValue == false ? HEditorStyles.OverrideBoundsHeightEnableContent2 : GUIContent.none,
                            OverrideBoundsHeightEnable.boolValue, GUILayout.MaxWidth(OverrideBoundsHeightEnable.boolValue == false ? 160f : 30f));
                        if (OverrideBoundsHeightEnable.boolValue == true)
                        {
                            OverrideBoundsHeight.intValue = VoxelBounds.intValue < OverrideBoundsHeight.intValue ? VoxelBounds.intValue : OverrideBoundsHeight.intValue;
                            OverrideBoundsHeight.intValue = OverrideBoundsHeight.intValue < 1 ? 1 : OverrideBoundsHeight.intValue;
                            OverrideBoundsHeight.intValue = EditorGUILayout.IntSlider(HEditorStyles.OverrideBoundsHeightEnableContent, OverrideBoundsHeight.intValue, 1, VoxelBounds.intValue);
                        }
                        else
                        {
                            OverrideBoundsHeight.intValue = VoxelBounds.intValue;
                        }

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        //EditorGUILayout.PropertyField(GroundLevelEnable, HEditorStyles.GroundLevelEnableContent);
                        GroundLevelEnable.boolValue = EditorGUILayout.ToggleLeft(GroundLevelEnable.boolValue == false ? HEditorStyles.GroundLevelEnableContent2 : GUIContent.none,
                            GroundLevelEnable.boolValue, GUILayout.MaxWidth(GroundLevelEnable.boolValue == false ? 160f : 30f));
                        if (GroundLevelEnable.boolValue == true)
                        {
                            EditorGUILayout.PropertyField(GroundLevel, HEditorStyles.GroundLevelEnableContent);
                        }

                        EditorGUILayout.EndHorizontal();

                        if (trgt.NeedToReallocForUI == true)
                        {
                            GUI.backgroundColor = HEditorStyles.warningBackgroundColor;
                            GUI.color = HEditorStyles.warningColor;
                        }

                        EditorGUILayout.BeginHorizontal();

                        if (GUILayout.Button("Apply Parameters", HEditorStyles.standartButton))
                        {
                            VoxelizationRuntimeData.OnReallocTextures?.Invoke();
                        }

                        GUI.backgroundColor = standartBackgroundColor;
                        GUI.color = standartColor;

                        if (GUILayout.Button(_showStatistic ? "Hide Statistics" : "Show Statistics"))
                        {
                            _showStatistic = !_showStatistic;
                        }

                        EditorGUILayout.EndHorizontal();

                        if (_showStatistic)
                        {
                            EditorGUILayout.Space(10f);

                            Vector3Int voxelResolution = HMath.CalculateVoxelResolution_UI(VoxelBounds.intValue, VoxelDensity.floatValue, OverrideBoundsHeightEnable.boolValue, OverrideBoundsHeight.intValue);
                            //EditorGUILayout.LabelField($"Voxel Resolution:  Width: {(int)voxelResolution.x}   Depth: {(int)voxelResolution.y}   Height: {(int)voxelResolution.z}");
                            Debug.Log("ExactResolution.vector3IntValue = " + ExactResolution.vector3IntValue + " voxelResolution = " + voxelResolution);
                            float voxelSize = HMath.CalculateVoxelSizeInCM_UI(VoxelBounds.intValue, VoxelDensity.floatValue);
                            //EditorGUILayout.LabelField($"Voxel Size:  Color {voxelSize:0.0} cm.   Position {(voxelSize / 2):0.0} cm.");

                            //float texturesSizeInMB = HMath.TexturesSizeInMB_UI(VoxelBounds.intValue, VoxelDensity.floatValue, OverrideBoundsHeightEnable.boolValue, OverrideBoundsHeight.intValue);
                            float texturesSizeInMB = HMath.TexturesSizeInMB_UI(ExactResolution.vector3IntValue, (VoxelizationUpdateMode)VoxelizationUpdateMode.enumValueIndex);
                            //float texturesSizeInMB = HMath.TexturesSizeInMB_UI(voxelResolution, (VoxelizationUpdateMode)VoxelizationUpdateMode.enumValueIndex);
                            //EditorGUILayout.LabelField($"GPU Memory Consumption:  {texturesSizeInMB:0.0} MB", myStyle);

                            GUIStyle myStyle = GUI.skin.GetStyle("HelpBox");
                            myStyle.richText = true;
                            myStyle.fontSize = 12;
                            //Debug.Log("ExactResolution.vector3Value = " + ExactResolution.vector3Value);
                            //Vector3 voxelsSize = new Vector3(ExactBounds.vector3Value.x / voxelResolution.x, ExactBounds.vector3Value.y / voxelResolution.y,
                            //ExactBounds.vector3Value.z / voxelResolution.z) * 100.0f;
                            Vector3 voxelsSize = new Vector3(ExactBounds.vector3Value.x / ExactResolution.vector3IntValue.x, ExactBounds.vector3Value.y / ExactResolution.vector3IntValue.y,
                                ExactBounds.vector3Value.z / ExactResolution.vector3IntValue.z) * 100.0f;
                            EditorGUILayout.HelpBox(
                                $"Voxel Resolution:  Width: {ExactResolution.vector3IntValue.x}   Depth: {ExactResolution.vector3IntValue.y}   Height: {ExactResolution.vector3IntValue.z}\n" +
                                $"Voxel Bounds:  {ExactBounds.vector3Value.x} x {ExactBounds.vector3Value.y} x {ExactBounds.vector3Value.z} m\n" +
                                $"Voxel Size:  Color {voxelsSize.x:0.0} cm.   Position {(voxelsSize.x / 2):0.0} cm.\n" +
                                $"GPU Memory Consumption:  {texturesSizeInMB:0.00} MB",
                                MessageType.None);
                            //EditorGUILayout.HelpBox(
                            //	$"Voxel Resolution:  Width: {ExactResolution.vector3IntValue.x}   Depth: {ExactResolution.vector3IntValue.y}   Height: {ExactResolution.vector3IntValue.z}\n" +
                            //	$"Voxel Bounds:  {ExactBounds.vector3Value.x} x {ExactBounds.vector3Value.y} x {ExactBounds.vector3Value.z} m\n" +
                            //	$"Voxel Size:  Color {voxelsSize.x:0.0} cm.   Position {(voxelsSize.x / 2):0.0} cm.\n" +
                            //	$"GPU Memory Consumption:  {texturesSizeInMB:0.00} MB",
                            //	MessageType.None);
                        }

                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space(5f);
                    }

                    //EditorGUILayout.PropertyField(InstancedTerrains, HEditorStyles.InstancedTerrains);

                    GUI.backgroundColor = standartBackgroundColor;
                    //GUI.color           = standartColor;

                    EditorGUILayout.EndFoldoutHeaderGroup();
                    EditorGUILayout.Space(3f);

                    if ((VoxelizationUpdateMode)VoxelizationUpdateMode.enumValueIndex == Scripts.Globals.VoxelizationUpdateMode.Partial)
                    {

                        _showUpdateOptions.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(_showUpdateOptions.boolValue, "Update Options");

                        if (_showUpdateOptions.boolValue)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(DynamicObjectsMask, HEditorStyles.DynamicObjectsMaskContent);

                            EditorGUI.indentLevel--;
                            EditorGUILayout.Space(5f);
                        }

                        EditorGUILayout.EndFoldoutHeaderGroup();
                    }

                }
            }

            GUI.backgroundColor = standartBackgroundColor;
            //GUI.color           = standartColor;
            using (new HEditorUtils.FoldoutScope(AnimBoolSsLightingTab, out var shouldDraw, HEditorStyles.ScreenSpaceLightingContent.text))
            {
                _ssLightingTab.boolValue = shouldDraw;
                if (shouldDraw)
                {
                    {
                        EditorGUILayout.PropertyField(EvaluateHitLighting, HEditorStyles.EvaluateHitLightingContent);
                    }

                    EditorGUILayout.PropertyField(DirectionalOcclusion, HEditorStyles.DirectionalOcclusionContent);

                }
            }


            using (new HEditorUtils.FoldoutScope(AnimBoolDebugTab, out var shouldDraw, "Debug Settings"/*, toggle: EnableDebug*/))
            {
                _debugTab.boolValue = shouldDraw;
                if (shouldDraw)
                {
                    EditorGUILayout.PropertyField(AttachToSceneCamera, new GUIContent("Follow Scene Camera"));

                }
            }

            HEditorUtils.HorizontalLine(1f);
            EditorGUILayout.Space(3);
            //EditorGUILayout.LabelField("HTrace WSGI Version: 1.3.1", HEditorStyles.VersionStyle);
            HEditorUtils.DrawLinkRow(
                ($"Documentation (v. " + HNames.HTRACE_WSGI_VERSION + ")", () => Application.OpenURL(HNames.HTRACE_WSGI_DOCUMENTATION_LINK)),
                ("Discord", () => Application.OpenURL(HNames.HTRACE_DISCORD_LINK)),
                ("Bug report", () => HBugReporterWindow.ShowWindow())
            );

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateStandartStyles()
        {
            HEditorStyles.foldout.fontStyle = FontStyle.Bold;
        }

        private void PropertiesRelative()
        {
            _globalSettingsTab = serializedObject.FindProperty("_globalSettingsTab");
            _ssLightingTab = serializedObject.FindProperty("_screenSpaceLightingTab");
            _wsgiTab = serializedObject.FindProperty("_wsgiTab");
            _reflectionsTab = serializedObject.FindProperty("_reflectionsTab");
            _lightingTab = serializedObject.FindProperty("_lightingTab");
            _debugTab = serializedObject.FindProperty("_debugTab");

            _showUpdateOptions = serializedObject.FindProperty("_showUpdateOptions");
            _showVoxelParams = serializedObject.FindProperty("_showVoxelParams");

            GeneralSettings = serializedObject.FindProperty("GeneralSettings");
            VoxelizationSettings = serializedObject.FindProperty("VoxelizationSettings");
            LightingSettings = serializedObject.FindProperty("LightingSettings");
            ScreenSpaceLightingSettings = serializedObject.FindProperty("ScreenSpaceLightingSettings");
            ReflectionIndirectLightingSettings = serializedObject.FindProperty("ReflectionIndirectLightingSettings");
            DebugSettings = serializedObject.FindProperty("DebugSettings");

            //Debug Tab
            AttachToSceneCamera = DebugSettings.FindPropertyRelative("AttachToSceneCamera");


            //Global Tab
            DebugModeWS = GeneralSettings.FindPropertyRelative("DebugModeWS");
            HBuffer = GeneralSettings.FindPropertyRelative("HBuffer");
            VolumetricDebug = GeneralSettings.FindPropertyRelative("VolumetricDebug");
            Tracing = GeneralSettings.FindPropertyRelative("TracingMode");
            RayCountMode = GeneralSettings.FindPropertyRelative("_rayCountMode");
            RayLength = GeneralSettings.FindPropertyRelative("_rayLength");
            Multibounce = GeneralSettings.FindPropertyRelative("Multibounce");

            // Lighting data
            DirectionalLight = LightingSettings.FindPropertyRelative("DirectionalLight");
            ExpandShadowmap = LightingSettings.FindPropertyRelative("_expandShadowmap");
            ShadowmapRange = LightingSettings.FindPropertyRelative("_shadowmapRange");
            ShadowmapUpdateMode = LightingSettings.FindPropertyRelative("ShadowmapUpdateMode");

            // Voxel Data
            VoxelizationMask = VoxelizationSettings.FindPropertyRelative("VoxelizationMask");
            VoxelizationUpdateMode = VoxelizationSettings.FindPropertyRelative("VoxelizationUpdateMode");
            AttachTo = VoxelizationSettings.FindPropertyRelative("AttachTo");
            LodMax = VoxelizationSettings.FindPropertyRelative("_lodMax");
            InstancedTerrains = VoxelizationSettings.FindPropertyRelative("InstancedTerrains");

            VoxelDensity = VoxelizationSettings.FindPropertyRelative("_voxelDensity");
            VoxelBounds = VoxelizationSettings.FindPropertyRelative("_voxelBounds");
            ClipmapRange = VoxelizationSettings.FindPropertyRelative("_clipmapRange");
            OverrideBoundsHeightEnable = VoxelizationSettings.FindPropertyRelative("_overrideBoundsHeightEnable");
            OverrideBoundsHeight = VoxelizationSettings.FindPropertyRelative("_overrideBoundsHeight");
            CenterShift = VoxelizationSettings.FindPropertyRelative("CenterShift");
            GroundLevelEnable = VoxelizationSettings.FindPropertyRelative("GroundLevelEnable");
            GroundLevel = VoxelizationSettings.FindPropertyRelative("GroundLevel");

            CulledObjectsMask = VoxelizationSettings.FindPropertyRelative("CulledObjectsMask");
            ExpandCullFov = VoxelizationSettings.FindPropertyRelative("_expandCullFov");
            ExpandCullRadius = VoxelizationSettings.FindPropertyRelative("_expandCullRadius");
            DynamicObjectsMask = VoxelizationSettings.FindPropertyRelative("DynamicObjectsMask");

            ExactBounds = VoxelizationSettings.FindPropertyRelative("ExactData").FindPropertyRelative("Bounds");
            ExactResolution = VoxelizationSettings.FindPropertyRelative("ExactData").FindPropertyRelative("Resolution");

            // Screen Space Lighting Tab
            EvaluateHitLighting = ScreenSpaceLightingSettings.FindPropertyRelative("EvaluateHitLighting");
            DirectionalOcclusion = ScreenSpaceLightingSettings.FindPropertyRelative("DirectionalOcclusion");
            OcclusionIntensity = ScreenSpaceLightingSettings.FindPropertyRelative("_occlusionIntensity");
        }
    }
}
#endif
