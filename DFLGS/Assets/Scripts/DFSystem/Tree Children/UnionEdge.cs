using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;


namespace DFSystem
{
    /// <summary>
    /// Combines all child nodes together.
    /// </summary>
    public class UnionEdge : Edge
    {
        public UnionEdge(Node parent) : base(parent) { }


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
                outCode.Append("\treturn min(");
                for (int i = 0; i < childFuncs.Count; ++i)
                {
                    outCode.Append(childFuncs[i]);
                    outCode.Append("(pos), ");
                    if (i < childFuncs.Count - 2)
                    {
                        outCode.Append("min(");
                    }
                }
                outCode.Append(')', childFuncs.Count - 1);
                outCode.AppendLine(";");
            }
        }
    }
}