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
    public class TorusNode : Node
    {
        public float LargeRadius, SmallRadius;

        public TorusNode(float largeRadius, float smallRadius)
			: base(0, 0)
        {
			LargeRadius = largeRadius;
			SmallRadius = smallRadius;
        }

		public override void EmitVariableDef(StringBuilder outDef,
											 string posName, string varNamePrefix,
											 uint uniqueID, Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(varNamePrefix);
			outDef.Append(uniqueID);
			outDef.Append(" = distTorus(");
			outDef.Append(posName);
			outDef.Append(", ");
			outDef.Append(LargeRadius);
			outDef.Append(", ");
			outDef.Append(SmallRadius);
			outDef.Append(");");
		}

		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write(LargeRadius);
			writer.Write(SmallRadius);
		}
		protected override void _Deserialize(BinaryReader reader)
		{
			base._Deserialize(reader);

			LargeRadius = reader.ReadSingle();
			SmallRadius = reader.ReadSingle();
		}

#if UNITY_EDITOR
		public override void EditorGUI()
		{
			base.EditorGUI();

			LargeRadius = EditorGUILayout.FloatField("Large r:", LargeRadius);
			SmallRadius = EditorGUILayout.FloatField("Small r:", SmallRadius);
		}
#endif
    }
}