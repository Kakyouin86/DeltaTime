//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;
using System.Collections;
using System;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerScript : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Camera the weather should hover over. Defaults to main camera.")]
        public Camera Camera;

        [Tooltip("The sun - must not be null!")]
        public Light Sun;

        [Tooltip("Configuration script. Should be deactivated for release builds.")]
        public WeatherMakerConfigurationScript ConfigurationScript;

        [Range(0.0f, 1.0f)]
        [Tooltip("Change the volume of all weather maker sounds.")]
        public float VolumeModifier = 1.0f;

        [Tooltip("Whether per pixel lighting is enabled - currently precipitation mist is the only material that support this.")]
        public bool EnablePerPixelLighting;

        [Header("Precipitation")]
        [Tooltip("Rain script")]
        public WeatherMakerFallingParticleScript RainScript;

        [Tooltip("Snow script")]
        public WeatherMakerFallingParticleScript SnowScript;

        [Tooltip("Hail script")]
        public WeatherMakerFallingParticleScript HailScript;

        [Tooltip("Sleet script")]
        public WeatherMakerFallingParticleScript SleetScript;

        [Tooltip("Whether the precipitation collides with the world. This can be a performance problem on lower end devices. Please be careful.")]
        public bool CollisionEnabled;

        [Tooltip("Intensity of precipitation (0-1)")]
        [Range(0.0f, 1.0f)]
        public float PrecipitationIntensity;

        [Tooltip("How long in seconds to fully change from one precipitation type to another")]
        [Range(0.0f, 300.0f)]
        public float PrecipitationChangeDuration = 4.0f;

        [Tooltip("The threshold change in intensity that will cause a cross-fade between precipitation changes. Intensity changes smaller than this value happen quickly.")]
        [Range(0.0f, 0.2f)]
        public float PrecipitationChangeThreshold = 0.1f;

        [Header("Lightning")]
        [Tooltip("Lightning script (random bolts)")]
        public WeatherMakerThunderAndLightningScript LightningScript;

        [Tooltip("Lightning bolt script")]
        public WeatherMakerLightningBoltScript LightningBoltScript;

        [Header("Clouds")]
        [Tooltip("Cloud script")]
        public WeatherMakerCloudScript CloudScript;

        [Header("Wind")]
        [Tooltip("Wind script")]
        public WeatherMakerWindScript WindScript;

        [Tooltip("Whether to enable wind - use this to activate or de-activate the WindScript")]
        public bool WindEnabled;

        [Header("Sky Sphere")]
        [Tooltip("Sky sphere script, not used in 2D")]
        public WeatherMakerSkySphereScript SkySphereScript;

        [Header("Day / Night Cycle")]
        [Tooltip("Day night script")]
        public WeatherMakerDayNightCycleScript DayNightScript;

        [Header("Fog")]
        [Tooltip("Fog script, null for none.")]
        public WeatherMakerFullScreenFogScript FogScript;

        [NonSerialized]
        private float lastPrecipitationIntensityChange = -1.0f;

        [NonSerialized]
        private float lastVolumeModifier = -1.0f;

        private void TweenScript(WeatherMakerFallingParticleScript script, float end)
        {
            float duration = (Mathf.Abs(script.Intensity - end) < PrecipitationChangeThreshold ? 0.0f : PrecipitationChangeDuration);
            TweenFactory.Tween("WeatherMakerPrecipitationChange_" + script.gameObject.GetInstanceID(), script.Intensity, end, duration, TweenScaleFunctions.Linear, (t) =>
            {
                // Debug.LogFormat("Tween key: {0}, value: {1}, prog: {2}", t.Key, t.CurrentValue, t.CurrentProgress);
                script.Intensity = t.CurrentValue;
            }, null);
        }

        private void ChangePrecipitation(WeatherMakerFallingParticleScript newPrecipitation)
        {
            if (newPrecipitation != currentPrecipitation && currentPrecipitation != null)
            {
                TweenScript(currentPrecipitation, 0.0f);
                lastPrecipitationIntensityChange = -1.0f;
            }
            currentPrecipitation = newPrecipitation;
        }

        private void UpdateCollision()
        {
            RainScript.CollisionEnabled = CollisionEnabled;
            SnowScript.CollisionEnabled = CollisionEnabled;
            HailScript.CollisionEnabled = CollisionEnabled;
            SleetScript.CollisionEnabled = CollisionEnabled;
        }

        private void UpdateSoundsVolumes()
        {
            LightningScript.VolumeModifier = VolumeModifier;
            RainScript.AudioSourceLight.VolumeModifier = RainScript.AudioSourceMedium.VolumeModifier = RainScript.AudioSourceHeavy.VolumeModifier = VolumeModifier;
            SnowScript.AudioSourceLight.VolumeModifier = SnowScript.AudioSourceMedium.VolumeModifier = SnowScript.AudioSourceHeavy.VolumeModifier = VolumeModifier;
            HailScript.AudioSourceLight.VolumeModifier = HailScript.AudioSourceMedium.VolumeModifier = HailScript.AudioSourceHeavy.VolumeModifier = VolumeModifier;
            SleetScript.AudioSourceLight.VolumeModifier = SleetScript.AudioSourceMedium.VolumeModifier = SleetScript.AudioSourceHeavy.VolumeModifier = VolumeModifier;
            WindScript.AudioSourceWind.VolumeModifier = VolumeModifier;
        }

        private void UpdateWind()
        {
            if (WindScript != null)
            {
                WindScript.gameObject.SetActive(WindEnabled);
                WindScript.EnableWind = WindEnabled;
            }
        }

        private void SetEnableHDR()
        {
            if (Camera.allowHDR)
            {
                Shader.EnableKeyword("HDR");
            }
            else
            {
                Shader.DisableKeyword("HDR");
            }
        }

        private void SetEnablePerPixelLighting()
        {
            if (EnablePerPixelLighting && SystemInfo.graphicsShaderLevel >= 30)
            {
                RainScript.MistMaterial.EnableKeyword("PER_PIXEL_LIGHTING");
                SnowScript.MistMaterial.EnableKeyword("PER_PIXEL_LIGHTING");
                HailScript.MistMaterial.EnableKeyword("PER_PIXEL_LIGHTING");
                SleetScript.MistMaterial.EnableKeyword("PER_PIXEL_LIGHTING");
                if (SkySphereScript != null)
                {
                    SkySphereScript.Material.EnableKeyword("PER_PIXEL_LIGHTING");
                }
            }
            else
            {
                RainScript.MistMaterial.DisableKeyword("PER_PIXEL_LIGHTING");
                SnowScript.MistMaterial.DisableKeyword("PER_PIXEL_LIGHTING");
                HailScript.MistMaterial.DisableKeyword("PER_PIXEL_LIGHTING");
                SleetScript.MistMaterial.DisableKeyword("PER_PIXEL_LIGHTING");
                if (SkySphereScript != null)
                {
                    SkySphereScript.Material.DisableKeyword("PER_PIXEL_LIGHTING");
                }
            }
        }

        private void UpdateShaders()
        {
            SetEnableHDR();
            SetEnablePerPixelLighting();
        }

        private void CheckParticleVariables(WeatherMakerFallingParticleScript script)
        {
            if (script == null || script.AudioSourceLight == null || script.AudioSourceMedium == null || script.AudioSourceHeavy == null)
            {
                Debug.LogErrorFormat("{0} script and/or audio are null", script.gameObject);
            }
        }

        private void CheckVariables()
        {

#if UNITY_EDITOR

            if (Camera == null)
            {
                Debug.LogError("Must assign a camera for weather maker to work properly. Tag a camera as main camera, or manually assign the camera property.");
            }
            CheckParticleVariables(RainScript);
            CheckParticleVariables(HailScript);
            CheckParticleVariables(SnowScript);
            CheckParticleVariables(SleetScript);

#endif

        }

        private void SetupReferences()
        {
            Instance = this;
            Camera = (Camera == null ? (Camera.main == null ? Camera.current : Camera.main) : Camera);
            UpdateCollision();
            RainScript.Camera = Camera;
            SnowScript.Camera = Camera;
            HailScript.Camera = Camera;
            SleetScript.Camera = Camera;
            CloudScript.Camera = Camera;
            DayNightScript.Camera = Camera;
            LightningScript.Camera = Camera;
            if (SkySphereScript != null)
            {
                SkySphereScript.Camera = Camera;
            }
            if (WindScript != null)
            {
                WindScript.Camera = Camera;
            }
            if (FogScript != null)
            {
                FogScript.Camera = Camera;
            }

#if UNITY_EDITOR

            int dirLightCount = 0;
            Light[] lights = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    dirLightCount++;
                }
            }
            if (dirLightCount != 1)
            {
                Debug.LogError("Weather Maker requires exactly one direction light to function properly. This is the Sun object built into the prefab. Please remove all other directional lights.");
            }

