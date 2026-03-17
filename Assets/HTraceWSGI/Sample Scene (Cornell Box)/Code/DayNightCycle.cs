using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HTraceWSGI.SampleScene.Code
{
    [ExecuteInEditMode]
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Sun Light Settings")]
        [Tooltip("The sun (directional light) to control. If null, will use Light component on this GameObject")]
        public Light SunLight;

        [Header("Day/Night Cycle Settings")]
        [Tooltip("Duration of a full day cycle in seconds")]
        [Range(1f, 300f)]
        public float DayDuration = 20f;

        [Range(1f, 5f)]
        public float SpeedUpNight = 2f;

        [Tooltip("Current time of day in hours (0 = midnight, 12 = noon, 24 = midnight)")]
        [Range(0f, 24f)]
        public float CurrentTimeOfDay = 6f; // Start at sunrise

        [Space]
        [Tooltip("Should the cycle automatically progress? (Only in Play Mode)")]
        public bool AutoProgress = true;

        [Tooltip("Speed multiplier for time progression")]
        [Range(0.1f, 5f)]
        public float TimeSpeed = 1f;

        [Header("Time Event Settings")]
        [Tooltip("Time when midnight event triggers (in hours)")]
        [Range(0f, 24f)]
        public float MidnightTime = 3f;

        [Tooltip("Time when sunrise event triggers (in hours)")]
        [Range(0f, 24f)]
        public float SunriseTime = 6f;

        [Tooltip("Time when noon event triggers (in hours)")]
        [Range(0f, 24f)]
        public float NoonTime = 15f;

        [Tooltip("Time when sunset event triggers (in hours)")]
        [Range(0f, 24f)]
        public float SunsetTime = 24f;

        [Header("Sun Rotation Settings")]
        [Tooltip("Axis around which the sun rotates (usually X for east-west movement)")]
        public Vector3 RotationAxis = Vector3.right;

        [Tooltip("Starting rotation offset")]
        public Vector3 BaseRotation = new Vector3(16f, 668.47f, -6.23f);

        [Tooltip("How many degrees the sun travels in a full cycle")]
        [Range(90f, 360f)]
        public float RotationRange = 180f; // Half circle (sunrise to sunset)

        [Tooltip("Offset the sun's path (0 = horizon at noon, positive = higher)")]
        [Range(-90f, 90f)]
        public float ElevationOffset = 0f;

        [Header("Light Intensity (Lux)")]
        [Tooltip("Enable automatic light intensity changes")]
        public bool AdjustIntensity = true;

        [Tooltip("Light intensity curve based on time of day (in Lux)")]
        public AnimationCurve IntensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Maximum light intensity in Lux")]
        [Range(0f, 120000f)]
        public float MaxIntensityLux = 120000f; // Typical bright sunlight

        [Header("Light Color & Temperature")]
        [Tooltip("Enable automatic light color changes")]
        public bool AdjustColor = true;

        [Tooltip("Light color gradient based on time of day")]
        public Gradient ColorGradient = new Gradient();

        [Space]
        [Tooltip("Enable automatic color temperature changes")]
        public bool AdjustColorTemperature = true;

        [Tooltip("Color temperature curve based on time of day (in Kelvin)")]
        public AnimationCurve TemperatureCurve = new AnimationCurve(
            new Keyframe(0f, 2000f),    // Midnight - warm/reddish
            new Keyframe(0.25f, 2500f), // Sunrise - warm orange
            new Keyframe(0.5f, 5500f),  // Noon - neutral white
            new Keyframe(0.75f, 2800f), // Sunset - warm
            new Keyframe(1f, 2000f)     // Midnight - warm/reddish
        );

        [Tooltip("Minimum color temperature in Kelvin")]
        [Range(1000f, 4000f)]
        public float MinTemperature = 2000f;

        [Tooltip("Maximum color temperature in Kelvin")]
        [Range(4000f, 20000f)]
        public float MaxTemperature = 6500f;

        [Header("Time Control Buttons")]
        [Space]
        public bool SetSunrise = false;
        public bool SetNoon = false;
        public bool SetSunset = false;
        public bool SetMidnight = false;

        [Header("Events")]
        public UnityEvent OnSunrise;
        public UnityEvent OnNoon;
        public UnityEvent OnSunset;
        public UnityEvent OnMidnight;

        [Header("Debug")]
        [Tooltip("Show debug information")]
        public bool ShowDebug = false;

        // Private variables
        private float previousTimeOfDay;
        private bool hasTriggeredSunrise, hasTriggeredNoon, hasTriggeredSunset, hasTriggeredMidnight;

        void OnEnable()
        {
            Initialize();
        }

        void Start()
        {
            Initialize();
        }

        void Initialize()
        {
            // Get Light component if not assigned
            if (SunLight == null)
            {
                SunLight = GetComponent<Light>();
            }

            if (SunLight == null)
            {
                Debug.LogWarning("DayNightCycle: No Light component found! Please assign SunLight or add Light component to this GameObject.");
                return;
            }

            // Set light to use color temperature if adjusting temperature
            if (AdjustColorTemperature)
            {
                SunLight.useColorTemperature = true;
            }

            // Initialize default gradient if empty
            InitializeDefaultGradient();

            // Set initial state
            previousTimeOfDay = CurrentTimeOfDay;
            UpdateSun();

            if (ShowDebug)
            {
                Debug.Log($"DayNightCycle initialized. Day duration: {DayDuration}s");
            }
        }

        private bool IsNight => CurrentTimeOfDay > SunsetTime || CurrentTimeOfDay < SunriseTime;

        void Update()
        {
            if (SunLight == null) return;

            // Handle button presses
            HandleButtonPresses();

            // Progress time if auto-progress is enabled and in play mode
            if (Application.isPlaying && AutoProgress)
            {
                float speedUpNight = IsNight ? SpeedUpNight : 1f;
                CurrentTimeOfDay += (Time.deltaTime / DayDuration) * TimeSpeed * 24f * speedUpNight;

                // Wrap around at 24.0
                if (CurrentTimeOfDay >= 24f)
                {
                    CurrentTimeOfDay -= 24f;
                    ResetEventFlags();
                }
            }

            // Update sun rotation and properties
            UpdateSun();

            // Check for events (only in play mode)
            if (Application.isPlaying)
            {
                CheckTimeEvents();
            }

            previousTimeOfDay = CurrentTimeOfDay;

            // Mark scene as dirty in editor
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(this);
            }
            #endif
        }

        void HandleButtonPresses()
        {
            if (SetSunrise)
            {
                SetToSunrise();
                SetSunrise = false;
            }

            if (SetNoon)
            {
                SetToNoon();
                SetNoon = false;
            }

            if (SetSunset)
            {
                SetToSunset();
                SetSunset = false;
            }

            if (SetMidnight)
            {
                SetToMidnight();
                SetMidnight = false;
            }
        }

        void UpdateSun()
        {
            if (SunLight == null) return;

            // Convert time of day to normalized value (0-1)
            float normalizedTime = CurrentTimeOfDay / 24f;

            // Calculate sun angle based on time of day
            float sunAngle = (normalizedTime * RotationRange) - (RotationRange * 0.5f) + ElevationOffset;

            // Create rotation based on the specified axis
            Quaternion sunRotation = Quaternion.AngleAxis(sunAngle, RotationAxis.normalized);

            // Apply base rotation and calculated sun rotation
            if (SunLight.transform != null)
            {
                SunLight.transform.rotation = Quaternion.Euler(BaseRotation) * sunRotation;
            }

            // Adjust light intensity in Lux
            if (AdjustIntensity)
            {
                float intensityLux = IntensityCurve.Evaluate(normalizedTime) * MaxIntensityLux;
                SunLight.intensity = intensityLux;
            }

            // Adjust light color
            if (AdjustColor)
            {
                SunLight.color = ColorGradient.Evaluate(normalizedTime);
            }

            // Adjust color temperature
            if (AdjustColorTemperature)
            {
                SunLight.useColorTemperature = true;
                float temperature = TemperatureCurve.Evaluate(normalizedTime);
                temperature = Mathf.Clamp(temperature, MinTemperature, MaxTemperature);
                SunLight.colorTemperature = temperature;
            }
        }

        void CheckTimeEvents()
        {
            // Check each event with configurable timing
            CheckTimeEvent(ref hasTriggeredMidnight, MidnightTime, OnMidnight, "Midnight");
            CheckTimeEvent(ref hasTriggeredSunrise, SunriseTime, OnSunrise, "Sunrise");
            CheckTimeEvent(ref hasTriggeredNoon, NoonTime, OnNoon, "Noon");
            CheckTimeEvent(ref hasTriggeredSunset, SunsetTime, OnSunset, "Sunset");
        }

        void CheckTimeEvent(ref bool hasTriggered, float eventTime, UnityEvent eventToTrigger, string eventName)
        {
            float tolerance = 0.5f; // Half hour tolerance

            // Handle special case for events at 24:00 (which is the same as 0:00)
            float adjustedEventTime = eventTime >= 24f ? 0f : eventTime;
            float adjustedCurrentTime = CurrentTimeOfDay;
            float adjustedPreviousTime = previousTimeOfDay;

            // Check if we crossed the event time
            bool crossedEvent = false;

            if (adjustedEventTime == 0f) // Event at midnight (0:00 or 24:00)
            {
                // Special handling for midnight crossing
                crossedEvent = !hasTriggered &&
                              ((adjustedCurrentTime >= (24f - tolerance) && adjustedPreviousTime < (24f - tolerance)) ||
                               (adjustedCurrentTime <= tolerance && adjustedPreviousTime > (24f - tolerance)));
            }
            else
            {
                // Normal event timing
                crossedEvent = !hasTriggered &&
                              adjustedCurrentTime >= (adjustedEventTime - tolerance) &&
                              adjustedCurrentTime < (adjustedEventTime + tolerance) &&
                              adjustedPreviousTime < (adjustedEventTime - tolerance);
            }

            if (crossedEvent)
            {
                eventToTrigger.Invoke();
                hasTriggered = true;
                if (ShowDebug) Debug.Log($"{eventName} triggered at {GetCurrentTimeString()}!");
            }
        }

        void ResetEventFlags()
        {
            hasTriggeredSunrise = false;
            hasTriggeredNoon = false;
            hasTriggeredSunset = false;
            hasTriggeredMidnight = false;
        }

        void InitializeDefaultGradient()
        {
            if (ColorGradient.colorKeys.Length == 0)
            {
                GradientColorKey[] colorKeys = new GradientColorKey[5];
                colorKeys[0] = new GradientColorKey(new Color(0.2f, 0.2f, 0.4f), 0f);    // Midnight - dark blue
                colorKeys[1] = new GradientColorKey(new Color(1f, 0.6f, 0.3f), 0.25f);   // Sunrise - orange
                colorKeys[2] = new GradientColorKey(new Color(1f, 1f, 0.9f), 0.5f);      // Noon - bright white
                colorKeys[3] = new GradientColorKey(new Color(1f, 0.4f, 0.2f), 0.75f);   // Sunset - red
                colorKeys[4] = new GradientColorKey(new Color(0.2f, 0.2f, 0.4f), 1f);    // Midnight - dark blue

                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1f, 0f);
                alphaKeys[1] = new GradientAlphaKey(1f, 1f);

                ColorGradient.SetKeys(colorKeys, alphaKeys);
            }
        }

        // Public methods for external control
        public void SetTimeOfDay(float hours)
        {
            CurrentTimeOfDay = Mathf.Clamp(hours, 0f, 24f);
            if (CurrentTimeOfDay == 24f) CurrentTimeOfDay = 0f;
            UpdateSun();
        }

        public void SetToSunrise()
        {
            SetTimeOfDay(SunriseTime);
        }

        public void SetToNoon()
        {
            SetTimeOfDay(NoonTime);
        }

        public void SetToSunset()
        {
            SetTimeOfDay(SunsetTime >= 24f ? 0f : SunsetTime);
        }

        public void SetToMidnight()
        {
            SetTimeOfDay(MidnightTime);
        }

        public void PauseTime()
        {
            AutoProgress = false;
        }

        public void ResumeTime()
        {
            AutoProgress = true;
        }

        public string GetCurrentTimeString()
        {
            int hours = Mathf.FloorToInt(CurrentTimeOfDay);
            int minutes = Mathf.FloorToInt((CurrentTimeOfDay - hours) * 60f);
            return $"{hours:00}:{minutes:00}";
        }

        public float GetCurrentTemperature()
        {
            float normalizedTime = CurrentTimeOfDay / 24f;
            return TemperatureCurve.Evaluate(normalizedTime);
        }

        public float GetCurrentIntensityLux()
        {
            float normalizedTime = CurrentTimeOfDay / 24f;
            return IntensityCurve.Evaluate(normalizedTime) * MaxIntensityLux;
        }

        // Debug visualization
        void OnDrawGizmosSelected()
        {
            if (SunLight == null) return;

            // Draw sun path
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position;
            float radius = 10f;
            Quaternion baseRot = Quaternion.Euler(BaseRotation);

            for (int i = 0; i < 36; i++)
            {
                float normalizedTime1 = i / 36f;
                float normalizedTime2 = (i + 1) / 36f;

                float angle1 = (normalizedTime1 * RotationRange) - (RotationRange * 0.5f) + ElevationOffset;
                float angle2 = (normalizedTime2 * RotationRange) - (RotationRange * 0.5f) + ElevationOffset;

                Quaternion sunRotation1 = Quaternion.AngleAxis(angle1, RotationAxis.normalized);
                Quaternion sunRotation2 = Quaternion.AngleAxis(angle2, RotationAxis.normalized);

                Vector3 point1 = center + (baseRot * sunRotation1) * Vector3.forward * radius;
                Vector3 point2 = center + (baseRot * sunRotation2) * Vector3.forward * radius;

                Gizmos.DrawLine(point1, point2);
            }

            // Draw current sun position
            Gizmos.color = Color.red;
            float normalizedTime = CurrentTimeOfDay / 24f;
            float currentAngle = (normalizedTime * RotationRange) - (RotationRange * 0.5f) + ElevationOffset;
            Quaternion currentSunRotation = Quaternion.AngleAxis(currentAngle, RotationAxis.normalized);
            Vector3 currentPos = center + (baseRot * currentSunRotation) * Vector3.forward * radius;
            Gizmos.DrawWireSphere(currentPos, 0.5f);

            // Draw event time markers
            DrawEventMarker(center, radius, MidnightTime, Color.blue, "M");
            DrawEventMarker(center, radius, SunriseTime, Color.yellow, "SR");
            DrawEventMarker(center, radius, NoonTime, Color.white, "N");
            DrawEventMarker(center, radius, SunsetTime, Color.red, "SS");

            // Draw time labels
            Gizmos.color = Color.white;
            Vector3 labelOffset = Vector3.up * 2f;

            #if UNITY_EDITOR
            string timeInfo = $"{GetCurrentTimeString()}\n{GetCurrentIntensityLux():F0} Lux\n{GetCurrentTemperature():F0}K";
            UnityEditor.Handles.Label(currentPos + labelOffset, timeInfo);
            #endif
        }

        void DrawEventMarker(Vector3 center, float radius, float eventTime, Color color, string label)
        {
            Gizmos.color = color;
            float eventNormalized = (eventTime >= 24f ? 0f : eventTime) / 24f;
            float eventAngle = (eventNormalized * RotationRange) - (RotationRange * 0.5f) + ElevationOffset;

            // Apply base rotation to the event marker position
            Quaternion baseRot = Quaternion.Euler(BaseRotation);
            Quaternion eventRotation = Quaternion.AngleAxis(eventAngle, RotationAxis.normalized);
            Vector3 eventPos = center + (baseRot * eventRotation) * Vector3.forward * radius;

            Gizmos.DrawWireCube(eventPos, Vector3.one * 0.5f);

            #if UNITY_EDITOR
            UnityEditor.Handles.Label(eventPos + Vector3.up * 0.5f, label);
            #endif
        }

        // Custom inspector validation
        void OnValidate()
        {
            // Get Light component if not assigned
            if (SunLight == null)
            {
                SunLight = GetComponent<Light>();
            }

            // Clamp current time of day
            CurrentTimeOfDay = Mathf.Clamp(CurrentTimeOfDay, 0f, 24f);
            if (CurrentTimeOfDay == 24f) CurrentTimeOfDay = 0f;

            // Clamp event times
            MidnightTime = Mathf.Clamp(MidnightTime, 0f, 24f);
            SunriseTime = Mathf.Clamp(SunriseTime, 0f, 24f);
            NoonTime = Mathf.Clamp(NoonTime, 0f, 24f);
            SunsetTime = Mathf.Clamp(SunsetTime, 0f, 24f);

            // Clamp temperature values
            MinTemperature = Mathf.Clamp(MinTemperature, 1000f, 4000f);
            MaxTemperature = Mathf.Clamp(MaxTemperature, 4000f, 20000f);
            if (MinTemperature >= MaxTemperature)
            {
                MaxTemperature = MinTemperature + 1000f;
            }

            if (SunLight != null)
            {
                UpdateSun();
            }
        }
    }

    // Custom Editor for better inspector experience
    #if UNITY_EDITOR
    [CustomEditor(typeof(DayNightCycle))]
    public class DayNightCycleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DayNightCycle dayNight = (DayNightCycle)target;

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Time Controls", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"Sunrise ({dayNight.SunriseTime:F0}:00)"))
            {
                dayNight.SetToSunrise();
            }
            if (GUILayout.Button($"Noon ({dayNight.NoonTime:F0}:00)"))
            {
                dayNight.SetToNoon();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"Sunset ({(dayNight.SunsetTime >= 24f ? 0 : dayNight.SunsetTime):F0}:00)"))
            {
                dayNight.SetToSunset();
            }
            if (GUILayout.Button($"Midnight ({dayNight.MidnightTime:F0}:00)"))
            {
                dayNight.SetToMidnight();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Current Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Time: {dayNight.GetCurrentTimeString()}");
            EditorGUILayout.LabelField($"Intensity: {dayNight.GetCurrentIntensityLux():F0} Lux");
            EditorGUILayout.LabelField($"Temperature: {dayNight.GetCurrentTemperature():F0} K");

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
    #endif
}
