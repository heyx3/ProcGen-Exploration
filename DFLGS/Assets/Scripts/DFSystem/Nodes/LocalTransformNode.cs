using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace DFSystem
{
	/// <summary>
	/// Represents a transformation, affecting all child nodes.
	/// Can be manipulated through Pos/Rot/Scale properties,
	///     or through the Mat property directly for weirder transforms.
	/// </summary>
	public class LocalTransformNode : Node
	{
		public Vector3 Pos
		{
			get { return pos; }
			set { pos = value; RecalcMat(); }
		}
		public Quaternion Rot
		{
			get { return rot; }
			set { rot = value; RecalcMat(); }
		}
		public Vector3 Scale
		{
			get { return scale; }
			set { scale = value; RecalcMat(); }
		}
		public Matrix4x4 Mat
		{
			get { return mat; }
			set { mat = value; }
		}

		private Vector3 pos, scale;
		private Quaternion rot;
		private Matrix4x4 mat;

		public LocalTransformNode()
			: base(1, 1)
		{
			pos = Vector3.zero;
			rot = Quaternion.identity;
			scale = Vector3.one;
		}
		public LocalTransformNode(Matrix4x4 _mat)
			: this()
		{
			mat = _mat;
		}
		public LocalTransformNode(Vector3 _pos, Quaternion _rot, Vector3 _scale)
			: this()
		{
			pos = _pos;
			rot = _rot;
			scale = _scale;
			RecalcMat();
		}

		private void RecalcMat()
		{
			mat = Matrix4x4.TRS(pos, rot, scale);
		}

		public override void EmitVariableDef(StringBuilder outDef,
											 string posName, string varNamePrefix,
											 uint uniqueID, Dictionary<Node, uint> nodeToID)
		{
			//Just use the value from its child node.
			outDef.Append(varNamePrefix);
			outDef.Append(uniqueID);
			outDef.Append(" = ");
			outDef.Append(varNamePrefix);
			outDef.Append(nodeToID[Inputs[0]]);
			outDef.Append(';');
		}
		public override Matrix4x4 Transform()
		{
			return Mat;
		}
	}
}
