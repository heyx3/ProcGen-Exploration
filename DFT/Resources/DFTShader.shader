Shader "DFT/DFTShader"
{
	Properties
    {
        _MainTex("Input samples", 2D) = "white" {}
    }
	SubShader
	{
		//No culling or depth.
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM

			#pragma target 3.0
            #pragma enable_d3d11_debug_symbols

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DFT_FORWARD DFT_INVERSE
            #pragma multi_compile DFT_HORZ DFT_VERT
            
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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			

            //Define the axis to sample along.
            #ifdef DFT_HORZ
                #define DIR_SAMPLES x
                #define DIR_LINES y
            #else
                #ifdef DFT_VERT
                    #define DIR_SAMPLES y
                    #define DIR_LINES x
                #else
                    //Satisfy IDEs, which don't know about multi_compile.
                    #define DIR_SAMPLES x
                    #define DIR_LINES y
                #endif
            #endif

            //Multiplies two complex numbers.
            float2 MultiplyComplex(float2 a, float2 b)
            {
                return float2((a.x * b.x) - (a.y * b.y),
                              (a.x * b.y) + (a.y * b.x));
            }

            //Gets the given piece of the DFT given the input to the sine/cosine
            //    and the sample/amplitude.
            float2 GetSampleResult(float trigInput, float2 sample)
            {
                #ifdef DFT_FORWARD
                    return MultiplyComplex(sample, float2(cos(trigInput), -sin(trigInput)));
                #else
                    //DFT_INVERSE
                    float _sin = sin(trigInput),
                          _cos = cos(trigInput);
                    return (sample.x * float2(_cos, _sin)) +
                           (sample.y * float2(-_sin, _cos));
                #endif
            }

            #define TWO_PI (2.0 * 3.1415926536)

			uniform sampler2D _MainTex;
            #define u_Samples _MainTex

            uniform float4 _MainTex_TexelSize;
            #define u_SamplesTexelSize _MainTex_TexelSize

            uniform int u_SamplesSizeX, u_SamplesSizeY;

			float4 frag (v2f i) : SV_Target
			{
                int2 u_SamplesSize = int2(u_SamplesSizeX, u_SamplesSizeY);

                float2 result = 0.0;
                float2 samplePos = 0.0;
                samplePos.DIR_LINES = i.uv.DIR_LINES + (0.5 * u_SamplesTexelSize.DIR_LINES);
                float _trigInput = TWO_PI * i.uv.DIR_SAMPLES;
                for (int sampleI = 0; sampleI < u_SamplesSize.DIR_SAMPLES; ++sampleI)
                {
                    //Get the sample.
                    samplePos.DIR_SAMPLES = sampleI * u_SamplesTexelSize.DIR_SAMPLES;
                    float2 sample = tex2D(u_Samples, samplePos).xy;
        
                    //Combine the sample with the sine/cosine waves.
                    float trigInput = _trigInput * sampleI;
                    result += GetSampleResult(trigInput, sample);
                }

            //TODO: Some kinda off-by-one sampling error. Try the compute shader again?
            #ifdef DFT_FORWARD
                result /= u_SamplesSize.DIR_SAMPLES;
            #endif
                return float4(result, 0.0, 1.0);
			}

			ENDCG
		}
	}
}
