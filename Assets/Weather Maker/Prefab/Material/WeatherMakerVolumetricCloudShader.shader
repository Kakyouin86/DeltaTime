//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

Shader "WeatherMaker/WeatherMakerVolumetricCloudShader"
{
	Properties
	{
		_FogNoise("Fog Noise", 2D) = "white" {}
		_WeatherMakerSunDirection("Sun Direction", Vector) = (-1, 0, -1, 0)
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent" "LightMode" = "Vertex" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex fog_full_screen_vertex_shader
			#pragma fragment frag
			
			#include "WeatherMakerFogShader.cginc"
			#define STEPS 30

			inline float noise(float3 x)
			{
				x.x += (_Time.x * 4);
				float3 p = floor(x);
				float3 f = frac(x);
				f = f * f * (3.0 - (2.0 * f));
				float2 uv = (p.xy + float2(37.0, 17.0) * p.z) + f.xy;
				uv += 0.5;
				uv *= 0.00390625;
				float4 uvlod = float4(uv.x, uv.y, 0, 0);
				float2 rg = tex2Dlod(_FogNoise, uvlod).yx;

				return (2 * lerp(rg.x, rg.y, f.z)) - 1.0;
				//return lerp(rg.x, rg.y, f.z);
			}

			inline float cloudMap5(float3 p)
			{
				float3 q = p - float3(0.0, 0.1, 1.0);
				float f;
				f = 0.50000 * noise(q);
				q *= 2.02;
				f += 0.25000 * noise(q);
				q *= 2.03;
				f += 0.12500 * noise(q);
				q *= 2.01;
				f += 0.06250 * noise(q);
				q *= 2.02;
				f += 0.03125 * noise(q);
				//return saturate(1.5 - (p.y - _WorldSpaceCameraPos.y) - 2.0 + 1.75 * f);
				//return saturate(1.5 - p.y - 2.0 + 1.75 * f);
				return saturate(f);
			}

			inline float cloudMap4(float3 p)
			{
				float3 q = p - float3(0.0, 0.1, 1.0);
				float f;
				f = 0.50000 * noise(q);
				q *= 2.02;
				f += 0.25000 * noise(q);
				q *= 2.03;
				f += 0.12500 * noise(q);
				q *= 2.01;
				f += 0.06250 * noise(q);
				//return saturate(1.5 - (p.y - _WorldSpaceCameraPos.y) - 2.0 + 1.75 * f);
				//return saturate(1.5 - p.y - 2.0 + 1.75 * f);
				return saturate(f);
			}

			inline float cloudMap3(float3 p)
			{
				float3 q = p - float3(0.0, 0.1, 1.0);
				float f;
				f = 0.50000 * noise(q);
				q *= 2.02;
				f += 0.25000 * noise(q);
				q *= 2.03;
				f += 0.12500 * noise(q);
				//return saturate(1.5 - (p.y - _WorldSpaceCameraPos.y) - 2.0 + 1.75 * f);
				//return saturate(1.5 - p.y - 2.0 + 1.75 * f);
				return saturate(f);
			}

			inline float cloudMap2(float3 p)
			{
				float3 q = p - float3(0.0, 0.1, 1.0);
				float f;
				f = 0.50000 * noise(q);
				q = q * 2.02;
				f += 0.25000 * noise(q);
				//return saturate(1.5 - (p.y - _WorldSpaceCameraPos.y) - 2.0 + 1.75 * f);
				//return saturate(1.5 - p.y - 2.0 + 1.75 * f);
				return saturate(f);
			}

			inline float4 integrate(float4 sum, float dif, float den, float3 bgcol, float t)
			{
				// lighting
				float3 lin = float3(0.65, 0.7, 0.75) * 1.4 + float3(1.0, 0.6, 0.3) * dif;
				float4 col = float4(lerp(float3(1.0, 0.95, 0.8), float3(0.25, 0.3, 0.35), den), den);
				col.xyz *= lin;
				col.xyz = lerp(col.xyz, bgcol, 1.0 - exp(-0.003 * t * t));

				// front to back blending    
				col.a *= 0.4;
				col.rgb *= col.a;
				return sum + col * (1.0 - sum.a);
			}
			
			inline void rayMarch5(float3 ro, float3 rd, float3 bgcol, inout float4 sum, inout float t)
			{
				for (int i = 0; i < STEPS && sum.a < 1.0; i++)
				{
					float3 pos = ro + t * rd;
					float den = cloudMap5(pos);
					if (den > 0.01)
					{
						float dif = saturate((den - cloudMap5(pos + 0.3 * _WeatherMakerSunDirection)) / 0.6);
						sum = integrate(sum, dif, den, bgcol, t);
					}
					t += max(0.05, 0.02 * t);
				}
			}
			
			inline void rayMarch4(float3 ro, float3 rd, float3 bgcol, inout float4 sum, inout float t)
			{
				for (int i = 0; i < STEPS && sum.a < 1.0; i++)
				{
					float3 pos = ro + t * rd;
					float den = cloudMap4(pos);
					if (den > 0.01)
					{
						float dif = saturate((den - cloudMap4(pos + 0.3 * _WeatherMakerSunDirection)) / 0.6);
						sum = integrate(sum, dif, den, bgcol, t);
					}
					t += max(0.05, 0.02 * t);
				}
			}

			inline void rayMarch3(float3 ro, float3 rd, float3 bgcol, inout float4 sum, inout float t)
			{
				for (int i = 0; i < STEPS && sum.a < 1.0; i++)
				{
					float3 pos = ro + t * rd;
					float den = cloudMap3(pos);
					if (den > 0.01)
					{
						float dif = saturate((den - cloudMap3(pos + 0.3 * _WeatherMakerSunDirection)) / 0.6);
						sum = integrate(sum, dif, den, bgcol, t);
					}
					t += max(0.05, 0.02 * t);
				}
			}
			
			inline void rayMarch2(float3 ro, float3 rd, float3 bgcol, inout float4 sum, inout float t)
			{
				for (int i = 0; i < STEPS && sum.a < 1.0; i++)
				{
					float3 pos = ro + t * rd;
					float den = cloudMap2(pos);
					if (den > 0.01)
					{
						float dif = saturate((den - cloudMap2(pos + 0.3 * _WeatherMakerSunDirection)) / 0.6);
						sum = integrate(sum, dif, den, bgcol, t);
					}
					t += max(0.05, 0.02 * t);
				}
			}
			
			inline float4 rayMarchClouds(float3 ro, float3 rd, float3 bgcol, float t)
			{
				float4 sum = float4(0.0, 0.0, 0.0, 0.0);
				rayMarch5(ro, rd, bgcol, sum, t);
				rayMarch4(ro, rd, bgcol, sum, t);
				rayMarch3(ro, rd, bgcol, sum, t);
				rayMarch2(ro, rd, bgcol, sum, t);
				return saturate(sum);
			}

			inline float4 render(float3 ro, float3 rd, float t)
			{
				// background sky     
				//float sun = clamp(dot(_WeatherMakerSunDirection, rd), 0.0, 1.0);
				//float3 col = float3(0.6, 0.71, 0.75) - rd.y * 0.2 * float3(1.0, 0.5, 1.0) + 0.15 * 0.5;
				//col += 0.2 * float3(1.0, .6, 0.1) * pow(sun, 8.0);
				float3 col = 0;

				// clouds    
				float4 res = rayMarchClouds(ro, rd, col, t);
				col = col * (1.0 - res.w) + res.xyz;

				// sun glare    
				//col += 0.2 * float3(1.0, 0.4, 0.2) * pow(sun, 3.0);

				return float4(col, saturate((col.r + col.g + col.b) * 0.5));
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
				float3 boxMin, boxMax;
				float randomizer = noise(worldPos * 0.05) * 8;
				GetFullScreenBoundingBox2(20, 40, boxMin, boxMax);
				RayBoxIntersect(_WorldSpaceCameraPos + randomizer, rayDir, depth, boxMin, boxMax, depth, distanceToBox);
				if (depth <= 0.0)
				{
					return 0;
				}

				float4 col = render(_WorldSpaceCameraPos, rayDir, 0);
				col.a = col.a * saturate(depth / 20.0);
				return col;
			}

			ENDCG
		}
	}
}