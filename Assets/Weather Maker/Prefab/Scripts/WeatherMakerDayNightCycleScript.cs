//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System;
using System.Collections.Generic;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerDayNightCycleScript : MonoBehaviour
    {
        #region Classes

        public class SunInfo
        {
            /// <summary>
            /// Calculation parameter, the date/time on the observer planet
            /// </summary>
            public DateTime DateTime;

            /// <summary>
            /// Calculation parameter, latitudeof observer planet in degrees
            /// </summary>
            public double Latitude;

            /// <summary>
            /// Calculation parameter, longitude of observer planet in degrees
            /// </summary>
            public double Longitude;

            /// <summary>
            /// Calculation parameter, axis tilt of observer planet in degrees
            /// </summary>
            public double AxisTilt;

            /// <summary>
            /// Position (unit vector) of the sun in the sky
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Normal (unit vector) of the sun in the sky pointing to 0,0,0 (negation of Position)
            /// </summary>
            public Vector3 Normal;

            // the rest of these are stored in case needed and are best understood by a Google or Bing search
            public double JulianDays;
            public double Declination;
            public double RightAscension;
            public double Azimuth;
            public double Altitude;
            public double SolarMeanAnomaly;
            public double EclipticLongitude;
            public double SiderealTime;
        }

        public class MoonInfo
        {
            /// <summary>
            /// The sun data used to calculate the moon info
            /// </summary>
            public SunInfo SunData;

            /// <summary>
            /// Position (unit vector) of the moon in the sky
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Normal (unit vector) of the moon in the sky pointing to 0,0,0 (negation of Position)
            /// </summary>
            public Vector3 Normal;

            /// <summary>
            /// Distance in kilometers
            /// </summary>
            public double Distance;

            /// <summary>
            /// Moon illumination phase
            /// </summary>
            public double Phase;

            /// <summary>
            /// Moon illumination angle
            /// </summary>
            public double Angle;

            /// <summary>
            /// Moon illumination fraction
            /// </summary>
            public double Fraction;

            // the rest of these are stored in case needed and are best understood by a Google or Bing search
            public double Azimuth;
            public double Altitude;
            public double RightAscension;
            public double Declination;
            public double LunarMeanAnomaly;
            public double EclipticLongitude;
            public double SiderealTime;
            public double ParallacticAngle;
        }

        #endregion Classes

        #region Public fields

        [Header("Display")]
        [Tooltip("The camera the day/night cycle is running in.")]
        public Camera Camera;

        [Tooltip("Sky sphere script, or null if no sky sphere")]
        public WeatherMakerSkySphereScript SkySphereScript;

        [Tooltip("The sun light to use for the day night cycle.")]
        public Light Sun;

        [Header("Day/Night Cycle")]
        [Range(-100000, 100000.0f)]
        [Tooltip("The day speed of the cycle. Set to 0 to freeze the cycle and manually control it. At a speed of 1, the cycle is in real-time. " +
            "A speed of 100 is 100 times faster than normal. Negative numbers run the cycle backwards.")]
        public float Speed = 10.0f;

        [Range(-100000, 100000.0f)]
        [Tooltip("The night speed of the cycle. Set to 0 to freeze the cycle and manually control it. At a speed of 1, the cycle is in real-time. " +
            "A speed of 100 is 100 times faster than normal. Negative numbers run the cycle backwards.")]
        public float NightSpeed = 10.0f;

        [Tooltip("The current time of day in seconds (local time).")]
        public float TimeOfDay = SecondsPerDay * 0.5f; // high noon default time of day

        [Header("Date")]
        [Tooltip("The year for simulating the sun position - this is not changed automatically and must be set and updated by you. " +
            "The calculation is only correct for dates in the range March 1 1900 to February 28 2100.")]
        public int Year = 2000;

        [Tooltip("The month for simulating the sun position - this is not changed automatically and must be set and updated by you.")]
        public int Month = 9;

        [Tooltip("The day for simulating the sun position - this is not changed automatically and must be set and updated by you.")]
        public int Day = 21;

        [Tooltip("Offset for the time zone of the lat / lon in seconds. You must calculate this based on the lat/lon you are providing and the year/month/day.")]
        public int TimeZoneOffsetSeconds = 21600;

        [Header("Location")]
        [Range(-90.0f, 90.0f)]
        [Tooltip("The latitude in degrees on the planet that the camera is at - 90 (north pole) to -90 (south pole)")]
        public double Latitude = 40.7608; // salt lake city latitude

        [Range(-180.0f, 180.0f)]
        [Tooltip("The longitude in degrees on the planet that the camera is at. -180 to 180.")]
        public double Longitude = -111.8910; // salt lake city longitude

        [Tooltip("The amount of degrees your planet is tilted - Earth is about 23.439f")]
        public float AxisTilt = 23.439f;

        [Range(0.0f, 360.0f)]
        [Tooltip("Rotate the sun this many degrees around the y axis. Useful if you want something besides an East/West cycle")]
        public float RotateYDegrees = 0.0f;

        [Header("Fade")]
        [Tooltip("Begin fading out the sun when it's dot product vs. the down vector becomes less than or equal to this value.")]
        public float SunDotFadeThreshold = -0.3f;

        [Tooltip("Disable the sun when it's dot product vs. the down vector becomes less than or equal to this value.")]
        public float SunDotDisableThreshold = -0.4f;

        [Tooltip("The Base sun intensity")]
        [Range(0.0f, 3.0f)]
        public float BaseSunIntensity = 1.0f;

        [Tooltip("The base sun shadow strength")]
        [Range(0.0f, 1.0f)]
        public float BaseSunShadowStrength = 0.8f;

        /// <summary>
        /// Sun intensity multipliers - all are applied to the final sun value
        /// </summary>
        [NonSerialized]
        public readonly Dictionary<string, float> SunIntensityMultipliers = new Dictionary<string, float>();

        /// <summary>
        /// Sun shadow intensity multipliers - all are applied to the final sun shadow strength value
        /// </summary>
        [NonSerialized]
        public readonly Dictionary<string, float> SunShadowIntensityMultipliers = new Dictionary<string, float>();

        /// <summary>
        /// Current sun info
        /// </summary>
        public readonly SunInfo SunData = new SunInfo();

        /// <summary>
        /// Current moon info
        /// </summary>
        public readonly MoonInfo MoonData = new MoonInfo();

        /// <summary>
        /// Number of seconds per day
        /// </summary>
        public const float SecondsPerDay = 86400.0f;

        /// <summary>
        /// Time of day at high noon
        /// </summary>
        public const float HighNoonTimeOfDay = SecondsPerDay * 0.5f;

        /// <summary>
        /// Number of seconds in one degree
        /// </summary>
        public const float SecondsForOneDegree = SecondsPerDay / 360.0f;

        #endregion Public fields

        public static void ConvertAzimuthAtltitudeToUnitVector(double azimuth, double altitude, ref Vector3 v)
        {
            v.y = (float)Math.Sin(altitude);
            float hyp = (float)Math.Cos(altitude);
            v.z = hyp * (float)Math.Cos(azimuth);
            v.x = hyp * (float)Math.Sin(azimuth);
        }

        /// <summary>
        /// Calculate the position of the sun
        /// </summary>
        /// <param name="sunInfo">Calculates and receives sun info, including position, etc.</param>
        public static void CalculateSunPosition(SunInfo sunInfo)
        {
            // dateTime should already be UTC format
            double d = (sunInfo.DateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds / dayMs) + jDiff;
            double e = degreesToRadians * sunInfo.AxisTilt; // obliquity of the Earth
            double m = SolarMeanAnomaly(d);
            double l = EclipticLongitude(m);
            double dec = Declination(e, l, 0);
            double ra = RightAscension(e, l, 0);
            double lw = -degreesToRadians * sunInfo.Longitude;
            double phi = degreesToRadians * sunInfo.Latitude;
            double h = SiderealTime(d, lw) - ra;
            double azimuth = Azimuth(h, phi, dec);
            double altitude = Altitude(h, phi, dec);

            ConvertAzimuthAtltitudeToUnitVector(azimuth, altitude, ref sunInfo.Position);
            sunInfo.Normal = -sunInfo.Position;
            sunInfo.JulianDays = d;
            sunInfo.Declination = dec;
            sunInfo.RightAscension = ra;
            sunInfo.Azimuth = azimuth;
            sunInfo.Altitude = altitude;
            sunInfo.SolarMeanAnomaly = m;
            sunInfo.EclipticLongitude = l;
            sunInfo.SiderealTime = h;
        }

        public static void CalculateMoonPosition(SunInfo sunInfo, MoonInfo moonInfo)
        {
            double d = sunInfo.JulianDays;
            double e = degreesToRadians * sunInfo.AxisTilt; // obliquity of the Earth
            double L = degreesToRadians * (218.316 + 13.176396 * d); // ecliptic longitude
            double M = degreesToRadians * (134.963 + 13.064993 * d); // mean anomaly
            double F = degreesToRadians * (93.272 + 13.229350 * d); // mean distance
            double l = L + degreesToRadians * 6.289 * Math.Sin(M); // longitude
            double b = degreesToRadians * 5.128 * Math.Sin(F); // latitude
            double dist = 385001.0 - (20905.0 * Math.Cos(M)); // distance to the moon in km
            double ra = RightAscension(e, l, b);
            double dec = Declination(e, l, b);
            const double sunDistance = 149598000.0; // avg sun distance to Earth
            double phi = Math.Acos(Math.Sin(sunInfo.Declination) * Math.Sin(dec) + Math.Cos(sunInfo.Declination) * Math.Cos(dec) * Math.Cos(sunInfo.RightAscension - ra));
            double inc = Math.Atan2(sunDistance * Math.Sin(phi), dist - sunDistance * Math.Cos(phi));
            double angle = Math.Atan2(Math.Cos(sunInfo.Declination) * Math.Sin(sunInfo.RightAscension - ra), Math.Sin(sunInfo.Declination) * Math.Cos(dec) - Math.Cos(sunInfo.Declination) * Math.Sin(dec) * Math.Cos(sunInfo.RightAscension - ra));
            double fraction = (1.0 + Math.Cos(inc)) * 0.5;
            double phase = 0.5 + (0.5 * inc * Math.Sign(angle) * (1.0 / Math.PI));
            double lw = -degreesToRadians * sunInfo.Longitude;
            phi = degreesToRadians * sunInfo.Latitude;
            double H = SiderealTime(d, lw) - ra;
            double h = Altitude(H, phi, dec);

            // formula 14.1 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
            double pa = Math.Atan2(Math.Sin(H), Math.Tan(phi) * Math.Cos(dec) - Math.Sin(dec) * Math.Cos(H));
            h = h + AstroRefraction(h); // altitude correction for refraction
            double azimuth = Azimuth(H, phi, dec);
            double altitude = h;
            ConvertAzimuthAtltitudeToUnitVector(azimuth, altitude, ref moonInfo.Position);
            moonInfo.Normal = -moonInfo.Position;
            moonInfo.Distance = dist;
            moonInfo.Phase = phase;
            moonInfo.Angle = angle;
            moonInfo.Fraction = fraction;
            moonInfo.Azimuth = azimuth;
            moonInfo.Altitude = altitude;
            moonInfo.RightAscension = ra;
            moonInfo.Declination = dec;
            moonInfo.LunarMeanAnomaly = M;
            moonInfo.EclipticLongitude = L;
            moonInfo.SiderealTime = H;
            moonInfo.ParallacticAngle = pa;
        }

        private const double degreesToRadians = Math.PI / 180.0;
        private const double dayMs = 1000.0 * 60.0 * 60.0 * 24.0;
        private const double j1970 = 2440587.5;
        private const double j2000 = 2451545.0;
        private const double jDiff = (j1970 - j2000);

        private float lastTimeOfDay = float.MinValue;

        private static double RightAscension(double e, double l, double b)
        {
            return Math.Atan2(Math.Sin(l) * Math.Cos(e) - Math.Tan(b) * Math.Sin(e), Math.Cos(l));
        }

        private static double Declination(double e, double l, double b)
        {
            return Math.Asin(Math.Sin(b) * Math.Cos(e) + Math.Cos(b) * Math.Sin(e) * Math.Sin(l));
        }

        private static double Azimuth(double h, double phi, double dec)
        {
            return Math.Atan2(Math.Sin(h), Math.Cos(h) * Math.Sin(phi) - Math.Tan(dec) * Math.Cos(phi));
        }

        private static double Altitude(double h, double phi, double dec)
        {
            return Math.Asin(Math.Sin(phi) * Math.Sin(dec) + Math.Cos(phi) * Math.Cos(dec) * Math.Cos(h));
        }

        private static double SiderealTime(double d, double lw)
        {
            return degreesToRadians * (280.16 + 360.9856235 * d) - lw;
        }

        private static double SolarMeanAnomaly(double d)
        {
            return degreesToRadians * (357.5291 + 0.98560028 * d);
        }

        private static double EclipticLongitude(double m)
        {
            double c = degreesToRadians * (1.9148 * Math.Sin(m) + 0.02 * Math.Sin(2.0 * m) + 0.0003 * Math.Sin(3.0 * m)); // equation of center
            double p = degreesToRadians * 102.9372; // perihelion of the Earth
            return m + c + p + Math.PI;
        }

        private static double AstroRefraction(double h)
        {
            // the following formula works for positive altitudes only.
            // if h = -0.08901179 a div/0 would occur.
            h = (h < 0.0 ? 0.0 : h);

            // formula 16.4 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
            // 1.02 / tan(h + 10.26 / (h + 5.10)) h in degrees, result in arc minutes -> converted to rad:
            return 0.0002967 / Math.Tan(h + 0.00312536 / (h + 0.08901179));
        }

        private static double CorrectAngle(double angleInRadians)
        {
            if (angleInRadians < 0)
            {
                return (2 * Math.PI) + angleInRadians;
            }
            else if (angleInRadians > 2 * Math.PI)
            {
                return angleInRadians - (2 * Math.PI);
            }
            else
            {
                return angleInRadians;
            }
        }

        private void UpdateTimeOfDay()
        {
            if (Speed != 0.0f)
            {
                TimeOfDay += (Speed * Time.deltaTime);
            }
            else if (NightSpeed != 0.0f)
            {
                TimeOfDay += (NightSpeed * Time.deltaTime);
            }

            // handle wrapping of time of day
            if (TimeOfDay < 0.0f)
            {
                TimeOfDay += SecondsPerDay;
            }
            else if (TimeOfDay >= SecondsPerDay)
            {
                TimeOfDay -= SecondsPerDay;
            }
        }

        private void UpdateSunPosition()
        {
            if (lastTimeOfDay != TimeOfDay)
            {
                if (Camera.orthographic)
                {
                    Sun.transform.rotation = Quaternion.AngleAxis(180.0f + ((TimeOfDay / SecondsPerDay) * 360.0f), Vector3.right);
                }
                else
                {
                    // position the sun far out at the edge of the sky sphere using solar calculations

                    // convert local time of day to UTC time of day - quick and dirty calculation
                    double offsetSeconds = TimeZoneOffsetSeconds;// 3600.0 * (Math.Sign(Longitude) * Longitude * 24.0 / 360.0);
                    TimeSpan t = TimeSpan.FromSeconds(TimeOfDay + offsetSeconds);
                    SunData.DateTime = new DateTime(Year, Month, Day, 0, 0, 0, DateTimeKind.Utc) + t; ;
                    SunData.Latitude = Latitude;
                    SunData.Longitude = Longitude;
                    SunData.AxisTilt = AxisTilt;

                    CalculateSunPosition(SunData);

                    // position and scale the sun
                    Sun.transform.position = Camera.transform.position - SunData.Normal;
                    Sun.transform.LookAt(Camera.transform.position, Vector3.up);
                    Sun.transform.Rotate(Vector3.up, RotateYDegrees, Space.World);
                    Sun.transform.position = (Sun.transform.forward * Camera.farClipPlane * -0.8f);

                    float dot = Vector3.Dot(Sun.transform.forward, Vector3.down);
                    if (dot <= SunDotDisableThreshold)
                    {
                        Sun.intensity = Sun.shadowStrength = 0.0f;
                    }
                    else if (dot <= SunDotFadeThreshold)
                    {
                        Debug.Assert(SunDotDisableThreshold <= SunDotFadeThreshold, "SunDotDisableThreshold should be less than or equal to SunDotFadeThreshold");
                        float range = (SunDotFadeThreshold - SunDotDisableThreshold);
                        float lerp = Mathf.Lerp(0.0f, 1.0f, (dot - SunDotDisableThreshold) / range);
                        Sun.intensity = BaseSunIntensity * lerp;
                        Sun.shadowStrength = BaseSunShadowStrength;
                    }
                    else
                    {
                        Sun.intensity = BaseSunIntensity;
                        Sun.shadowStrength = BaseSunShadowStrength;
                    }

                    foreach (float multiplier in SunIntensityMultipliers.Values)
                    {
                        Sun.intensity *= multiplier;
                    }
                    foreach (float multiplier in SunShadowIntensityMultipliers.Values)
                    {
                        Sun.shadowStrength *= multiplier;
                    }
                }
                lastTimeOfDay = TimeOfDay;
            }
        }

        private void DoDayNightCycle()
        {
            UpdateTimeOfDay();
            UpdateSunPosition();
            if (!Camera.orthographic)
            {
                CalculateMoonPosition(SunData, MoonData);
            }
            // Debug.LogFormat("Moon angle: {0}, phase: {1}, fraction: {2}", MoonData.Angle, MoonData.Phase, MoonData.Fraction);
        }

        private void Start()
        {
            DoDayNightCycle();
        }

        private void Update()
        {
            DoDayNightCycle();
        }
    }
}

// resources:
// https://en.wikipedia.org/wiki/Position_of_the_Sun
// http://stackoverflow.com/questions/8708048/position-of-the-sun-given-time-of-day-latitude-and-longitude
// http://www.grasshopper3d.com/forum/topics/solar-calculation-plugin
// http://guideving.blogspot.nl/2010/08/sun-position-in-c.html
// https://github.com/mourner/suncalc
// http://stackoverflow.com/questions/1058342/rough-estimate-of-the-time-offset-from-gmt-from-latitude-longitude
// http://www.stjarnhimlen.se/comp/tutorial.html
// http://www.suncalc.net/#/40.7608,-111.891,12/2000.09.21/12:46
// http://www.suncalc.net/scripts/suncalc.js

// total eclipse:
// 43.7678
// -111.8323
// Maximum eclipse : 	2017/08/21	17:34:18.6	49.5°	133.1°