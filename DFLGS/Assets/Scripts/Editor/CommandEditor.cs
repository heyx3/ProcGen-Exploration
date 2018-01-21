using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace DFSystem.Editor
{
	public class CommandEditor : EditorWindow
	{
		public List<Command> Commands;

		private List<string> nodeTypes;
		private Vector2 scrollPos = Vector2.zero;


		public void ResetList()
		{
			Commands.Clear();
			Commands.Add(new Command('F', new BoxNode(0.5f)));
			Commands.Add(new Command('+', new LocalTransformNode(Vector3.one,
																 Quaternion.identity,
																 Vector3.one)));
		}

		/// <summary>
		/// Returns an error message, or "null" if the file was saved correctly.
		/// </summary>
		public string SaveToFile(string filePath)
		{
			try
			{
				using (var fileStream = File.Open(filePath, FileMode.Create))
				using (var writer = new BinaryWriter(fileStream))
				{
					writer.Write(Commands.Count);
					foreach (var command in Commands)
						command.Serialize(writer);
				}
			}
			catch (Exception e)
			{
				return e.Message;
			}

			return "";
		}
		/// <summary>
		/// Returns an error message, or "null" if the file was saved correctly.
		/// </summary>
		public string LoadFromFile(string filePath)
		{
			try
			{
				using (var fileStream = File.Open(filePath, FileMode.Open))
				using (var reader = new BinaryReader(fileStream))
				{
					int nElements = reader.ReadInt32();
					Commands.Clear();
					for (int i = 0; i < nElements; ++i)
					{
						var command = new Command();
						command.Deserialize(reader);
						Commands.Add(command);
					}
				}
			}
			catch (Exception e)
			{
				return e.Message;
			}

			return "";
		}

		private void Awake()
		{
			titleContent = new GUIContent("Commands");
			nodeTypes = NodeSerialization.Options.ToList();
		}
		private void OnGUI()
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos);
			{
				for (int i = 0; i < Commands.Count; ++i)
				{
					if (i > 0)
						GUILayout.Space(5.0f);

					var command = Commands[i];
					GUILayout.BeginHorizontal();
					{
						//Convert the command char(s) into a single string.
						string str = command.StartChar.ToString();
						if (command.EndChar.HasValue)
							str += command.EndChar.Value;

						//Show the editor.
						str = GUILayout.TextField(str);

						//Parse the string into command char(s).
						if (str.Length > 2)
							str = str[0].ToString() + str[2];
						if (str.Length > 0)
						{
							command.StartChar = str[0];
							if (str.Length == 2)
								command.EndChar = str[1];
							else
								command.EndChar = null;
						}

						GUILayout.Space(7.5f);

						//Select the node type.
						EditorGUI.BeginChangeCheck();
						var nodeTypesArr = nodeTypes.Select(n => n.Replace("Node", "")).ToArray();
						var currentStr = NodeSerialization.GetTypeName(command.Node);
						int strI = Math.Max(IndexOf(currentStr), 0);
						strI = EditorGUILayout.Popup(strI, nodeTypesArr);
						if (EditorGUI.EndChangeCheck())
							command.Node = NodeSerialization.MakeNode(nodeTypes[strI]);

						if (GUILayout.Button("-"))
						{
							Commands.RemoveAt(i);
							i -= 1;
							GUILayout.EndHorizontal();
							continue;
						}
					}
					GUILayout.EndHorizontal();

					//Tab in and show the editor for the selected node.
					GUILayout.BeginHorizontal();
					GUILayout.Space(20.0f);
					GUILayout.BeginVertical();
					{
						command.Node.EditorGUI();
					}
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();

					GUILayout.Space(15.0f);
				}
			}
			GUILayout.EndScrollView();

			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Add"))
					Commands.Add(new Command('A', new BoxNode(0.5f)));
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10.0f);

			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Save"))
				{
					var path = EditorUtility.SaveFilePanel("Choose where to save",
														   Path.Combine(Application.dataPath, "../"),
														   "MyCommands", "commands");
					if (path != "")
					{
						var errMsg = SaveToFile(path);
						if (errMsg != "")
							Debug.LogError("Error saving to " + path + ": " + errMsg);
					}
				}
				if (GUILayout.Button("Load"))
				{
					var path = EditorUtility.OpenFilePanel("Choose the file to load",
														   Path.Combine(Application.dataPath, "../"),
														   "commands");
					if (path != "")
					{
						var errMsg = LoadFromFile(path);
						if (errMsg != "")
							Debug.LogError("Error loading from " + path + ": " + errMsg);
					}
				}
				if (GUILayout.Button("Clear") &&
					EditorUtility.DisplayDialog("Confirm",
												"Are you sure you want to clear everything?", "Yes"))
				{
					ResetList();
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10.0f);
		}
		private int IndexOf(string index)
		{
			for (int i = 0; i < nodeTypes.Count; ++i)
				if (nodeTypes[i] == index)
					return i;
			return -1;
		}
	}
}