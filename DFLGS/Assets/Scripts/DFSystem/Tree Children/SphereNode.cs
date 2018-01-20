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

		public override void EmitVariableDef(StringBuilder outDef, uint uniqueID,
											 Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(ShaderDefs.GetOutputVarName(uniqueID));
			outDef.Append(" = distSphere(");
			outDef.Append(ShaderDefs.PosInputName);
			outDef.Append(", ");
			outDef.Append(Radius);
			outDef.Append(");");
		}
    }
}