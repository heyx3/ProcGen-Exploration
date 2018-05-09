﻿Shader "PolyShape/Generator"
{
    //Generates/refines a concave polygon.
    //The polygon is stored as a horizontal 1D texture where each pixel is a vertex.
    //The Red and Green channels store the position.
    //The Blue channel stores the "variance",
    //    which affects how spread-out the newly-generated points will be.
    //The Alpha channel is not used.
    
    //The polygon texture must have the following properties:
    //    * HDR (i.e. floating-point)
    //    * Point filtering
    //    * Repeat wrapping mode

    //The first pass (index 0) is for generating a new shape from scratch.
    //    this shape will approximate a circle with a certain radius.
    //The second pass (index 1) is for refining the shape by doubling its vertices
    //    (so it's assumed the render target is exactly twice the width of the input texture).

	Properties
	{
        _MainTex ("Input Shape", 2D) = "white" {}
        _Seed ("RNG Seed", Float) = 0.92874243

        //The following properties are for the "generation" pass.
        _Radius ("Initial generation radius", Float) = 1.0
        _InitialVariance ("Initial generation variance min/max", Vector) = (0.25, 0.5, 0.0, 0.0)
        _OutputSize ("Output Size", Float) = 0.0

        //The following properties are for the "refinement" pass.
        _VarianceScale ("Refinement Variation Scale", Vector) = (0.25, 0.75, 0.0, 0.0)
	}
	SubShader
	{
        Tags { "RenderType" = "Opaque" }

        Cull Off
        ZTest Off
        ZWrite Off
        Blend One Zero


        CGINCLUDE

        #include "UnityCG.cginc"
        #include "Assets/Gaussians.cginc"

        #define PI (3.14159265359)
        #define TWO_PI (PI * 2.0)
        float _Seed;

        #pragma vertex vert
        float4 vert(float4 vertex : POSITION) : SV_POSITION
        {
            return UnityObjectToClipPos(vertex);
        }

        ENDCG

        //The generation pass.
        //TODO: Add randomness to the circle.
        Pass
        {
            CGPROGRAM
            float _Radius;
            float2 _InitialVariance;
            float _OutputSize;

            #pragma fragment frag
            float4 frag(UNITY_VPOS_TYPE _pixelPos : VPOS) : SV_Target
            {
                float2 pixelPos = _pixelPos.xy;
                float pixelUVx = (pixelPos.x + 0.5) / _OutputSize;

                float angle = (pixelUVx * TWO_PI);
                float2 vertPos = float2(cos(angle), sin(angle));
                vertPos *= _Radius;

                float variance = lerp(_InitialVariance.x, _InitialVariance.y,
                                      hashTo1(float3(vertPos, _Seed)));

                return float4(vertPos, variance, 1.0);
            }
            ENDCG
        }


        //The refinement pass.
		Pass
		{
			CGPROGRAM
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float2 _VarianceScale;

			#pragma fragment frag
            float4 frag(UNITY_VPOS_TYPE _destPixelPos : VPOS) : SV_Target
			{
                float2 destPixelPos = _destPixelPos.xy;
                float sourcePixelPos = floor((destPixelPos.x / 2.0) + 0.01);
                float sourceUV1 = _MainTex_TexelSize.x * (sourcePixelPos.x + 0.5),
                      sourceUV2 = sourceUV1 + _MainTex_TexelSize.x;

                float4 startVert = tex2D(_MainTex, float2(sourceUV1, 0.0));
                float4 rngVals = hashTo4(float3(startVert.xy, _Seed));

                //Scale down the variance for the next iteration.
                float varianceScale = lerp(_VarianceScale.x, _VarianceScale.y, rngVals.x);

                //If this is an even fragment, the vertex shouldn't move.
                if ((_destPixelPos.x % 1.999999) < 0.5)
                    return float4(startVert.xy, startVert.z * varianceScale, 1.0);


                //If this is an odd fragment, we are generating a new "midpoint" vertex.

                float4 endVert = tex2D(_MainTex, float2(sourceUV2, 0.0));
                float4 midpoint = (startVert + endVert) / 2.0;

                //Generate the offset using a uniform random angle and a Gaussian random magnitude.
                float3 gaussianInput = hashTo3(rngVals.yz);
                float angle = rngVals.w * TWO_PI,
                      distance = startVert.z * gaussian(gaussianInput);
                float2 posOffset = distance * float2(cos(angle), sin(angle));
                //posOffset = startVert.z * lerp(-1.0, 1.0, gaussianInput.xy);
                
                return float4(midpoint.xy + posOffset,
                              startVert.z * varianceScale,
                              1.0);
			}
			ENDCG
		}
	}
}
