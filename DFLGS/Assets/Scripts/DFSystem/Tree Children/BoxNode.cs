using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace DFSystem
{
    public class BoxNode : Node
    {
        public Vector3 Size;


        public BoxNode(Vector3 size, State thisState)
            : base(thisState)
        {
            Size = size;
        }


        protected override void OutputExpression(string childEdgesDist, StringBuilder outCode)
        {
            outCode.Append("\tfloat3 d = abs(pos) - float3(");
            outCode.Append(Size.x);
            outCode.Append(", ");
            outCode.Append(Size.y);
            outCode.Append(", ");
            outCode.Append(Size.z);
            outCode.AppendLine(");");

            string expr = "((min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0)))) / " + ThisState.Scale;

            if (childEdgesDist.Length == 0)
            {
                outCode.Append("\treturn ");
                outCode.Append(expr);
                outCode.AppendLine(";");
            }
            else
            {
                outCode.Append("\treturn min(");
                outCode.Append(expr);
                outCode.Append(", ");
                outCode.Append(childEdgesDist);
                outCode.AppendLine(");");
            }
        }
    }
}