﻿// Simple "just colors" shader that's used for built-in debug visualizations,
// in the editor etc. Just outputs _Color; and blend/Z/cull/bias
// controlled by material parameters.

Shader "Unlit/ScreenColorTex"
{ 
	Properties
	{
        _MainTex("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)

		_SrcBlend ("SrcBlend", Int) = 5.0 // SrcAlpha
		_DstBlend ("DstBlend", Int) = 10.0 // OneMinusSrcAlpha
		_ZWrite ("ZWrite", Int) = 0.0 // Off
		_ZTest ("ZTest", Int) = 0.0 // Always
		_Cull ("Cull", Int) = 0.0 // Off
		_ZBias ("ZBias", Float) = 0.0
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Pass
		{
			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]
			ZTest [_ZTest]
			Cull [_Cull]
			Offset [_ZBias], [_ZBias]

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
			};
			struct v2f {
				float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
			};

			float4 _Color;
            sampler2D _MainTex;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_M, v.vertex);
                o.uv = v.uv;
				return o;
			}
			fixed4 frag (v2f i) : SV_Target
			{
				return _Color * tex2D(_MainTex, i.uv);
			}

			ENDCG  
		}  
	}
}