using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


public class LSystemEditor : EditorWindow
{
	[MenuItem("DFLS/Show Editor")]
	public static void ShowEditor()
	{
		LSystemEditor window = EditorWindow.GetWindow<LSystemEditor>();
		window.TryInit();
	}


	private bool init = false;
	private void TryInit()
	{
		if (!init)
		{
			init = true;

			//Initialize the window.
		}
	}

	LSystem.LState InitialState =
        new LSystem.LState("F",
                           new List<LSystem.Rule>() { new LSystem.Rule('F', "F+F+F") },
	                       new LSystem.PrioritySystem_Random());


	void OnGUI()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Seed");
		InitialState.Value = GUILayout.TextField(InitialState.Value);
		GUILayout.EndHorizontal();

		GUILayout.Space(10.0f);

		GUILayout.Label("Rules");
		for (int i = 0; i < InitialState.Rules.Count; ++i)
		{
			GUILayout.BeginHorizontal();

			string newStr = GUILayout.TextField(InitialState.Rules[i].Trigger.ToString());
			if (newStr.Length == 1)
			{
				InitialState.Rules[i] = new LSystem.Rule(newStr[0], InitialState.Rules[i].ReplaceWith);
			}

			newStr = GUILayout.TextField(InitialState.Rules[i].ReplaceWith);
			InitialState.Rules[i] = new LSystem.Rule(InitialState.Rules[i].Trigger, newStr);

			if (GUILayout.Button("Delete"))
			{
				InitialState.Rules.RemoveAt(i);
				i -= 1;
			}

			GUILayout.EndHorizontal();
		}
		if (GUILayout.Button("Add"))
		{
			InitialState.Rules.Add(new LSystem.Rule('F', "F"));
		}

		GUILayout.Space(10.0f);

		if (GUILayout.Button("Edit Commands"))
		{
			//TODO: Show command editor.
		}

        GUILayout.Space(15.0f);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
        {
            var filePath = EditorUtility.SaveFilePanel("Choose where to save",
                                                       Path.Combine(Application.dataPath, "../"),
                                                       "MySystem",
                                                       "lsystem");
            if (filePath != "")
            {
                var errMsg = LSystem.Serializer.ToFile(InitialState, filePath);
                if (errMsg != "")
                {
                    Debug.LogError("Error saving to " + filePath + ": " + errMsg);
                }
            }
        }
        if (GUILayout.Button("Load"))
        {
            var filePath = EditorUtility.OpenFilePanel("Choose the file to load",
                                                       Path.Combine(Application.dataPath, "../"),
                                                       "lsystem");
            if (filePath != "")
            {
                var errMsg = LSystem.Serializer.FromFile(filePath, out InitialState);
                if (errMsg != "")
                {
                    Debug.LogError("Error loading from " + filePath + ": " + errMsg);
                }
            }
        }
        if (GUILayout.Button("Clear") &&
            EditorUtility.DisplayDialog("Confirm", "Are you sure you want to clear everything?", "Yes"))
        {
            InitialState = new LSystem.LState("F",
                                              new List<LSystem.Rule>() { new LSystem.Rule('F', "F+F+F") },
                                              new LSystem.PrioritySystem_Random());
        }
        GUILayout.EndHorizontal();
	}
}