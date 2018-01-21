﻿using System;
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

			mat = Matrix4x4.identity;
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

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write(pos.x);
			writer.Write(pos.y);
			writer.Write(pos.z);

			writer.Write(rot.x);
			writer.Write(rot.y);
			writer.Write(rot.z);
			writer.Write(rot.w);

			writer.Write(scale.x);
			writer.Write(scale.y);
			writer.Write(scale.z);

			for (int col = 0; col < 4; ++col)
				for (int row = 0; row < 4; ++row)
					writer.Write(mat[row, col]);

#if UNITY_EDITOR
			writer.Write(manualMatrixEdit);
#endif
		}
		protected override void _Deserialize(BinaryReader reader)
		{
			base._Deserialize(reader);

			pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			rot = new Quaternion(reader.ReadSingle(), reader.ReadSingle(),
								 reader.ReadSingle(), reader.ReadSingle());
			scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

			for (int col = 0; col < 4; ++col)
				for (int row = 0; row < 4; ++row)
					mat[row, col] = reader.ReadSingle();

#if UNITY_EDITOR
			manualMatrixEdit = reader.ReadBoolean();
#endif
		}

#if UNITY_EDITOR
		private bool manualMatrixEdit = false;
		public override void EditorGUI()
		{
			base.EditorGUI();

			if (manualMatrixEdit)
			{
				var layout = new GUILayoutOption[]
				{
					GUILayout.Width(100.0f),
				};

				for (int col = 0; col < 4; ++col)
					for (int row = 0; row < 4; ++row)
						mat[row, col] = EditorGUILayout.FloatField(mat[row, col], layout);
			}
			else
			{
				pos = EditorGUILayout.Vector3Field("Pos", pos);

				UnityEditor.EditorGUI.BeginChangeCheck();
				Vector3 eulerAngles = EditorGUILayout.Vector3Field("Rot", rot.eulerAngles);
				if (UnityEditor.EditorGUI.EndChangeCheck())
					rot = Quaternion.Euler(eulerAngles);

				scale = EditorGUILayout.Vector3Field("Scale", scale);

				RecalcMat();
			}
		}
#endif
	}
}
