using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace DFSystem
{
    public class TorusNode : Node
    {
        public int Axis;
        private string AxisComponent
        {
            get
            {
                switch (Axis)
                {
                    case 0: return "x";
                    case 1: return "y";
                    case 2: return "z";
                    default:
                        return "[Unknown axis " + Axis + "]";
                }
            }
        }
        private string PlaneSwizzle
        {
            get
            {
                switch (Axis)
                {
                    case 0: return "yz";
                    case 1: return "xz";
                    case 2: return "xy";
                    default:
                        return "[Unknown axis " + Axis + "]";
                }
            }
        }

        public float MajorRadius, MinorRadius;


        public TorusNode(int axis, float majorRadius, float minorRadius,
                         State thisState)
            : base(thisState)
        {
            Axis = axis;
            MajorRadius = majorRadius;
            MinorRadius = minorRadius;
        }


        protected override void OutputExpression(string childEdgesDist, StringBuilder outCode)
        {
            outCode.Append("\tfloat2 q = float2(length(pos.");
            outCode.Append(PlaneSwizzle);
            outCode.Append(") - ");
            outCode.Append(MajorRadius);
            outCode.Append(", pos.");
            outCode.Append(AxisComponent);
            outCode.AppendLine(");");

            string expr = "((length(q) - )" + MinorRadius + ") / " + ThisState.Scale;

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