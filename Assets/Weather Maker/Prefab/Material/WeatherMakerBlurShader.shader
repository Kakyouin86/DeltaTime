//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

// http://rastergrid.com/blog/2010/09/efficient-gaussian-blur-with-linear-sampling/
// _MainTex must be bilinear

Shader "WeatherMaker/WeatherMakerBlurShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "red" {}
	}
	SubShader
	{
		Cull Back ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		CGINCLUDE

		#pragma fragmentoption ARB_precision_hint_fastest

		// default is BLUR17
		#pragma multi_compile __ BLUR7 

		#include "WeatherMakerShader.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;

#if defined(BLUR7)

			float2 offsets : TEXCOORD1;

#else

			float4 offsets : TEXCOORD1;

#endif

		};

		v2f vert (appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = AdjustFullScreenUV(v.uv);

#if defined(BLUR7)

			// take top left 3 and bottom right 3 plus center pixel average
			o.offsets = float2(_MainTex_TexelSize.x * 0.333333, _MainTex_TexelSize.y * 0.333333);

#else

			// (0.4,-1.2) , (-1.2,-0.4) , (1.2,0.4) and (-0.4,1.2).
			o.offsets = float4
			(
				_MainTex_TexelSize.x * 0.4,
				_MainTex_TexelSize.x * 1.2,
				_MainTex_TexelSize.y * 0.4,
				_MainTex_TexelSize.y * 1.2
			);

#endif

			return o;
		}

		ENDCG

		// optimized blur
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, (i.uv));

#if defined(BLUR7)

				// 7 tap approximation with 2 texture lookups
				col += tex2D(_MainTex, (float2(i.uv.x - i.offsets.x, i.uv.y - i.offsets.y)));
				col += tex2D(_MainTex, (float2(i.uv.x + i.offsets.x, i.uv.y + i.offsets.y)));
				col *= 0.333333; // 3 total colors to average

#else

				// 17 tap approximation with 4 texture lookups
				col += tex2D(_MainTex, (float2(i.uv.x + i.offsets.x, i.uv.y - i.offsets.w)));
				col += tex2D(_MainTex, (float2(i.uv.x - i.offsets.y, i.uv.y - i.offsets.z)));
				col += tex2D(_MainTex, (float2(i.uv.x + i.offsets.y, i.uv.y + i.offsets.z)));
				col += tex2D(_MainTex, (float2(i.uv.x - i.offsets.x, i.uv.y + i.offsets.w)));
				col *= 0.2; // 5 total colors to average

#endif

				return col;
            }

            ENDCG
		}
	}
}
