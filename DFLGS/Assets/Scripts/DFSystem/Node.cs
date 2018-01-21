using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;


namespace DFSystem
{
	/// <summary>
	/// An expression in the distance field.
	/// Part of a tree structure, where child nodes are sub-expressions.
	/// </summary>
	public abstract class Node
	{
		public List<Node> Inputs = new List<Node>();

		public int NMinNodes { get; private set; }
		public int NMaxNodes { get; private set; }


		public Node(int nMinNodes, int nMaxNodes)
		{
			NMinNodes = nMinNodes;
			NMaxNodes = nMaxNodes;
		}


		/// <summary>
		/// Calculates this node's local transformation, to be applied on top of its parent.
		/// Default behavior: the identity transform.
		/// </summary>
		public virtual Matrix4x4 Transform() { return Matrix4x4.identity; }
		/// <summary>
		/// Writes the shader code to compute this node's distance value.
		/// </summary>
		/// <param name="varNamePrefix">
		/// A node's output is already defined as a float named (varNamePrefix + uniqueID).
		/// </param>
		/// <param name="nodeToID">
		/// A lookup for the unique ID of any node.
		/// </param>
		public abstract void EmitVariableDef(StringBuilder outDef,
											 string posName, string varNamePrefix,
											 uint uniqueID, Dictionary<Node, uint> nodeToID);

		public virtual void Serialize(BinaryWriter writer)
		{
			writer.Write(NodeSerialization.GetTypeName(this));
		}
		public static Node Deserialize(BinaryReader reader)
		{
			string typeName = reader.ReadString();
			var node = NodeSerialization.MakeNode(typeName);
			node._Deserialize(reader);
			return node;
		}
		protected virtual void _Deserialize(BinaryReader reader) { }

#if UNITY_EDITOR
		/// <summary>
		/// Displays a GUI for editing this node's properties.
		/// </summary>
		public virtual void EditorGUI() { }
#endif
	}


	/// <summary>
	/// Serializes/deserializes node types.
	/// </summary>
	public static class NodeSerialization
	{
		public static Func<Node> GetFactory(string typeName)
		{
			return GetEntry(typeName).Factory;
		}
		public static Node MakeNode(string typeName)
		{
			return GetFactory(typeName)();
		}

		public static string GetTypeName(Node node)
		{
			return GetEntry(node).Name;
		}

		public static IEnumerable<string> Options
		{
			get { return entries.Select(e => e.Name); }
		}
		

		private struct Entry
		{
			public Type Type;
			public Func<Node> Factory;
			public string Name;
			public Entry(Func<Node> factory)
			{
				Factory = factory;
				Type = Factory().GetType();
				Name = Type.Name;
			}
		}
		private static readonly Entry[] entries = new Entry[]
		{
			new Entry(() => new BoxNode(1.0f)),
			new Entry(() => new SphereNode(1.0f)),
			new Entry(() => new EllipsoidNode(Vector3.one)),
			new Entry(() => new PlaneNode()),
			new Entry(() => new TorusNode(1.0f, 0.1f)),
			new Entry(() => new UnionNode()),
			new Entry(() => new IntersectionNode()),
			new Entry(() => new SubtractionNode(null)),
			new Entry(() => new LocalTransformNode())
		};
		private static Entry GetEntry(string typeName)
		{
			try
			{
				return entries.First(e => e.Name == typeName);
			}
			catch (InvalidOperationException)
			{
				throw new ArgumentException("No entry found for node '" + typeName +
											    "'! Did you forget to make an Entry?");
			}
		}
		private static Entry GetEntry(Node node)
		{
			try
			{
				var type = node.GetType();
				return entries.First(e => e.Type == type);
			}
			catch (InvalidOperationException)
			{
				throw new ArgumentException("No entry found for node '" + node.GetType().Name +
											    "'! Did you forget to make an Entry?");
			}
		}
	}
}