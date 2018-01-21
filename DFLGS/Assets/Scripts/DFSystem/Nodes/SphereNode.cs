using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DFSystem
{
    public class SphereNode : Node
    {
        public float Radius;

        public SphereNode(float radius)
            : base(0, 0)
        {
            Radius = radius;
        }

		public override void EmitVariableDef(StringBuilder outDef,
											 string posName, string varNamePrefix,
											 uint uniqueID, Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(varNamePrefix);
			outDef.Append(uniqueID);
			outDef.Append(" = distSphere(");
			outDef.Append(posName);
			outDef.Append(", ");
			outDef.Append(Radius);
			outDef.Append(");");
		}
    }
}