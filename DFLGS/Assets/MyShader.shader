// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "DFLS/Shader"

{

	Properties { }

	SubShader

	{

		Tags { "RenderType"="Opaque" }



		Pass

		{

			CGPROGRAM

			#pragma vertex vert

			#pragma fragment frag

			#include "UnityCG.cginc"



			struct appdata

			{

				float4 vertex : POSITION;

				float3 worldPos : TEXCOORD0;

			};

			struct v2f

			{

				float3 worldPos : TEXCOORD0;

				float4 vertex : SV_POSITION;

			};



			v2f vert(appdata IN)

			{

				v2f o;

				o.vertex = UnityObjectToClipPos(IN.vertex);

				o.worldPos = mul(unity_ObjectToWorld, IN.vertex).xyz;

				return o;

			}





//smin() is a version of min() that gives smooth results when used in distance field functions.

float smin(float d1, float d2, float k)

{

    //Source: http://iquilezles.org/www/articles/smin/smin.htm

    float h = saturate(0.5 + (0.5 * (d1 - d2) / k));

    return lerp(d2, d1, h) - (k * h * (1.0 - h));

}



//Below are the distance functions for basic shapes.

//These are all signed distance functions.

//Source: http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm

float distSphere(float3 pos, float radius)

{

	return length(pos) - radius;

}

float distBox(float3 pos, float sideLength)

{

	float3 dist = abs(pos) - sideLength.xxx;

	return min(max(dist.x, max(dist.y, dist.z)),

			   0.0) +

		   length(max(dist, 0.0));

}

float distPlane(float3 pos)

{

	return pos.y;

}

float distEllipsoid(float3 pos, float3 radius)

{

	float smallestRadius = min(min(radius.x, radius.y), radius.z);

    return smallestRadius * (length(pos / radius) - 1.0);

}

float distTorus(float3 pos, float largeRadius, float smallRadius)

{

	float2 cylinderPos = float2(length(pos.xz) - largeRadius, pos.y);

	return length(cylinderPos) - smallRadius;

}

float distCone(float3 pos, float2 wtf)

{

	//TODO: How tf is the cone defined?

	float q = length(pos.xy);

	return dot(wtf, float2(q, pos.z));

}

//TODO: Add the other smin types.

//TODO: Capsule, cylinder

//TODO: Repetition function.


float distFunc(float3 inputPos)

{

	float4 pos4d;

	float4 inputPos4d = float4(inputPos, 1.0);

	pos4d = mul(

float4x4(

1, 0, 0, 0, 

0, 1, 0, 0, 

0, 0, 1, 0, 

0, 0, 0, 1),

			   inputPos4d);

	float3 pos0 = pos4d.xyz / pos4d.w;

	float dist0;

	{

	dist0 = distBox(pos0, 0.5);

	}

	return dist0;

}



			float4 frag(v2f IN) : SV_Target

			{

				return float4(IN.worldPos, 1.0);

			}



			ENDCG

		}

	}

}

