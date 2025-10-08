//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;
using System.Collections;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerWindScript : MonoBehaviour
    {
        [Tooltip("Optional camera the wind is being shown in.")]
        public Camera Camera;

        [SingleLine("Wind speed range - set to a maximum of 0 for no random speed.")]
        public RangeOfFloats WindSpeedRange = new RangeOfFloats { Minimum = 50.0f, Maximum = 500.0f };

        [Tooltip("The absolute maximum of the wind speed. This only impacts the wind volume by dividing the current wind speed by this value.")]
        public float AbsoluteMaximumWindSpeed = 500.0f;

        [SingleLine("Wind turbulence range - set to a maximum 0 for no random turbulence.")]
        public RangeOfFloats WindTurbulenceRange = new RangeOfFloats { Minimum = 0.0f, Maximum = 100.0f };

        [SingleLine("Wind pulse magnitude range - set to a maximum of 0 for no random pulse magnitude.")]
        public RangeOfFloats WindPulseMagnitudeRange = new RangeOfFloats { Minimum = 2.0f, Maximum = 4.0f };

        [SingleLine("Wind pulse frequency range - set to a maximum of 0 for no random pulse frequency.")]
        public RangeOfFloats WindPulseFrequencyRange = new RangeOfFloats { Minimum = 0.01f, Maximum = 0.1f };

        [Tooltip("Set the wind direction. Leave as Vector3.zero to randomize the wind direction.")]
        public Vector3 WindDirection;
        private Vector3 lastWindDirection = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        [Tooltip("The maximum velocity for the wind")]
        public Vector3 WindVelocityMaximum;

        [Tooltip("Additional sound volume multiplier for the wind")]
        [Range(0.0f, 2.0f)]
        public float WindSoundMultiplier = 0.5f;

        [SingleLine("How often the wind speed and direction changes (minimum and maximum change interval in seconds). Set to 0 for no change.")]
        public RangeOfFloats WindChangeInterval = new RangeOfFloats { Minimum = 0.0f, Maximum = 30.0f };

        [Tooltip("Whether the wind can blow upwards. Default is false.")]
        public bool AllowBlowUp = false;

        [Tooltip("Whether wind should be enabled.")]
        public bool EnableWind = true;

        /// <summary>
        /// The current wind velocity, not including turbulence and pulsing
        /// </summary>
        public Vector3 CurrentWindVelocity { get; private set; }

        /// <summary>
        /// Wind zone
        /// </summary>
        public WindZone WindZone { get; private set; }

        /// <summary>
        /// Wind audio source
        /// </summary>
        public LoopingAudioSource AudioSourceWind { get; private set; }

        /// <summary>
        /// Allow notification of when the wind velocity changes
        /// </summary>
        public System.Action<Vector3> WindChanged { get; set; }

        private float nextWindTime;

        private void Awake()
        {
            WindZone = GetComponent<WindZone>();
            AudioSourceWind = new LoopingAudioSource(GetComponent<AudioSource>());
        }

        private void UpdateWind()
        {
            if (EnableWind)
            {
                if (WindSpeedRange.Maximum > 0.0f || WindTurbulenceRange.Maximum > 0.0f || WindPulseMagnitudeRange.Maximum > 0.0f ||
                    WindPulseFrequencyRange.Maximum > 0.0f || WindDirection != lastWindDirection)
                {
                    lastWindDirection = WindDirection = WindDirection.normalized;
                    if (Camera != null)
                    {
                        WindZone.transform.position = Camera.transform.position;
                        if (!Camera.orthographic)
                        {
                            WindZone.transform.Translate(0.0f, WindZone.radius, 0.0f);
                        }
                    }
                    if (nextWindTime < Time.time)
                    {
                        if (WindSpeedRange.Maximum > 0.0f)
                        {
                            WindZone.windMain = WindSpeedRange.Random();
                        }
                        if (WindTurbulenceRange.Maximum > 0.0f)
                        {
                            WindZone.windTurbulence = WindTurbulenceRange.Random();
                        }
                        if (WindPulseMagnitudeRange.Maximum > 0.0f)
                        {
                            WindZone.windPulseMagnitude = WindPulseMagnitudeRange.Random();
                        }
                        if (WindPulseFrequencyRange.Maximum > 0.0f)
                        {
                            WindZone.windPulseFrequency = WindPulseFrequencyRange.Random();
                        }
                        if (WindDirection == Vector3.zero)
                        {
                            if (Camera != null && Camera.orthographic)
                            {
                                int val = UnityEngine.Random.Range(0, 2);
                                WindZone.transform.rotation = Quaternion.Euler(0.0f, (val == 0 ? 90.0f : -90.0f), 0.0f);
                            }
                            else
                            {
                                float xAxis = (AllowBlowUp ? UnityEngine.Random.Range(-30.0f, 30.0f) : 0.0f);
                                WindZone.transform.rotation = Quaternion.Euler(xAxis, UnityEngine.Random.Range(0.0f, 360.0f), 0.0f);
                            }
                        }
                        else if (Camera.orthographic)
                        {
                            WindZone.transform.right = new Vector3(WindDirection.x, WindDirection.y, 0.0f);
                        }
                        else
                        {
                            WindZone.transform.forward = WindDirection;
                        }
                        nextWindTime = Time.time + WindChangeInterval.Random();
                    }
                }
                AudioSourceWind.Play((WindZone.windMain / AbsoluteMaximumWindSpeed) * WindSoundMultiplier);
                Vector3 newVelocity = WindDirection * WindZone.windMain;
                if (newVelocity != CurrentWindVelocity)
                {
                    CurrentWindVelocity = newVelocity;
                    if (WindChanged != null)
                    {
                        WindChanged(newVelocity);
                    }
                }
            }
            else
            {
                AudioSourceWind.Stop();
                WindZone.windMain = WindZone.windTurbulence = WindZone.windPulseFrequency = WindZone.windPulseMagnitude = 0.0f;
                CurrentWindVelocity = Vector3.zero;
            }
            AudioSourceWind.Update();
        }

        private void Update()
        {
            UpdateWind();
        }
    }
}