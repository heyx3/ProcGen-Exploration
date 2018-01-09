using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DFSystem
{
    public class SphereNode : Node
    {
        public float Radius;


        public SphereNode(float radius, State thisState)
            : base(thisState)
        {
            Radius = radius;
        }


        protected override void OutputExpression(string childEdgesDist, StringBuilder outCode)
        {
            string expr = "(length(pos) - " + Radius.ToString() + ") / " + ThisState.Scale;

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