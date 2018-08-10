Shader "PolyShape/Renderer"
{
    //A two-pass material to render a polygon to a texture.

    //The first pass should be given a line strip mesh,
    //    where each line is between two texels in the shape's data texture.

    //The second pass should be given a full-screen quad
    //    (or at least a quad that covers the whole shape).
    //The quad does not transform using a transform matrix

    //Uses the stencil buffer.

	Properties
	{
        //Properties for the first pass:
        _MainTex ("Shape texture", 2D) = "white" {}
        _ShapeOffsetAndScale ("Shape Offset (XY) and Scale (ZW)", Vector) = (0,0,1,1)

        //Properties for the second pass:
        _Color ("In Poly Color", Color) = (1,1,1,1)
        _BlendSrc ("Blend Src", Int) = 5 //SrcAlpha
        _BlendDest ("Blend Dest", Int) = 10 //OneMinusSrcAlpha
        _BlendOp ("Blend Op", Int) = 0 //Add
	}
	SubShader
	{
        Tags { }

        Cull Off
        ZTest Off
        ZWrite Off

        Pass
        {
            //For every line in the polygon, render a triangle
            //    made of that line plus a constant arbitrary point.
            //Use the stencil buffer to count whether each pixel was rendered
            //    an even or odd number of times.
            //The next pass will use the stencil buffer to color in the shape.

            Blend One Zero
            ColorMask 0
            Stencil {
                Pass Invert
                WriteMask 1
            }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma geometry geom
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

            float4 _ShapeOffsetAndScale;
            sampler2D _MainTex;
			
            float inverseLerp(float a, float b, float x)
            {
                return (x - a) / (b - a);
            }
            float2 transformPoint(float2 p)
            {
                return _ShapeOffsetAndScale.xy + (_ShapeOffsetAndScale.zw * p);
            }

			float4 vert(float4 inV : POSITION) : SV_POSITION
			{
                float2 vertPos = tex2Dlod(_MainTex, float4(inV.xy, 0.0, 0.0)).xy;
                vertPos = transformPoint(vertPos);

                return float4(vertPos, 0.00001, 1.0);
			}

            [maxvertexcount(3)]
            void geom(line v2f input[2], inout TriangleStream<v2f> OutputStream)
            {
                //The line segment makes one end of a triangle.
                OutputStream.Append(input[0]);
                OutputStream.Append(input[1]);

                //Add a third point -- the first vertex in the polygon.
                v2f v3;
                v3.vertex = float4(transformPoint(tex2Dlod(_MainTex, float4(0,0,0,0)).xy),
                                   input[0].vertex.z, 1.0);
                OutputStream.Append(v3);
            }
			
            //Fragment shader doesn't actually matter, but it's required.
			fixed4 frag(v2f i) : SV_Target
			{
                return 1.0;
			}
			ENDCG
		}

		Pass
		{
            Blend [_BlendSrc] [_BlendDest]
            BlendOp [_BlendOp]

            Stencil {
                Ref 1
                Comp Equal
            }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

            float4 _Color;
			
			float4 vert(float4 inV : POSITION) : SV_POSITION
			{
                return inV;
			}

			fixed4 frag() : SV_Target
			{
                return _Color;
			}
			ENDCG
		}
	}
}
