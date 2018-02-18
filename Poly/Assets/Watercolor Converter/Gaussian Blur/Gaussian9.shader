Shader "Poly/Gaussian9"
{
    //A 2-pass 9x9 Gaussian filter.

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off
        ZWrite Off
        ZTest Always

        //Both passes are identical except for the axis they blend along.
        CGINCLUDE

        #include "UnityCG.cginc"
        #include "Gaussian.cginc"

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
        float4 SampleGaussian(v2f i, float2 dir)
        {
            return gaussian9Line(_MainTex, i.uv, _MainTex_TexelSize.xy, dir);
        }

        ENDCG
            
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			float4 frag (v2f i) : SV_Target
			{
                return SampleGaussian(i, float2(1.0, 0.0));
			}
			ENDCG
		}   
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			float4 frag (v2f i) : SV_Target
			{
                return SampleGaussian(i, float2(0.0, 1.0));
			}
			ENDCG
		}
	}
}
