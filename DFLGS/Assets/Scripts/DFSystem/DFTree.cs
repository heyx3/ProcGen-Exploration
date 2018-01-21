using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace DFSystem
{
	/// <summary>
	/// An signed distance-field, represented as a tree of expressions.
	/// </summary>
	public struct DFTree
	{
		/// <summary>
		/// Used to determine if two Matrices are equal.
		/// Larger values yields smaller memory footprint but less precision.
		/// </summary>
		public const float Epsilon = 0.001f;

		#region Distance Field function definitions
		public static readonly string Funcs = @"
//smin() is a version of min() that gives smooth results when used in distance field functions.
float smin(float d1, float d2, float k)
{
    //Source: http://iquilezles.org/www/articles/smin/smin.htm
    float h = saturate(0.5 + (0.5 * (d1 - d2) / k));
    return lerp(b, a, h) - (k * h * (1.0 - h));
}

//Below are the distance functions for basic shapes.
//These are all signed distance functions.
//Source: http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
float distSphere(float3 point, float radius)
{
	return length(point) - radius;
}
float distBox(float3 point, float sideLength)
{
	float3 dist = abs(point) - sideLength.xxx;
	return min(max(dist.x, max(dist.y, dist.z)),
			   0.0) +
		   length(max(d, 0.0));
}
float distPlane(float3 point)
{
	return point.y;
}
float distEllipsoid(float3 point, float3 radius)
{
	float smallestRadius = min(min(radius.x, radius.y), radius.z);
	return smallestRadius * (length(point / radius) - 1.0)
}
float distTorus(float3 point, float largeRadius, float smallRadius)
{
	float2 cylinderPos = float2(length(point.xz) - largeRadius, point.y);
	return length(cylinderPos) - smallRadius;
}
float distCone(float3 point, float2 wtf)
{
	//TODO: How tf is the cone defined?
	float q = length(point.xy);
	return dot(wtf, float2(q, point.z));
}
//TODO: Add the other smin types.
//TODO: Capsule, cylinder
//TODO: Repetition function.
";
		#endregion

		public Node Root;

		public DFTree(Node root) { Root = root; }

		public string GenerateDistanceFunc(string funcName)
		{
			//Give each node a unique ID.
			var nodeIDs = new Dictionary<Node, uint>();
			{
				uint nextID = 0;
				Stack<Node> nodesToCheck = new Stack<Node>();
				nodesToCheck.Push(Root);
				while (nodesToCheck.Count > 0)
				{
					var node = nodesToCheck.Pop();

					if (nodeIDs.ContainsKey(node))
						throw new Exception("Node occurs in the tree more than once!");
					nodeIDs.Add(node, nextID);
					nextID += 1;

					foreach (var childNode in node.Inputs)
						nodesToCheck.Push(childNode);
				}
			}

			//Build the transform hierarchy.
			//While we're at it, create a list of all nodes ordered by depth, for later.
			var transforms = new List<Matrix4x4>(nodeIDs.Count);
			var nodeTransformIDs = new Dictionary<Node, int>(nodeIDs.Count);
			var nodesInAscendingDepth = new List<Node>(nodeIDs.Count);
			{
				//Starting from the root and moving breadth-first,
				//    get each node's world-space transform.
				//Don't add a node's transform to the list if an identical one already exists.
				Stack<Node> currentLayer = new Stack<Node>(),
							nextLayer = new Stack<Node>();
				var nodeParentTransforms = new Dictionary<Node, Matrix4x4>(nodeIDs.Count);
				currentLayer.Push(Root);
				nodeParentTransforms.Add(Root, Matrix4x4.identity);
				while (currentLayer.Count > 0)
				{
					//Process the current layer, then swap in the next layer as the "current" layer.
					while (currentLayer.Count > 0)
					{
						var node = currentLayer.Pop();
						nodesInAscendingDepth.Add(node);

						//Calculate this node's transform.
						var parentTransorm = nodeParentTransforms[node];
						var nodeWorldTransform = node.Transform() * parentTransorm;

						//Find it in the list of transforms, or add it if it doesn't already exist.
						int nodeTrIndex = IndexOf(nodeWorldTransform, transforms);
						if (nodeTrIndex == -1)
						{
							transforms.Add(nodeWorldTransform);
							nodeTrIndex = transforms.Count - 1;
						}

						nodeTransformIDs.Add(node, nodeTrIndex);

						//Add this node's children to be processed.
						foreach (var childNode in node.Inputs)
							nextLayer.Push(childNode);
					}

					//Swap in the next layer. Reuse the "current layer" list as the next next layer.
					var temp = currentLayer;
					currentLayer = nextLayer;
					nextLayer = temp;
				}
			}


			//Now it's time to actually write the shader code.
			StringBuilder outCode = new StringBuilder();
			outCode.Append("float ");
			outCode.Append(funcName);
			outCode.AppendLine("(float3 inputPos)");
			outCode.AppendLine("{");

			//Generate the transformed positions using hard-coded matrices.
			outCode.AppendLine("\tfloat4 pos4d;");
			outCode.AppendLine("\tfloat4 inputPos4d = float4(inputPos, 1.0);");
			for (int i = 0; i < transforms.Count; ++i)
			{
				outCode.AppendLine("\tpos4d = mul(");
				GenerateMat(outCode, transforms[i]);
				outCode.AppendLine(",");
				outCode.AppendLine("\t\t\t   inputPos4d);");
				outCode.Append("\tfloat3 pos");
				outCode.Append(i);
				outCode.AppendLine(" = pos4d.xyz / pos4d.w;");
			}

			//From the deepest nodes to the root, write out each node's expression.
			for (int i = nodesInAscendingDepth.Count - 1; i >= 0; --i)
			{
				var node = nodesInAscendingDepth[i];

				outCode.Append("\tfloat dist");
				outCode.Append(nodeIDs[node]);
				outCode.AppendLine(";");
				outCode.AppendLine("\t{");
			
				outCode.Append('\t');
				node.EmitVariableDef(outCode,
									 "pos" + nodeTransformIDs[node], "dist",
									 nodeIDs[node], nodeIDs);
				outCode.AppendLine();

				outCode.AppendLine("\t}");
			}

			//Finish the function.
			outCode.Append("\treturn dist");
			outCode.Append(nodeIDs[Root]);
			outCode.AppendLine(";");
			outCode.Append('}');

			return outCode.ToString();
		}
		private static int IndexOf(Matrix4x4 mat, List<Matrix4x4> mats)
		{
			for (int i = 0; i < mats.Count; ++i)
			{
				bool equal = true;

				for (int row = 0; row < 4; ++row)
				{
					if (!equal)
						break;

					for (int col = 0; col < 4; ++col)
					{
						if (Mathf.Abs(mat[row, col] - mats[i][row, col]) > Epsilon)
						{
							equal = false;
							break;
						}
					}
				}

				if (equal)
					return i;
			}

			return -1;
		}
		private void GenerateMat(StringBuilder outCode, Matrix4x4 mat)
		{
			outCode.AppendLine("float4x4(");
			for (int row = 0; row < 4; ++row)
			{
				for (int col = 0; col < 4; ++col)
				{
					outCode.Append(mat[row, col]);
					if (col + row < 6)
						outCode.Append(", ");
				}

				if (row < 3)
					outCode.AppendLine();
			}
			outCode.Append(')');
		}
	}
}
