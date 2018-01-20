using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DFSystem
{
    public class PlaneNode : Node
    {
		public PlaneNode() : base(0, 0) { }

		public override void EmitVariableDef(StringBuilder outDef, uint uniqueID,
											 Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(ShaderDefs.GetOutputVarName(uniqueID));
			outDef.Append(" = distPlane(");
			outDef.Append(ShaderDefs.PosInputName);
			outDef.Append(");");
		}
    }
}