//Provides a number of image filters.
//Each filter is defined in one or more passes.
//For some filters, it is necessary to run the passes in the order they are defined.


//The passes are as follows, in order:

//Part 1 of a 5-tap Gaussian blur.
//Part 2 of a 5-tap Gaussian blur.

//Part 1 of a 9-tap Gaussian blur.
//Part 2 of a 9-tap Gaussian blur.

//A greyscale filter that preserves luminance.

//Part 1 of a Sobel edge detection filter.
//Part 2 of a Sobel edge detection filter.

//Part 1 of a Prewitt edge detection filter.
//Part 2 of a Prewitt edge detection filter.

//A "pack vectors" filter that maps values from [-1,+1] to [0,1]. The alpha is left unchanged.


//Some notes about these filters:

//Both Gaussian blur filters are optimized by assuming that
//    the input texture uses linear filtering instead of point filtering,
//    so make sure the texture uses linear filtering.

//The luminance-preserving greyscale filter is good for edge detection filters.

//The Sobel and Prewitt filters operate on a greyscale image
//    (so the input image only needs a Red channel).
//They output 2 values, using the RG channels,
//    representing the horizontal and vertical direction
//    of the detected edge at that pixel.
//The outputs will be negative if the edge faces left/up,
//    so an HDR render target is required.
//Both edge filters do not take advantage of linear texture filtering,
//    so point filtering is recommended to improve performance.


Shader "Hidden/UberFilter"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off
        ZWrite Off
        ZTest Always


        CGINCLUDE
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            
            //Make it easy to quickly write a fragment shader.
            #define START_FRAG float4 frag(v2f i) : SV_Target {
            #define SAMPLE(offset) tex2D(_MainTex, i.uv + (_MainTex_TexelSize.xy * offset))
            #define END_FRAG }

            //Define the "up" and "down" directions in terms of UV.
            #if defined(UNITY_UV_STARTS_AT_TOP)
                #define UV_UP -1.0
                #define UV_DOWN 1.0
            #else
                #define UV_UP 1.0
                #define UV_DOWN -1.0
            #endif
        ENDCG

        
        //Vertical 5-tap Gaussian:
        Pass
        {
            CGPROGRAM
            START_FRAG
                const float c1 = 0.29411764705882354,
                            c2 = 0.35294117647058826;
                const float2 offset = float2(0.0, 1.3333333333);
                return (SAMPLE(0.0) * c1) +
                       (SAMPLE(offset) * c2) +
                       (SAMPLE(-offset) * c2);
            END_FRAG
            ENDCG
        }
        //Horizontal 5-tap Gaussian:
        Pass
        {
            CGPROGRAM
            START_FRAG
                const float c1 = 0.29411764705882354,
                            c2 = 0.35294117647058826;
                const float2 offset = float2(1.3333333333, 0.0);
                return (SAMPLE(0.0) * c1) +
                       (SAMPLE(offset) * c2) +
                       (SAMPLE(-offset) * c2);
            END_FRAG
            ENDCG
        }

        //Vertical 9-tap Gaussian:
        Pass
        {
            CGPROGRAM
            START_FRAG
                const float c1 = 0.227027027,
                            c2 = 0.3162162162,
                            c3 = 0.07027027027;
                const float2 offset1 = float2(0.0, 1.3846153846),
                             offset2 = float2(0.0, 3.2307692308);
                return (SAMPLE(0.0) * c1) +
                       (SAMPLE(offset1) * c2) +
                       (SAMPLE(-offset1) * c2) +
                       (SAMPLE(offset2) * c3) +
                       (SAMPLE(-offset2) * c3);
            END_FRAG
            ENDCG
        }
        //Horizontal 9-tap Gaussian:
        Pass
        {
            CGPROGRAM
            START_FRAG
                const float c1 = 0.227027027,
                            c2 = 0.3162162162,
                            c3 = 0.07027027027;
                const float2 offset1 = float2(1.3846153846, 0.0),
                             offset2 = float2(3.2307692308, 0.0);
                return (SAMPLE(0.0) * c1) +
                       (SAMPLE(offset1) * c2) +
                       (SAMPLE(-offset1) * c2) +
                       (SAMPLE(offset2) * c3) +
                       (SAMPLE(-offset2) * c3);
            END_FRAG
            ENDCG
        }

        //Luminance-preserving Greyscale:
        Pass
        {
            CGPROGRAM
            START_FRAG
                //Source: https://en.wikipedia.org/wiki/Grayscale#Colorimetric_(perceptual_luminance-preserving)_conversion_to_grayscale
                float3 input = SAMPLE(0.0).rgb;
                const float3 coefficients = float3(0.2126, 0.7152, 0.0722);
                return float4(dot(input, coefficients), 0.0, 0.0, 1.0);
            END_FRAG
            ENDCG
        }

        //Vertical Sobel:
		Pass
		{
			CGPROGRAM
            START_FRAG
                float s1 = SAMPLE(float2(0.0, UV_UP)).r,
                      s2 = SAMPLE(0.0).r,
                      s3 = SAMPLE(float2(0.0, UV_DOWN)).r;
                return float4(s1 + (s2 * 2.0) + s3,
                              s1 - s3,
                              0.0, 1.0);
			END_FRAG
			ENDCG
		}
        //Horizontal Sobel:
        Pass
        {
            CGPROGRAM
            START_FRAG
                float2 s1 = SAMPLE(float2(-1.0, 0.0)).xy,
                       s2 = SAMPLE(0.0).xy,
                       s3 = SAMPLE(float2(1.0, 0.0)).xy;
                return float4(s1.x - s3.x,
                              s1.y + (s2.y * 2.0) + s3.y,
                              0.0, 1.0);
            END_FRAG
            ENDCG
        }

        //Vertical Prewitt:
        Pass
        {
            CGPROGRAM
            START_FRAG
                float s1 = SAMPLE(float2(0.0, UV_UP)).r,
                      s2 = SAMPLE(0.0).r,
                      s3 = SAMPLE(float2(0.0, UV_DOWN)).r;
                return float4(s1 + s2 + s3,
                              s3 - s1,
                              0.0, 1.0);
            END_FRAG
            ENDCG
        }
        //Horizontal Prewitt:
        Pass
        {
            CGPROGRAM
            START_FRAG
                float2 s1 = SAMPLE(float2(-1.0, 0.0)).xy,
                       s2 = SAMPLE(0.0).xy,
                       s3 = SAMPLE(float2(1.0, 0.0)).xy;
                return float4(s3.x - s1.x,
                              s1.y + s2.y + s3.y,
                              0.0, 1.0);
            END_FRAG
            ENDCG
        }

        //Pack Vectors:
        Pass
        {
            CGPROGRAM
            START_FRAG
                float4 col = SAMPLE(0.0);
                return float4(0.5 + (0.5 * col.rgb), col.a);
            END_FRAG
            ENDCG
        }
	}
}
