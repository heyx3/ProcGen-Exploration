using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;


namespace DFSystem
{
	/// <summary>
	/// The different ways to combine shapes.
	/// </summary>
	public enum UnionTypes
	{
		Hard,
		Soft,
	}

    /// <summary>
    /// Combines all child nodes together.
    /// </summary>
    public class UnionNode : Node
    {
		public UnionTypes UnionType = UnionTypes.Hard;
		public float ExtraParam1 = 0.1f;

		public UnionNode(params Node[] children)
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
			
			//Edge-case: one input to "union" together.
			if (Inputs.Count == 1)
			{
				outDef.Append(varNamePrefix);
				outDef.Append(nodeToID[Inputs[0]]);
				outDef.Append(';');
				return;
			}
			
			//Figure out what kind of "min" function to call.
			string funcName = null,
				   extraArgs = null;
			switch (UnionType)
			{
				case UnionTypes.Hard:
					funcName = "min";
					extraArgs = "";
					break;
				case UnionTypes.Soft:
					funcName = "smin";
					extraArgs = ", " + ExtraParam1;
					break;
				default: throw new ArgumentException(UnionType.ToString());
			}
			//Call the "min" function for every pair of inputs.
			for (int i = 0; i < Inputs.Count; ++i)
			{
				if (i < Inputs.Count - 1)
				{
					outDef.Append(funcName);
					outDef.Append("(");
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
			for (int i = 1; i < Inputs.Count; ++i)
			{
				outDef.Append(extraArgs);
				outDef.Append(')');
			}
			outDef.Append(';');
		}
    }
}