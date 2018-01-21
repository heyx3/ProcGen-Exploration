using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


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

		public override void EmitVariableDef(StringBuilder outDef,
											 string posName, string varNamePrefix,
											 uint uniqueID, Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(varNamePrefix);
			outDef.Append(uniqueID);
			outDef.Append(" = ");
			
			//Edge-case: one input.
			if (Inputs.Count == 1)
			{
				outDef.Append(varNamePrefix);
				outDef.Append(nodeToID[Inputs[0]]);
				outDef.Append(';');
				return;
			}

			outDef.Append("max(-");
			outDef.Append(varNamePrefix);
			outDef.Append(nodeToID[Inputs[0]]);
			outDef.Append(", ");

			//Call the "min" function for every pair of inputs.
			for (int i = 1; i < Inputs.Count; ++i)
			{
				if (i < Inputs.Count - 1)
				{
					outDef.Append("min(");
					outDef.Append(varNamePrefix);
					outDef.Append(nodeToID[Inputs[i]]);
					outDef.Append(", ");
				}
				else
				{
					outDef.Append(varNamePrefix);
					outDef.Append(nodeToID[Inputs[i]]);
				}
			}
			//Close out the function calls.
			outDef.Append(')', Inputs.Count - 1);
			outDef.Append(';');
		}
    }
}