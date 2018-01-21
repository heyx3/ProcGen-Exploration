using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace DFSystem
{
    public class TorusNode : Node
    {
        public float LargeRadius, SmallRadius;

        public TorusNode(float largeRadius, float smallRadius)
			: base(0, 0)
        {
			LargeRadius = largeRadius;
			SmallRadius = smallRadius;
        }

		public override void EmitVariableDef(StringBuilder outDef,
											 string posName, string varNamePrefix,
											 uint uniqueID, Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(varNamePrefix);
			outDef.Append(uniqueID);
			outDef.Append(" = distTorus(");
			outDef.Append(posName);
			outDef.Append(", ");
			outDef.Append(LargeRadius);
			outDef.Append(", ");
			outDef.Append(SmallRadius);
			outDef.Append(");");
		}
    }
}