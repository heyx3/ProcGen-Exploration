using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace DFSystem
{
    public class EllipsoidNode : Node
    {
        public Vector3 Radius;

        public EllipsoidNode(Vector3 radius)
            : base(0, 0)
        {
            Radius = radius;
        }

		public override void EmitVariableDef(StringBuilder outDef, uint uniqueID,
											 Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(ShaderDefs.GetOutputVarName(uniqueID));
			outDef.Append(" = distEllipse(");
			outDef.Append(ShaderDefs.PosInputName);
			outDef.Append(", ");
			outDef.Append(Radius);
			outDef.Append(");");
		}
    }
}