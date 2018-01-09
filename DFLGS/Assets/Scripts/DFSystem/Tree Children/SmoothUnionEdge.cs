using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;


namespace DFSystem
{
    /// <summary>
    /// Combines all child nodes together with smooth transitions at their seams.
    /// </summary>
    public class SmoothUnionEdge : Edge
    {
        public float Smoothness;

        public SmoothUnionEdge(Node parent, float smoothness = 0.1f)
            : base(parent)
        {
            Smoothness = smoothness;
        }


        protected override void OutputExpression(List<string> childFuncs, StringBuilder outCode)
        {
            if (childFuncs.Count == 0)
            {
                outCode.AppendLine("\treturn 9999999999.0;");
            }
            else if (childFuncs.Count == 1)
            {
                outCode.Append("\treturn ");
                outCode.Append(childFuncs[0]);
                outCode.AppendLine("(pos);");
            }
            else
            {
                //Get each child's distance.
                for (int i = 0; i < childFuncs.Count; ++i)
                {
                    outCode.Append("\tfloat d");
                    outCode.Append(i + 1);
                    outCode.Append(" = ");
                    outCode.Append(childFuncs[i]);
                    outCode.AppendLine("(pos);");
                }
                outCode.Append("\treturn ");


                //Call "smin" on all child distances.

                StringBuilder lastCall = new StringBuilder();
                string smoothnessStr = Smoothness.ToString();
                ShaderConsts.CallSmin(lastCall, "d1", "d2", smoothnessStr);

                for (int i = 2; i < childFuncs.Count; ++i)
                {
                    string lastCallStr = lastCall.ToString();
                    ShaderConsts.CallSmin(lastCall, lastCallStr, "d" + (i + 1), smoothnessStr);
                }
                outCode.AppendLine(";");
            }
        }
    }
}