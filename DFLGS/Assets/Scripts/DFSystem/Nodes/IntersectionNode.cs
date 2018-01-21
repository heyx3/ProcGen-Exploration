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
    /// Combines all child nodes together.
    /// </summary>
    public class IntersectionNode : Node
    {
		public IntersectionNode(params Node[] children)
			: base(1, int.MaxValue)
		{
			Inputs.AddRange(children);
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
			
			//Call the "max" function for every pair of inputs.
			for (int i = 0; i < Inputs.Count; ++i)
			{
				if (i < Inputs.Count - 1)
				{
					outDef.Append("max(");
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
			//Close out the "min" calls.
			outDef.Append(')', Inputs.Count - 1);
			outDef.Append(';');
		}
    }
}