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
		//TODO: Add LocalTransform and WorldTranslate node.


		public List<Node> Inputs = new List<Node>();

		public int NMinNodes { get; private set; }
		public int NMaxNodes { get; private set; }


		public Node(int nMinNodes, int nMaxNodes)
		{
			NMinNodes = nMinNodes;
			NMaxNodes = nMaxNodes;
		}


		/// <summary>
		/// Applies this node's transformation on top of the given one.
		/// Default behavior: the identity transformation.
		/// </summary>
		public virtual Matrix4x4 Transform(Matrix4x4 currentMat) { return currentMat; }
		/// <summary>
		/// Writes the shader code to compute this node's distance value.
		/// The final distance value should be named ShaderDefs.GetOutputVarName(uniqueID).
		/// </summary>
		public abstract void EmitVariableDef(StringBuilder outDef, uint uniqueID,
											 Dictionary<Node, uint> nodeToID);
	}
}