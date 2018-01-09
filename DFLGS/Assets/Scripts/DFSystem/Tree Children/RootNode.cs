using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace DFSystem
{
	/// <summary>
	/// The node at the top of the tree.
	/// Its constructor automatically fills in the rest of the tree underneath it.
	/// </summary>
	public class RootNode : Node
	{
		public RootNode(string lSystemValue, Dictionary<char, Command> commands)
		    : base(new State(Vector3.zero, 1.0f, Quaternion.identity))
		{
			Node currentNode = this;

			for (int i = 0; i < lSystemValue.Length; ++i)
				if (commands.ContainsKey(lSystemValue[i]))
					commands[lSystemValue[i]].Apply(ref currentNode);
		}

        protected override void OutputExpression(string childEdgesDist, System.Text.StringBuilder outCode)
        {
            if (childEdgesDist.Length == 0)
            {
                outCode.AppendLine("return 999999.0;");
            }
            else
            {
                outCode.Append("return ");
                outCode.Append(childEdgesDist);
                outCode.AppendLine(";");
            }
        }
	}
}
