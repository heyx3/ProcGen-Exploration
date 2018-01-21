using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace LSystem.Editor
{
	public class LSystemEditor : EditorWindow
	{
		[MenuItem("DFLS/Show Editor")]
		public static void ShowEditor()
		{
			GetWindow<LSystemEditor>().Show();
		}


		private LState initialState = new LState("F", new List<Rule>() { new Rule('F', "F+F+F") },
												 new PrioritySystem_Random());
		private List<DFSystem.Command> commands = new List<DFSystem.Command>()
		{
			new DFSystem.Command('F', new DFSystem.BoxNode(0.5f)),
			new DFSystem.Command('+', new DFSystem.LocalTransformNode(Vector3.one,
																	  Quaternion.identity,
																	  Vector3.one)),
		};
		private int nIterations = 4;


		private void Awake()
		{
			titleContent = new GUIContent("L-System");
		}
		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Seed");
			initialState.Value = GUILayout.TextField(initialState.Value, GUILayout.MinWidth(50.0f));
			GUILayout.EndHorizontal();

			GUILayout.Space(10.0f);

			GUILayout.Label("Rules");
			for (int i = 0; i < initialState.Rules.Count; ++i)
			{
				GUILayout.BeginHorizontal();

				string newStr = GUILayout.TextField(initialState.Rules[i].Trigger.ToString(),
													GUILayout.MinWidth(50.0f));
				if (newStr.Length == 1)
				{
					initialState.Rules[i] = new LSystem.Rule(newStr[0],
															 initialState.Rules[i].ReplaceWith);
				}

				newStr = GUILayout.TextField(initialState.Rules[i].ReplaceWith,
											 GUILayout.MinWidth(120.0f));
				initialState.Rules[i] = new LSystem.Rule(initialState.Rules[i].Trigger, newStr);

				if (GUILayout.Button("Delete"))
				{
					initialState.Rules.RemoveAt(i);
					i -= 1;
				}

				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add"))
			{
				initialState.Rules.Add(new LSystem.Rule('F', "F"));
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(10.0f);

			if (GUILayout.Button("Edit Commands"))
			{
				var wnd = GetWindow<DFSystem.Editor.CommandEditor>();
				wnd.Commands = commands;
				wnd.Show();
			}

			GUILayout.Space(15.0f);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Save"))
			{
				var filePath = EditorUtility.SaveFilePanel("Choose where to save",
														   Path.Combine(Application.dataPath, "../"),
														   "MySystem", "lsystem");
				if (filePath != "")
				{
					var errMsg = LSystem.Serializer.ToFile(initialState, filePath);
					if (errMsg != "")
						Debug.LogError("Error saving to " + filePath + ": " + errMsg);
				}
			}
			if (GUILayout.Button("Load"))
			{
				var filePath = EditorUtility.OpenFilePanel("Choose the file to load",
														   Path.Combine(Application.dataPath, "../"),
														   "lsystem");
				if (filePath != "")
				{
					var errMsg = LSystem.Serializer.FromFile(filePath, out initialState);
					if (errMsg != "")
					{
						Debug.LogError("Error loading from " + filePath + ": " + errMsg);
					}
				}
			}
			if (GUILayout.Button("Clear") &&
				EditorUtility.DisplayDialog("Confirm",
											"Are you sure you want to clear everything?", "Yes"))
			{
				var defaultRule = new List<Rule>() { new Rule('F', "F+F+F") };
				initialState = new LSystem.LState("F", defaultRule,
												  new LSystem.PrioritySystem_Random());
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(15.0f);


			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Make Shader"))
				{
					var filePath = EditorUtility.SaveFilePanelInProject("Save Shader",
																		"MyShader", "shader",
																		"Choose the shader name");
					if (filePath != "")
					{
						var errMsg = MakeShader(filePath);
						if (errMsg == "")
							EditorUtility.RevealInFinder(filePath);
						else
							Debug.LogError("Error generating " + filePath + ": " + errMsg);
					}
				}
				nIterations = EditorGUILayout.IntField("Iterations", nIterations);
			}
		}
		private void OnDestroy()
		{
			GetWindow<DFSystem.Editor.CommandEditor>().Close();
		}

		/// <summary>
		/// Returns an error message, or the empty string if everything is fine.
		/// </summary>
		private string MakeShader(string filePath)
		{
			//Generate the L-system's output.
			var lSysState = initialState;
			for (int i = 0; i < nIterations; ++i)
				lSysState = lSysState.Evaluate();

			//Convert into a distance field function.
			var dfTree = new DFSystem.DFTree(lSysState.Value, commands);
			var shaderText = @"Shader ""DFLS/Shader""
{
	Properties { }
	SubShader
	{
		Tags { ""RenderType""=""Opaque"" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include ""UnityCG.cginc""

			struct appdata
			{
				float4 vertex : POSITION;
				float3 worldPos : TEXCOORD0;
			};
			struct v2f
			{
				float3 worldPos : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata IN)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(IN.vertex);
				o.worldPos = mul(_Object2World, IN.vertex).xyz;
				return o;
			}

" + DFSystem.DFTree.Funcs + "\n" + dfTree.GenerateDistanceFunc("distFunc") + @"

			float4 frag(v2f IN) : SV_Target
			{
				return float4(IN.worldPos, 1.0);
			}

			ENDCG
		}
	}
}
";

			//Create the shader object.
			try
			{
				File.WriteAllText(filePath, shaderText.Replace("\r", "\n"));
			}
			catch (Exception e)
			{
				return "Unable to save " + filePath + ": " + e.Message;
			}

			return "";
		}
	}
}