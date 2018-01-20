using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;


namespace DFSystem
{
    /// <summary>
	/// Subtracts a number of shapes from another shape.
    /// </summary>
    public class SubtractionNode : Node
    {
		public SubtractionNode(Node body, params Node[] toSub)
			: base(1, int.MaxValue)
		{
			Inputs.Add(body);
			Inputs.AddRange(toSub);
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

			outDef.Append("max(-");
			outDef.Append(ShaderDefs.GetOutputVarName(nodeToID[Inputs[0]]));
			outDef.Append(", ");

			//Call the "min" function for every pair of inputs.
			for (int i = 1; i < Inputs.Count; ++i)
			{
				var inputVar = ShaderDefs.GetOutputVarName(nodeToID[Inputs[i]]);
				if (i < Inputs.Count - 1)
				{
					outDef.Append("min(");
					outDef.Append(inputVar);
					outDef.Append(", ");
				}
				else
				{
					outDef.Append(inputVar);
				}
			}
			//Close out the function calls.
			outDef.Append(')', Inputs.Count - 1);
			outDef.Append(';');
		}
    }
}