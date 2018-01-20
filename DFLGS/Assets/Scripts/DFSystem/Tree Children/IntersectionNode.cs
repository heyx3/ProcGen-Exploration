using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;


namespace DFSystem
{
    /// <summary>
    /// Combines all child nodes together.
    /// </summary>
    public class IntersectionNode : Node
    {
		public IntersectionNode(params Node[] children)
			: base(1, int.MaxValue)
		{
			Inputs.AddRange(children);
		}

		public override void EmitVariableDef(StringBuilder outDef, uint uniqueID,
											 Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(ShaderDefs.GetOutputVarName(uniqueID));
			outDef.Append(" = ");
			
			//Edge-case: one input.
			if (Inputs.Count == 1)
			{
				outDef.Append(ShaderDefs.GetOutputVarName(nodeToID[Inputs[0]]));
				outDef.Append(';');
				return;
			}
			
			//Call the "max" function for every pair of inputs.
			for (int i = 0; i < Inputs.Count; ++i)
			{
				var inputVar = ShaderDefs.GetOutputVarName(nodeToID[Inputs[i]]);
				if (i < Inputs.Count - 1)
				{
					outDef.Append("max(");
					outDef.Append(inputVar);
					outDef.Append(", ");
				}
				else
				{
					outDef.Append(inputVar);
				}
			}
			//Close out the "min" calls.
			outDef.Append(')', Inputs.Count - 1);
			outDef.Append(';');
		}
    }
}