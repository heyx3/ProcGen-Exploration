using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;


namespace DFSystem
{
	/// <summary>
	/// A specific shape in the distance field.
	/// </summary>
	public abstract class Node
	{
		public List<Node> Inputs = new List<Node>();

		public int NMinNodes { get; private set; }
		public int NMaxNodes { get; private set; }


		public Node(int nMinNodes, int nMaxNodes)
		{
			NMinNodes = nMinNodes;
			NMaxNodes = nMaxNodes;
		}


		/// <summary>
		/// Writes the shader code to define
		/// </summary>
		public abstract void EmitFunctionDefinition(StringBuilder outDef, int transformMatIndex,
													Dictionary<Node, int> nodeToID);
	}
}