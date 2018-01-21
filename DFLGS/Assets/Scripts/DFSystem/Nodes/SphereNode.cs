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
    public class SphereNode : Node
    {
        public float Radius;

        public SphereNode(float radius)
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
			outDef.Append(" = distSphere(");
			outDef.Append(posName);
			outDef.Append(", ");
			outDef.Append(Radius);
			outDef.Append(");");
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write(Radius);
		}
		protected override void _Deserialize(BinaryReader reader)
		{
			base._Deserialize(reader);

			Radius = reader.ReadSingle();
		}

#if UNITY_EDITOR
		public override void EditorGUI()
		{
			base.EditorGUI();

			Radius = EditorGUILayout.FloatField("Radius", Radius);
		}
#endif
    }
}