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
    public class BoxNode : Node
    {
        public float SideLength;

        public BoxNode(float sideLength)
			: base(0, 0)
        {
			SideLength = sideLength;
        }

		public override void EmitVariableDef(StringBuilder outDef,
											 string posName, string varNamePrefix,
											 uint uniqueID, Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(varNamePrefix);
			outDef.Append(uniqueID);
			outDef.Append(" = distBox(");
			outDef.Append(posName);
			outDef.Append(", ");
			outDef.Append(SideLength);
			outDef.Append(");");
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(SideLength);
		}
		protected override void _Deserialize(BinaryReader reader)
		{
			base._Deserialize(reader);
			SideLength = reader.ReadSingle();
		}

#if UNITY_EDITOR
		public override void EditorGUI()
		{
			base.EditorGUI();
			SideLength = EditorGUILayout.FloatField("Side Length", SideLength);
		}
#endif
	}
}