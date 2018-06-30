// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Raymarcher"
{
    Properties
    {
        _SkyColor("Sky Color", Color) = (0.6, 0.6, 1.0, 1.0)
        _MaxSteps("Max Ray Steps", Int) = 300
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
        Cull Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
                float3 worldPos : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(vertex);
                o.worldPos = mul(unity_ObjectToWorld, vertex).xyz;
				return o;
			}
			
            float3 _SkyColor;
            int _MaxSteps;

            float getSphereDist(float3 pos, float3 center, float radius)
            {
                return distance(center, pos) - radius;
            }
            float4 getWorldDistAndData(float3 pos)
            {
                return float4(getSphereDist(pos, 0.0, 3.0),
                              float3(1.0, 1.0, 0.5));
            }

			fixed4 frag (v2f v) : SV_Target
			{
                float3 ray = normalize(v.worldPos - _WorldSpaceCameraPos);

                float3 marchPos = _WorldSpaceCameraPos;
                for (int i = 0; i < _MaxSteps; ++i)
                {
                    float4 distAndData = getWorldDistAndData(marchPos);
                    if (distAndData.x < 0.01)
                        return float4(distAndData.yzw, 1.0);
                    marchPos += ray * distAndData.x;
                }

				return float4(_SkyColor, 1.0);
			}
			ENDCG
		}
	}
}
