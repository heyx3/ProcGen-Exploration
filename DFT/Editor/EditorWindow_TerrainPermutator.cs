using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace DFT.Editor
{
    /// <summary>
    /// Allows you to create heightmaps similar to a given heightmap by messing with the DFT.
    /// </summary>
    public class EditorWindow_TerrainPermutator : EditorWindow
    {
        [MenuItem("DFT/Terrain")]
        public static void ShowWindow()
        {
            GetWindow<EditorWindow_TerrainPermutator>().Show();
        }


        private Texture2D tex_OriginalTerrain = null,
                          tex_NewTerrain = null;
        private static void Init(ref Texture2D texVariable)
        {
            if (texVariable == null)
            {
                texVariable = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
                texVariable.filterMode = FilterMode.Point;
                texVariable.wrapMode = TextureWrapMode.Clamp;
            }
        }


        /// <summary>
        /// Returns null if no terrain is selected.
        /// </summary>
        private Terrain GetSelectedTerrain()
        {
            //Return the first available terrain in the selection.
            return Selection.gameObjects.Select(go => go.GetComponentInChildren<Terrain>())
                                        .FirstOrDefault(t => t != null);
        }

        private void OnGUI()
        {
            //Buttons to get initial heightmap.
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load heightmap from file"))
            {
                string path = EditorUtility.OpenFilePanelWithFilters(
                                      "Choose the image file", Application.dataPath,
                                      new string[] { "PNG", "png", "JPEG", "jpg", "JPEG", "jpeg" });
                if (path.Length > 0)
                {
                    Init(ref tex_OriginalTerrain);
                    tex_OriginalTerrain.LoadImage(File.ReadAllBytes(path));
                }
            }
            if (GUILayout.Button("Set Heightmap from Selected Terrain"))
            {
                Terrain selected = GetSelectedTerrain();
                if (selected == null)
                {
                    Debug.LogError("No terrain is selected!");
                }
                else
                {
                    var heightmap = selected.terrainData.GetHeights(0, 0,
                                                                    selected.terrainData.heightmapWidth,
                                                                    selected.terrainData.heightmapHeight);

                    Init(ref tex_OriginalTerrain);
                    tex_OriginalTerrain.Resize(selected.terrainData.heightmapWidth,
                                               selected.terrainData.heightmapHeight);
                    tex_OriginalTerrain.Filter((x, y, col, cols) =>
                    {
                        float h = heightmap[x, y];
                        return new Color(h, h, h, 1.0f);
                    });
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(15.0f);

            if (tex_OriginalTerrain != null)
            {
                var rect = GUILayoutUtility.GetRect(tex_OriginalTerrain.width,
                                                    tex_OriginalTerrain.height);
                GUI.DrawTexture(rect, tex_OriginalTerrain);
            }

            GUILayout.Space(35.0f);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Randomize Amplitude"))
                {
                    //TODO: Randomize amplitude.
                }
                if (GUILayout.Button("Randomize phase"))
                {
                    //TODO: Randomize phase.
                }
                if (GUILayout.Button("Reset"))
                {
                    tex_OriginalTerrain.CopyTo(tex_OriginalTerrain);
                }
            }

            if (tex_NewTerrain != null)
            {
                var rect = GUILayoutUtility.GetRect(tex_NewTerrain.width,
                                                    tex_NewTerrain.height);
                GUI.DrawTexture(rect, tex_NewTerrain);
            }
        }
    }
}
