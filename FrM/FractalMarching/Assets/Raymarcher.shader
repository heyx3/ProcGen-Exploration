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
            float3 getNormal(float3 pos)
            {
                float2 c = float2(0.0, 0.01);
                float sample_ = getWorldDistAndData(pos),
                      sampleX = getWorldDistAndData(pos + c.yxx),
                      sampleY = getWorldDistAndData(pos + c.xyx),
                      sampleZ = getWorldDistAndData(pos + c.xxy);
                return normalize(float3(sampleX - sample_,
                                        sampleY - sample_,
                                        sampleZ - sample_));
            }

            bool marchToSurface(inout float3 pos, float3 dir, int nIterations,
                                out float3 data, out float dist)
            {
                for (int i = 0; i < nIterations; ++i)
                {
                    float4 d = getWorldDistAndData(pos);
                    dist = d.x;
                    data = d.yzw;

                    if (dist < 0.01)
                        return true;

                    pos += (dir * dist);
                }

                return false;
            }

            float3 lightSurface(float3 surfaceCol, float3 normal)
            {
                float3 lightDir = normalize(float3(1, -1, 1));

                return surfaceCol * dot(normal, -lightDir);
            }

			fixed4 frag (v2f v) : SV_Target
			{
                float3 ray = normalize(v.worldPos - _WorldSpaceCameraPos);

                float3 marchPos = _WorldSpaceCameraPos;
                float3 data;
                float dist;
                bool hit = marchToSurface(marchPos, ray, _MaxSteps, data, dist);

                if (!hit)
                    return float4(_SkyColor, 1.0);

                float3 normal = getNormal(marchPos),
                       surfaceColor = data,
                       litSurfaceColor = lightSurface(surfaceColor, normal);

                return float4(litSurfaceColor, 1.0);
			}
			ENDCG
		}
	}
}
