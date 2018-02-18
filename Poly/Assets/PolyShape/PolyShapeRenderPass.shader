Shader "PolyShape/ColorPass"
{
    //Run this shader on a quad mesh using the output of the PolyShape/StencilPass shader
    //    to color in the PolyShape.

	Properties
	{
        _Color ("In Poly Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Pass
		{
    		Tags { "RenderType"="Opaque" }

            Cull Off
            ZTest Off
            ZWrite Off
            Blend One Zero

            Stencil {
                Ref 1
                Comp Equal
            }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

            float4 _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;//UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
                return _Color;
			}
			ENDCG
		}
	}
}
