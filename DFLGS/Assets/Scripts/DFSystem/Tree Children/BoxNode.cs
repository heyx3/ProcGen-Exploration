﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


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

		public override void EmitVariableDef(StringBuilder outDef, uint uniqueID,
											 Dictionary<Node, uint> nodeToID)
		{
			outDef.Append(ShaderDefs.GetOutputVarName(uniqueID));
			outDef.Append(" = distBox(");
			outDef.Append(ShaderDefs.PosInputName);
			outDef.Append(", ");
			outDef.Append(SideLength);
			outDef.Append(");");
		}
    }
}