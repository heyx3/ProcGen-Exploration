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
    public class EllipsoidNode : Node
    {
        public Vector3 Radius;

        public EllipsoidNode(Vector3 radius)
            : base(0, 0)
        {
            Radius = radius;
        }

		public override void EmitVariableDef(StringBuilder outDef,
											 string posName, string varNamePrefix,
											 uint uniqueID, Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(varNamePrefix);
			outDef.Append(uniqueID);
			outDef.Append(" = distEllipse(");
			outDef.Append(posName);
			outDef.Append(", ");
			outDef.Append(Radius);
			outDef.Append(");");
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Radius.x);
			writer.Write(Radius.y);
			writer.Write(Radius.z);
		}
		protected override void _Deserialize(BinaryReader reader)
		{
			base._Deserialize(reader);
			Radius = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

#if UNITY_EDITOR
		public override void EditorGUI()
		{
			base.EditorGUI();
			Radius = EditorGUILayout.Vector3Field("Radius", Radius);
		}
#endif
    }
}