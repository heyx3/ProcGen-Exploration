using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DFSystem
{
    public class PlaneNode : Node
    {
		public PlaneNode() : base(0, 0) { }

		public override void EmitVariableDef(StringBuilder outDef,
											 string posName, string varNamePrefix,
											 uint uniqueID, Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(varNamePrefix);
			outDef.Append(uniqueID);
			outDef.Append(" = distPlane(");
			outDef.Append(posName);
			outDef.Append(");");
		}
    }
}