#endif

        }

        private void Awake()
        {
            SetupReferences();
        }

        private void Start()
        {
            if (WeatherMakerLightManagerScript.Instance != null)
            {
                // wire up lightning bolt lights to the light manager
                LightningBoltScript.LightAddedCallback = LightAdded;
                LightningBoltScript.LightRemovedCallback = LightRemoved;
            }
        }

        private void Update()
        {
            CheckVariables();
            if (currentPrecipitation != null && PrecipitationIntensity != lastPrecipitationIntensityChange)
            {
                lastPrecipitationIntensityChange = PrecipitationIntensity;
                TweenScript(currentPrecipitation, PrecipitationIntensity);
            }
            if (VolumeModifier != lastVolumeModifier)
            {
                lastVolumeModifier = VolumeModifier;
                UpdateSoundsVolumes();
            }
            UpdateCollision();
            UpdateWind();
            UpdateShaders();
        }

        private void LightAdded(Light l)
        {
            WeatherMakerLightManagerScript.Instance.AddLight(l);
        }

        private void LightRemoved(Light l)
        {
            WeatherMakerLightManagerScript.Instance.RemoveLight(l);
        }

        public WeatherMakerFallingParticleScript CurrentPrecipitation
        {
            get { return currentPrecipitation; }
            set
            {
                if (value != currentPrecipitation)
                {
                    ChangePrecipitation(value);
                }
            }
        }

        private WeatherMakerFallingParticleScript currentPrecipitation;

        /// <summary>
        /// Gets the current time of day in seconds. 86400 seconds per day.
        /// </summary>
        public float TimeOfDay { get { return DayNightScript.TimeOfDay; } set { DayNightScript.TimeOfDay = value; } }

        /// <summary>
        /// Singleton
        /// </summary>
        public static WeatherMakerScript Instance { get; private set; }
    }
}