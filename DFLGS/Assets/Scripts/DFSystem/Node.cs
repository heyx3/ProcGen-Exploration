using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;


namespace DFSystem
{
	/// <summary>
	/// An expression in the distance field.
	/// Part of a tree structure, where child nodes are sub-expressions.
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
		/// Applies this node's local transformation, to be applied on top of its parent.
		/// Default behavior: the identity transform.
		/// </summary>
		public virtual Matrix4x4 Transform() { return Matrix4x4.identity; }
		/// <summary>
		/// Writes the shader code to compute this node's distance value.
		/// The final distance value should be named ShaderDefs.GetOutputVarName(uniqueID).
		/// </summary>
		public abstract void EmitVariableDef(StringBuilder outDef,
											 string posName, string varNamePrefix,
											 uint uniqueID, Dictionary<Node, uint> nodeToID);
	}
}