using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


namespace DFSystem
{
	/// <summary>
	/// A mapping from L-system tokens to DF expressions.
	/// Some expressions have an "end" char to indicate the end of all "child" nodes.
	/// </summary>
	public class Command
	{
		public char StartChar;
		public char? EndChar;
		public Node Node;

		public Command() { }
		public Command(char startChar, Node node, char? endChar = null)
		{
			StartChar = startChar;
			EndChar = endChar;
			Node = node;
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(StartChar);
			writer.Write(EndChar.HasValue);
			if (EndChar.HasValue)
				writer.Write(EndChar.Value);
			Node.Serialize(writer);
		}
		public void Deserialize(BinaryReader reader)
		{
			StartChar = reader.ReadChar();
			EndChar = (reader.ReadBoolean() ? reader.ReadChar() : new Nullable<char>());
			Node = Node.Deserialize(reader);
		}

		public override string ToString()
		{
			string str = StartChar.ToString();
			if (EndChar.HasValue)
				str += EndChar.Value;
			return str;
		}
	}
}
