using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace DFSystem
{
    public class BoxNode : Node
    {
        public float SideLength;

        public BoxNode(float sideLength)
			: base(0, 0)
        {
			SideLength = sideLength;
        }

		public override void EmitVariableDef(StringBuilder outDef,
											 string posName, string varNamePrefix,
											 uint uniqueID, Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(varNamePrefix);
			outDef.Append(uniqueID);
			outDef.Append(" = distBox(");
			outDef.Append(posName);
			outDef.Append(", ");
			outDef.Append(SideLength);
			outDef.Append(");");
		}
    }
}