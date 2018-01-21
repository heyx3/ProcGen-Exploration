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
    public class PlaneNode : Node
    {
		public PlaneNode() : base(0, 0) { }

		public override void EmitVariableDef(StringBuilder outDef,
											 string posName, string varNamePrefix,
											 uint uniqueID, Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(varNamePrefix);
			outDef.Append(uniqueID);
			outDef.Append(" = distPlane(");
			outDef.Append(posName);
			outDef.Append(");");
		}
    }
}