using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Manages lights in world space for use in shaders - you do not need to add the directional light to the Lights list, it is done automatically
    /// </summary>
    public class WeatherMakerLightManagerScript : MonoBehaviour
    {
        [Tooltip("Whether to find all lights in the scene automatically if no Lights were added programatically. If false, you must manually add / remove lights using the Lights property. " +
            "To ensure correct behavior, do not change in script, set it once in the inspector and leave it. If this is true, AddLight and RemoveLight do nothing.")]
        public bool AutoFindLights = false;

        [Tooltip("Weather maker script reference - must NOT be null!")]
        public WeatherMakerScript WeatherScript;

        [Tooltip("Spot light quadratic attenuation - default is 0.05")]
        [Range(0.0f, 1.0f)]
        public float SpotLightQuadraticAttenuation = 0.05f;

        [Tooltip("Point light quadratic attenuation - default is 0.2")]
        [Range(0.0f, 1.0f)]
        public float PointLightQuadraticAttenuation = 0.2f;

        /// <summary>
        /// Maximum number of lights to send to the shaders - 8 is the max for now
        /// </summary>
        public const int MaximumLightCount = 8;

        [System.NonSerialized]
        private Vector4[] lightPositions = new Vector4[MaximumLightCount];

        [System.NonSerialized]
        private Vector4[] lightSpotDirections = new Vector4[MaximumLightCount];

        [System.NonSerialized]
        private Vector4[] lightColors = new Vector4[MaximumLightCount];

        [System.NonSerialized]
        private Vector4[] lightAtten = new Vector4[MaximumLightCount];

        /// <summary>
        /// Add lights to this list to use it instead of the global light finder if you have specific lights you want to show up in the shaders.
        /// You should add the directional light at a minimum if you are using this list.
        /// </summary>
        [System.NonSerialized]
        private readonly List<Light> lights = new List<Light>();

        private void SetLightAtIndex(Light l, ref int i)
        {
            if (!l.enabled || l.color.a == 0.0f || l.intensity == 0.0f || l.range <= 0.0f)
            {
                return;
            }

            float rangeSquared;
            switch (l.type)
            {
                case LightType.Directional:
                    lightPositions[i] = -l.transform.forward;
                    lightPositions[i].w = 0;
                    lightSpotDirections[i] = Vector4.zero;
                    lightAtten[i] = new Vector4(-1.0f, 1.0f, 0.0f, 0.0f);
                    lightColors[i] = l.color * l.intensity;
                    break;

                case LightType.Spot:
                    lightPositions[i] = l.transform.position;
                    lightPositions[i].w = 1;
                    lightSpotDirections[i] = -l.transform.forward;
                    rangeSquared = l.range * l.range;
                    lightAtten[i] = new Vector4(Mathf.Cos(l.spotAngle * 0.5f * Mathf.Deg2Rad), 1.0f / Mathf.Cos(l.spotAngle * 0.25f * Mathf.Deg2Rad), SpotLightQuadraticAttenuation / l.range, rangeSquared);
                    lightColors[i] = l.color * l.intensity;
                    break;

                case LightType.Point:
                    lightPositions[i] = l.transform.position;
                    lightPositions[i].w = 1;
                    lightSpotDirections[i] = Vector4.zero;
                    rangeSquared = l.range * l.range;
                    lightAtten[i] = new Vector4(-1.0f, 1.0f, PointLightQuadraticAttenuation / l.range, rangeSquared);
                    lightColors[i] = l.color * l.intensity;
                    break;

                case LightType.Rectangle:
                default:
                    return;
            }
            i++;
        }

        private int LightSorter(Light l1, Light l2)
        {
            // sort disabled or invisible lights to the back
            if (!l1.enabled || l1.intensity == 0.0f || l1.color.a == 0.0f)
            {
                return 1;
            }
            else if (!l2.enabled || l2.intensity == 0.0f || l2.color.a == 0.0f)
            {
                return -1;
            }

            // directional lights always come first and have highest priority
            if (l1.type == LightType.Directional)
            {
                return -1;
            }
            else if (l2.type == LightType.Directional)
            {
                return 1;
            }

            // create total sum of distance, intensity and range to use as sort
            float mag1 = (Vector3.Distance(l1.transform.position, WeatherScript.Camera.transform.position) - l1.range) * l1.intensity;
            float mag2 = (Vector3.Distance(l2.transform.position, WeatherScript.Camera.transform.position) - l2.range) * l2.intensity;
            return mag1.CompareTo(mag2);
        }

        private void SetLightsToShader()
        {
            int lightCount, lightIndex;

            // if no user lights specified, find all the lights in the scene and sort them
            if (AutoFindLights)
            {
                Light[] allLights = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None);
                lights.Clear();
                foreach (Light l in allLights)
                {
                    if (l.enabled)
                    {
                        lights.Add(l);
                    }
                }
            }
            else if (WeatherScript.Sun.enabled)
            {
                if (!lights.Contains(WeatherScript.Sun))
                {
                    lights.Add(WeatherScript.Sun);
                }
            }
            else
            {
                lights.Remove(WeatherScript.Sun);
            }
            lights.Sort(LightSorter);
            for (lightCount = 0, lightIndex = 0; lightIndex < lights.Count && lightCount < MaximumLightCount; lightIndex++)
            {
                SetLightAtIndex(lights[lightIndex], ref lightCount);
            }
            Shader.SetGlobalVectorArray("weatherMaker_LightPosition", lightPositions);
            Shader.SetGlobalVectorArray("weatherMaker_LightSpotDirection", lightSpotDirections);
            Shader.SetGlobalVectorArray("weatherMaker_LightAtten", lightAtten);
            Shader.SetGlobalVectorArray("weatherMaker_LightColor", lightColors);
            Shader.SetGlobalInt("weatherMaker_LightCount", lightCount);

            if (AutoFindLights)
            {
                lights.Clear();
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        private void LateUpdate()
        {
            SetLightsToShader();
        }

        /// <summary>
        /// Add a light, unless AutoFindLights is true
        /// </summary>
        /// <param name="l">Light to add</param>
        /// <returns>True if light added, false if not</returns>
        public bool AddLight(Light l)
        {
            if (!AutoFindLights)
            {
                lights.Add(l);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove a light, unless AutoFindLights is true
        /// </summary>
        /// <param name="l"></param>
        /// <returns>True if light removed, false if not</returns>
        public bool RemoveLight(Light l)
        {
            if (!AutoFindLights)
            {
                return lights.Remove(l);
            }
            return false;
        }

        /// <summary>
        /// Current set of lights
        /// </summary>
        public IEnumerable<Light> Lights { get { return lights; } }

        public static WeatherMakerLightManagerScript Instance { get; private set; }
    }
}
