using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace DFT.Editor
{
    [Serializable]
    [CustomEditor(typeof(DFTTestScript))]
    public class CustomInspector_DFTTestScript : UnityEditor.Editor
    {
        public int NSamples = 10;

        [SerializeField]
        private float[] timeSamples = null;
        [SerializeField]
        private Complex[] dftResults = null;
        [SerializeField]
        private bool useGPU = false;


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var script = (DFTTestScript)target;

            GUILayout.Space(20.0f);

            //Button to sample from time signal.
            GUILayout.BeginHorizontal();
            NSamples = EditorGUILayout.IntField(NSamples);
            if (NSamples < 2)
                NSamples = 2;
            if (GUILayout.Button("Sample " + NSamples.ToString() + " points"))
            {
                timeSamples = DFTTestScript.GetDiscreteTimeSignal(script.TimeSignal, NSamples);
                var newKeys = new Keyframe[timeSamples.Length];

                float timeIncrement = 1.0f / (timeSamples.Length - 1);
                for (int sampleI = 0; sampleI < timeSamples.Length; ++sampleI)
                    newKeys[sampleI] = new Keyframe(sampleI * timeIncrement, timeSamples[sampleI]);

                script.SampledTimeSignal.keys = newKeys;
                script.CloneCurve(ref script.SampledTimeSignal);
                Repaint();
            }
            GUILayout.EndHorizontal();

            useGPU = GUILayout.Toggle(useGPU, "Use GPU");

            //Button to perform forward DFT.
            if (GUILayout.Button("Perform Forward DFT"))
            {
                if (useGPU)
                {
                    dftResults = new Complex[timeSamples.Length];
                    string errMsg = DFT_GPU.Forward((uint)timeSamples.Length, 1,
                                                (x, y) => timeSamples[x],
                                                (x, y, c) => dftResults[x] = c);
                    if (errMsg.Length > 0)
                        Debug.LogError(errMsg);
                }
                else
                {
                    Complex[,] _timeSamples = new Complex[timeSamples.Length, 1];
                    for (int i = 0; i < timeSamples.Length; ++i)
                        _timeSamples[i, 0] = new Complex(timeSamples[i], 0.0f);

                    var _dftResults = Algo2D.Forward(_timeSamples);

                    dftResults = new Complex[timeSamples.Length];
                    for (int i = 0; i < timeSamples.Length; ++i)
                        dftResults[i] = _dftResults[i, 0];

                    //dftResults = Algo1D.Forward(timeSamples.Select(f => new Complex(f, 0.0f)).ToArray());
                }

                var newKeys_Cosine = new Keyframe[dftResults.Length];
                var newKeys_Sine = new Keyframe[dftResults.Length];
                
                for (int sampleI = 0; sampleI < dftResults.Length; ++sampleI)
                {
                    newKeys_Cosine[sampleI] = new Keyframe(sampleI, dftResults[sampleI].R);
                    newKeys_Sine[sampleI] = new Keyframe(sampleI, dftResults[sampleI].I);
                }

                script.CosFrequencySignal.keys = newKeys_Cosine;
                script.SinFrequencySignal.keys = newKeys_Sine;
                script.CloneCurve(ref script.CosFrequencySignal);
                script.CloneCurve(ref script.SinFrequencySignal);
            }

            //Button to perform inverse DFT.
            if (GUILayout.Button("Perform inverse DFT"))
            {
                float[] results = null;
                if (useGPU)
                {
                    results = new float[dftResults.Length];
                    string errMsg = DFT_GPU.Inverse((uint)results.Length, 1,
                                                    (x, y) => dftResults[x],
                                                    (x, y, c) => results[x] = c);
                    if (errMsg.Length > 0)
                        Debug.LogError(errMsg);
                }
                else
                {
                    Complex[,] _dftResults = new Complex[dftResults.Length, 1];
                    for (int i = 0; i < dftResults.Length; ++i)
                        _dftResults[i, 0] = dftResults[i];

                    var _results = Algo2D.Inverse(_dftResults);

                    results = new float[dftResults.Length];
                    for (int i = 0; i < results.Length; ++i)
                        results[i] = _results[i, 0].R;

                    //results = Algo1D.Inverse(dftResults).Select(c => c.R).ToArray();
                }

                var newKeys = new Keyframe[results.Length];
                float timeIncrement = 1.0f / (results.Length - 1);
                for (int sampleI = 0; sampleI < results.Length; ++sampleI)
                    newKeys[sampleI] = new Keyframe(sampleI * timeIncrement, results[sampleI]);

                script.ReconstructedTimeSignal.keys = newKeys;
                script.CloneCurve(ref script.ReconstructedTimeSignal);
            }
        }
    }
}
