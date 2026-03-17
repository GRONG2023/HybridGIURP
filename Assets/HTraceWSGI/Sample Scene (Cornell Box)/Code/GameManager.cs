using HTraceWSGI.SampleScene.Code;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public DayNightCycle DayNightCycle;
    public Volume Volume;
    public GameObject InstancingExample;
    public Light[] Lights;

    [Space]
    public float MaxLightIntensity = 10000;
    public float MinExposure = -2f; // URP postExposure是EV值，通常范围-5~5
    public float MaxExposure = 2f;

    [Header("UI Settings")]
    [SerializeField] private bool showInstancingButton = true;
    [SerializeField] private bool defaultButtonStyle = true;
    [SerializeField] private GUIStyle buttonStyle;

    public System.Action OnInstancingEnabled;
    public System.Action OnInstancingDisabled;

    private bool _instancingEnabledButton = false;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        DayNightCycle.AutoProgress = true;
        DayNightCycle.CurrentTimeOfDay = 9;
        StartCoroutine(ChangeVolumeProfile(true, 0.1f));
    }

    /// <summary>
    /// 设置曝光效果
    /// </summary>
    public void SetVolumeProfile(bool isDay)
    {
        StartCoroutine(ChangeVolumeProfile(isDay, 2));
    }

    private IEnumerator ChangeVolumeProfile(bool isDay, float timeInSeconds)
    {
        // 在URP中用ColorAdjustments来控制曝光
        bool hasExposure = Volume.sharedProfile.TryGet<ColorAdjustments>(out var colorAdjustments);
        if (!hasExposure) yield break;

        colorAdjustments.postExposure.overrideState = true;

        float fromExposure = colorAdjustments.postExposure.value;
        float toExposure = isDay ? MaxExposure : MinExposure;

        float elapsed = 0f;
        while (elapsed < timeInSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / timeInSeconds);
            colorAdjustments.postExposure.value = Mathf.Lerp(fromExposure, toExposure, t);
            yield return null;
        }

        colorAdjustments.postExposure.value = toExposure;
    }

    public void IsEnableLights(bool isActive)
    {
        StartCoroutine(ChangeLightIntensity(isActive, 1));
    }

    private IEnumerator ChangeLightIntensity(bool isActive, float timeInSeconds)
    {
        if (Lights == null || Lights.Length == 0) yield break;

        float from = Lights[0].intensity;
        float to = isActive ? MaxLightIntensity : 0f;

        foreach (var lightElem in Lights)
        {
            lightElem.gameObject.SetActive(true);
            lightElem.intensity = from;
        }

        float elapsed = 0f;
        while (elapsed < timeInSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / timeInSeconds);
            foreach (var lightElem in Lights)
            {
                lightElem.intensity = Mathf.Lerp(from, to, t);
            }
            yield return null;
        }

        if (!isActive)
        {
            foreach (var lightElem in Lights)
            {
                lightElem.gameObject.SetActive(false);
            }
        }
    }

    // --- Day/Night/Auto dropdown ---
    private int _selectedModeIndex = 2; // 0 = Day, 1 = Night, 2 = Auto
    private int _prevSelectedModeIndex = 2;
    private readonly string[] _modes = { "Day", "Night", "Auto" };

    private void OnGUI()
    {
        if (!showInstancingButton) return;

        if (buttonStyle == null || defaultButtonStyle)
        {
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                padding = new RectOffset(10, 10, 5, 5)
            };
        }

        float buttonWidth = 300f;
        float buttonHeight = 40f;
        float margin = 10f;

        // Instancing toggle button
        Rect buttonRect = new Rect(
            margin,
            Screen.height - buttonHeight - margin,
            buttonWidth,
            buttonHeight
        );

        string buttonText = _instancingEnabledButton ? "Disable Instancing Example" : "Enable Instancing Example";
        if (GUI.Button(buttonRect, buttonText, buttonStyle))
        {
            ToggleInstancing();
        }

        // Day/Night/Auto dropdown
        Rect dropdownRect = new Rect(
            margin,
            Screen.height - (buttonHeight * 2) - margin * 2,
            buttonWidth,
            buttonHeight
        );

        _prevSelectedModeIndex = _selectedModeIndex;
        _selectedModeIndex = GUI.SelectionGrid(dropdownRect, _selectedModeIndex, _modes, 3, buttonStyle);

        if (_selectedModeIndex != _prevSelectedModeIndex)
        {
            StopAllCoroutines();

            switch (_selectedModeIndex)
            {
                case 0: // Day
                    DayNightCycle.AutoProgress = false;
                    DayNightCycle.CurrentTimeOfDay = 10;
                    StartCoroutine(ChangeVolumeProfile(true, 0.25f));
                    StartCoroutine(ChangeLightIntensity(false, 0.5f));
                    break;

                case 1: // Night
                    DayNightCycle.AutoProgress = false;
                    DayNightCycle.CurrentTimeOfDay = 0;
                    StartCoroutine(ChangeVolumeProfile(false, 0.25f));
                    StartCoroutine(ChangeLightIntensity(true, 0.5f));
                    break;

                case 2: // Auto
                    DayNightCycle.AutoProgress = true;

                    if (_prevSelectedModeIndex == 0)
                    {
                        StartCoroutine(ChangeVolumeProfile(false, 0.5f));
                        StartCoroutine(ChangeLightIntensity(true, 0.5f));
                    }
                    else if (_prevSelectedModeIndex == 1)
                    {
                        StartCoroutine(ChangeVolumeProfile(true, 0.5f));
                        StartCoroutine(ChangeLightIntensity(false, 0.5f));
                    }
                    break;
            }
        }
    }

    // --- Instancing logic ---
    public void ToggleInstancing()
    {
        _instancingEnabledButton = !_instancingEnabledButton;

        if (_instancingEnabledButton)
            EnableInstancing();
        else
            DisableInstancing();
    }

    public void EnableInstancing()
    {
        _instancingEnabledButton = true;
        OnInstancingEnabled?.Invoke();
        HandleInstancingEnabled();
    }

    public void DisableInstancing()
    {
        _instancingEnabledButton = false;
        OnInstancingDisabled?.Invoke();
        HandleInstancingDisabled();
    }

    private void HandleInstancingEnabled()
    {
        InstancingExample.SetActive(true);
    }

    private void HandleInstancingDisabled()
    {
        InstancingExample.SetActive(false);
    }

    public void ShowInstancingButton(bool show)
    {
        showInstancingButton = show;
    }

    public bool IsInstancingEnabled()
    {
        return _instancingEnabledButton;
    }

    public void SetButtonStyle(GUIStyle newStyle)
    {
        buttonStyle = newStyle;
    }

    private void OnEnable()
    {
        OnInstancingEnabled += HandleCustomInstancingLogic;
        OnInstancingDisabled += HandleCustomInstancingLogic;
    }

    private void OnDisable()
    {
        OnInstancingEnabled -= HandleCustomInstancingLogic;
        OnInstancingDisabled -= HandleCustomInstancingLogic;
    }

    private void HandleCustomInstancingLogic() { }
}