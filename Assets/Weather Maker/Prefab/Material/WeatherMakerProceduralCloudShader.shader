//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

Shader "WeatherMaker/WeatherMakerProceduralCloudShader"
{
	Properties
	{
		_FogColor("Fog Color", Color) = (1,1,1,1)
		_FogNoise("Fog Noise", 2D) = "white" {}
		_FogNoiseScale("Fog Noise Scale", Range(0.0, 1.0)) = 0.02
		_FogNoiseMultiplier("Fog Noise Multiplier", Range(0.01, 4.0)) = 1
		_FogNoiseVelocity("Fog Noise Velocity", Vector) = (0.1, 0.2, 0, 0)
		_FogHeight("Height of clouds", Float) = 1200
		_WeatherMakerSunDirection("Sun Direction", Vector) = (-1, 0, -1, 0)
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent" "LightMode" = "Vertex" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			Cull Back Lighting On ZWrite Off

			CGPROGRAM

			#pragma target 3.0
			#pragma vertex fog_full_screen_vertex_shader
			#pragma fragment frag
			#pragma multi_compile __ PER_PIXEL_LIGHTING

			#include "WeatherMakerFogShader.cginc"
			
			inline float hash(float n)
			{
				return frac(sin(n) * 43758.5453);
			}

			inline float noise(float2 x)
			{
				//float2 p = floor(x);
				//float2 f = frac(x);
				//f = f * f * (3.0 - 2.0 * f);
				//float n = p.x + p.y * 57.0;
				//float res = lerp(lerp(hash(n + 0.0), hash(n + 1.0),f.x), lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y);
				//return res;
				return tex2Dlod(_FogNoise, float4(x * _FogNoiseScale, 0, 0)) * _FogNoiseMultiplier;
			}

			// 5 octaves fbm, no loop
			float fbm(float2 p)
			{
				float f = 0.0;
				f += 0.50000 * noise(p);
				p *= 2.02;
				f += 0.25000 * noise(p);
				p *= 2.03;
				f += 0.12500 * noise(p);
				p *= 2.01;
				f += 0.06250 * noise(p);
				p *= 2.04;
				f += 0.03125 * noise(p);
				return f;
			}
			
			fixed4 frag (fog_full_screen_fragment i) : SV_Target
			{
				float3 rayDir = normalize(i.forwardLine);
				float depth01 = GetDepth01(i.uv);
				float distanceToBox;

				// depth is 0-1 value, which needs to be changed to world space distance
				float3 worldPos = _WorldSpaceCameraPos + (i.forwardLine * depth01);

				// calculate depth exactly in world space
				float depth = distance(worldPos, _WorldSpaceCameraPos);
				float fullDepth = depth;
				float3 boxMin, boxMax;
				GetFullScreenBoundingBox2(_FogHeight, _FogHeight + 1, boxMin, boxMax);
				RayBoxIntersect(_WorldSpaceCameraPos, rayDir, depth, boxMin, boxMax, depth, distanceToBox);
				if (depth <= 0.0)
				{
					return 0;
				}

				worldPos = _WorldSpaceCameraPos + (rayDir * distanceToBox);
				float2 xz = (_FogNoiseVelocity * _Time.x) + (0.1 * worldPos).xz;
				float f = fbm(xz);
				float cover = 0.85;// CLOUD_COVER;
				float sharpness = 0.015;// CLOUD_SHARPNESS;
				float c = f - (1.0 - cover);
				f = 1.0 - (pow(sharpness, c));

				fixed4 color = (f * _FogColor);
				if (weatherMaker_LightPosition[0].w == 0.0)
				{

				}

#if defined(PER_PIXEL_LIGHTING)

				color *= CalculateVertexColorWorldSpace(worldPos, weatherMaker_LightPosition[0].w);

#endif

				return color;
			}

			ENDCG
		}
	}
}
