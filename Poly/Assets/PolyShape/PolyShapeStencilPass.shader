Shader"PolyShape/StencilPass"
{
    //Run this shader in a render texture to rasterize the PolyShape.
    //Note that the shader is designed so that the PolyShape is stretched to fill the whole texture.

	Properties
	{
        _ShapeMin("Shape Min", Vector) = (0,0,0,0)
        _ShapeMax("Shape Max", Vector) = (1,1,0,0)

        _PointOnShape("Point On Shape", Vector) = (0,0,0,0)
	}
	SubShader
	{
		Pass
		{
		    Tags { "RenderType"="Opaque" }

            //For every line in the polygon, render a triangle
            //    made of that line plus a constant arbitrary point.
            //Count in the stencil buffer whether each pixel was shaded an even or odd number of times.
            //The next shader will use this information to see what parts of the shape are inside.
            Cull Off
            ZTest Off
            ZWrite Off
            ColorMask 0
            Blend One Zero

            Stencil {
                Pass Invert
                WriteMask 1
            }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma geometry geom
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

            float2 _ShapeMin, _ShapeMax, _PointOnShape;
			
            float inverseLerp(float a, float b, float x)
            {
                return (x - a) / (b - a);
            }

			v2f vert (appdata v)
			{
				v2f o;
    
                //Map the vertices to the range [-1, 1] based on the given min/max.
                o.vertex = float4(lerp(-1.0, 1.0,
                                       inverseLerp(_ShapeMin.x, _ShapeMax.x,
                                                   v.vertex.x)),
                                  lerp(
                                       #if UNITY_UV_STARTS_AT_TOP
                                         1.0, -1.0,
                                       #else
                                         -1.0, 1.0,
                                       #endif
                                       inverseLerp(_ShapeMin.y, _ShapeMax.y,
                                                   v.vertex.y)),
                                  0.01,
                                  1.0);

				return o;
			}

            [maxvertexcount(3)]
            void geom(line v2f input[2], inout TriangleStream<v2f> OutputStream)
            {
                //The line segment makes one third of a triangle.
                OutputStream.Append(input[0]);
                OutputStream.Append(input[1]);
                
                //Add a third point, assumed to be a vertex of the polygon.
                v2f triTip;
                triTip.vertex = float4(_PointOnShape.xy, input[0].vertex.z, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                    triTip.vertex.y = -triTip.vertex.y;
                #endif
                OutputStream.Append(triTip);
            }
			
			fixed4 frag (v2f i) : SV_Target
			{
                return 1.0;
			}
			ENDCG
		}
	}
}
