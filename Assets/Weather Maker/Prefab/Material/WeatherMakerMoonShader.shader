Shader "WeatherMaker/WeatherMakerMoonShader"
{
	Properties
	{
		_MainTex("Diffuse Texture", 2D) = "white" {}
		_MoonTintColor( "Color Tint", Color) = (1.0, 1.0, 1.0, 1.0)
		_WeatherMakerSunColor("Light Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_WeatherMakerSunDirection("Light Direction", Vector) = (0.0, 0.0, -1.0, 0.0)
		_WeatherMakerNightMultiplier("Night Multiplier", Range(0.0, 1.0)) = 0
	}
	SubShader
	{
		CGINCLUDE

		#include "WeatherMakerFogShader.cginc"

		sampler2D _GrabTexture;
		uniform float4 _MoonTintColor;

		struct vertexInput
		{
			float4 vertex: POSITION;
			float3 normal: NORMAL;
			float4 texcoord: TEXCOORD0;
		};

		struct vertexOutput
		{
			float4 pos: SV_POSITION;
			float3 normalWorld: NORMAL;
			float2 tex: TEXCOORD0;
			float4 grabPos: TEXCOORD2;
		};

		ENDCG

		GrabPass{}

		Pass
		{
			Tags{ "Queue" = "Background+20" "RenderType" = "Transparent" "IgnoreProjector" = "True" "LightMode" = "Vertex" }
			Cull Back
			Blend Off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;
				o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.tex = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.grabPos = ComputeGrabScreenPos(o.pos);

				return o;
			}

			fixed4 frag(vertexOutput i): COLOR
			{
				fixed4 bgColor = tex2Dproj(_GrabTexture, i.grabPos);
				float4 tex = tex2D(_MainTex, i.tex.xy);
				float lightNormal = saturate(dot(i.normalWorld, _WeatherMakerSunDirection));
				float3 lightFinal = _WeatherMakerSunColor * lightNormal;
				float alpha = max(_WeatherMakerNightMultiplier, max(lightFinal.r, max(lightFinal.g, lightFinal.b))) * (_WeatherMakerNightMultiplier + 0.5) * 0.6667;
				fixed4 moonColor = fixed4(tex.xyz * lightFinal * _MoonTintColor.xyz, _WeatherMakerNightMultiplier);

				return lerp(moonColor, bgColor, max(bgColor.a, 1.0 - alpha));
			}
			
			ENDCG
		}
		/*
		GrabPass{}

		Pass
		{
			Tags{ "Queue" = "Background+20" "RenderType" = "Transparent" "IgnoreProjector" = "True" "LightMode" = "Vertex" }
			Cull Back
			Blend Off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;
				o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				o.pos = UnityObjectToClipPos(v.vertex * 2);
				o.tex = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.grabPos = ComputeGrabScreenPos(o.pos);

				return o;
			}

			fixed4 frag(vertexOutput i) : COLOR
			{
				fixed4 bgColor = tex2Dproj(_GrabTexture, i.grabPos);
				fixed4 moonColor = fixed4(_MoonTintColor.rgb * 0.1, 0);

				return lerp(moonColor + bgColor, bgColor, max(bgColor.a, _WeatherMakerNightMultiplier * 0.1));
			}
			ENDCG
		}
		*/
	}
	FallBack "Transparent/Diffuse"
}