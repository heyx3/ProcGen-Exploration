using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace DFT.Editor
{
    public class EditorWindow_TextureFilterer : EditorWindow
    {
        [MenuItem("DFT/Texture")]
        public static void ShowWindow()
        {
            GetWindow<EditorWindow_TextureFilterer>().Show();
        }


        public Texture2D ForwardDFT_Src, ForwardDFT_Dest,
                         InverseDFT_Src, InverseDFT_Dest;

        public bool ShowSection_Forward = true,
                    ShowSection_Inverse = false;

        public float ScaleVal = 100.0f;
        public Vector2 ScrollPos = Vector2.zero;


        private static void InitDisplayTex(ref Texture2D tex)
        {
            if (tex == null)
            {
                tex = new Texture2D(1, 1, TextureFormat.RGBAFloat, false, true);
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;
            }
        }

        private void OnGUI()
        {
            ScrollPos = GUILayout.BeginScrollView(ScrollPos);

            ShowSection_Forward = EditorGUILayout.Foldout(ShowSection_Forward, "Forward DFT");
            if (ShowSection_Forward)
            {
                if (GUILayout.Button("Load Source Image"))
                {
                    string path = EditorUtility.OpenFilePanelWithFilters(
                                      "Choose the image file", Application.dataPath,
                                      new string[] { "PNG", "png", "JPEG", "jpg", "JPEG", "jpeg" });
                    if (path.Length > 0)
                    {
                        InitDisplayTex(ref ForwardDFT_Src);

                        ForwardDFT_Src.LoadImage(File.ReadAllBytes(path));
                        ForwardDFT_Src.Apply(false, false);
                    }
                }
                if (ForwardDFT_Src != null)
                {
                    var texPos = GUILayoutUtility.GetRect(Math.Min(512, ForwardDFT_Src.width),
                                                          Math.Min(512, ForwardDFT_Src.height));
                    GUI.DrawTexture(texPos, ForwardDFT_Src, ScaleMode.ScaleToFit);

                    GUILayout.Space(15.0f);
                    
                    if (GUILayout.Button("Run Forward DFT"))
                    {
                        var samples = Algo2D.ToSamples(ForwardDFT_Src);
                        //var dftResults = Algo2D.Forward(samples);

                        var dftResults = new Complex[samples.GetLength(0), samples.GetLength(1)];
                        DFT_GPU.Forward((uint)samples.GetLength(0), (uint)samples.GetLength(1),
                                        (x, y) => samples[x, y],
                                        (x, y, c) => dftResults[x, y] = c);

                        InitDisplayTex(ref ForwardDFT_Dest);
                        Algo2D.ToTex(dftResults, ForwardDFT_Dest);
                    }
                    if (ForwardDFT_Dest != null)
                    {
                        texPos = GUILayoutUtility.GetRect(Math.Min(256, ForwardDFT_Dest.width),
                                                          Math.Min(256, ForwardDFT_Dest.height));
                        GUI.DrawTexture(texPos, ForwardDFT_Dest, ScaleMode.ScaleToFit);

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Shift"))
                        {
                            ForwardDFT_Dest.Filter(
                                      (x, y, c, pixels) =>
                                      {
                                          x = (x + (ForwardDFT_Dest.width / 2)) % ForwardDFT_Dest.width;
                                          y = (y + (ForwardDFT_Dest.height / 2)) % ForwardDFT_Dest.height;
                                          return pixels[x + (y * ForwardDFT_Dest.width)];
                                      });
                        }
                        if (GUILayout.Button("Map to [0, 1]"))
                        {
                            //A continuous 1:1 mapping from (-inf, +inf) to (0, 1) is:
                            //    1/(1+e^(-x))
                            ForwardDFT_Dest.Filter(f => (1.0f / (1.0f + Mathf.Exp(-f))));
                        }
                        if (GUILayout.Button("Map to [-inf, +inf]"))
                        {
                            //The inverse of the above mapping is:
                            //    ln(-x/(x-1))
                            ForwardDFT_Dest.Filter(f => Mathf.Log(-f / (f - 1)));
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        ScaleVal = EditorGUILayout.FloatField("Log Scale: ", ScaleVal);
                        if (GUILayout.Button("Forward"))
                        {
                            ForwardDFT_Dest.Filter(f => (ScaleVal * Mathf.Log(f + 1)));
                        }
                        if (GUILayout.Button("Invert"))
                        {
                            //Use the inverse of the "forward" scale.
                            ForwardDFT_Dest.Filter(f => (Mathf.Exp(f / ScaleVal) - 1.0f));
                        }
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(15.0f);

                    if (ForwardDFT_Dest != null)
                    {
                        if (GUILayout.Button("Save Forward DFT"))
                        {
                            string path = EditorUtility.SaveFilePanel("Choose the location",
                                                                      Application.dataPath,
                                                                      "DFT", "png");
                            if (path.Length > 0)
                                File.WriteAllBytes(path, ForwardDFT_Dest.EncodeToPNG());
                        }
                    }
                }
            }

            GUILayout.Space(30.0f);

            ShowSection_Inverse = EditorGUILayout.Foldout(ShowSection_Inverse, "Inverse DFT");
            if (ShowSection_Inverse)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Load DFT Img"))
                {
                    string path = EditorUtility.OpenFilePanelWithFilters(
                                      "Choose the DFT image file", Application.dataPath,
                                      new string[] { "JPEG", "jpeg", "JPEG", "jpg", "PNG", "png" });
                    if (path.Length > 0)
                    {
                        InitDisplayTex(ref InverseDFT_Src);

                        InverseDFT_Src.LoadImage(File.ReadAllBytes(path));
                        InverseDFT_Src.Apply(false, false);
                    }
                }
                if (ForwardDFT_Dest != null && GUILayout.Button("Copy DFT Img"))
                {
                    InitDisplayTex(ref InverseDFT_Src);
                    ForwardDFT_Dest.CopyTo(InverseDFT_Src);
                }
                GUILayout.EndHorizontal();

                if (InverseDFT_Src != null)
                {
                    var texPos = GUILayoutUtility.GetRect(Math.Min(256, InverseDFT_Src.width),
                                                          Math.Min(256, InverseDFT_Src.height));
                    GUI.DrawTexture(texPos, InverseDFT_Src, ScaleMode.ScaleToFit);

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Map to [0, 1]"))
                    {
                        //A continuous 1:1 mapping from (-inf, +inf) to (0, 1) is:
                        //    1/(1+e^(-x))
                        InverseDFT_Src.Filter(f => (1.0f / (1.0f + Mathf.Exp(-f))));
                    }
                    if (GUILayout.Button("Map to [-inf, +inf]"))
                    {
                        //The inverse of the above mapping is:
                        //    ln(-x/(x-1))
                        InverseDFT_Src.Filter(f => Mathf.Log(-f / (f - 1)));
                    }
                    GUILayout.EndHorizontal();


                    GUILayout.Space(15.0f);

                    if (GUILayout.Button("Run Inverse DFT"))
                    {
                        var samples = Algo2D.ToSamples(InverseDFT_Src);
                        var results = Algo2D.Inverse(samples);

                        InitDisplayTex(ref InverseDFT_Dest);
                        Algo2D.ToTex(results, InverseDFT_Dest);
                    }
                    if (InverseDFT_Dest != null)
                    {
                        texPos = GUILayoutUtility.GetRect(Math.Min(512, InverseDFT_Dest.width),
                                                          Math.Min(512, InverseDFT_Dest.height));
                        GUI.DrawTexture(texPos, InverseDFT_Dest, ScaleMode.ScaleToFit);
                    }

                    GUILayout.Space(15.0f);

                    if (InverseDFT_Dest != null)
                    {
                        if (GUILayout.Button("Save Inverse DFT"))
                        {
                            string path = EditorUtility.SaveFilePanel("Choose the location",
                                                                      Application.dataPath,
                                                                      "DFT", "png");
                            if (path.Length > 0)
                                File.WriteAllBytes(path, InverseDFT_Dest.EncodeToPNG());
                        }
                    }
                }
            }

            GUILayout.EndScrollView();
        }
    }
}
