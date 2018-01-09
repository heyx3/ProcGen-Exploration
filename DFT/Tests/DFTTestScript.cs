using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace DFT
{
    public class DFTTestScript : MonoBehaviour
    {
        public AnimationCurve TimeSignal = new AnimationCurve(new Keyframe(0.0f, 0.0f, 0.0f, 0.0f),
                                                              new Keyframe(0.2f, 0.4f, 5.0f, 5.0f),
                                                              new Keyframe(0.6f, 0.2f, 0.0f, 0.0f),
                                                              new Keyframe(1.0f, 1.0f, 0.0f, 0.0f)),
                              SampledTimeSignal = new AnimationCurve(),
                              SinFrequencySignal = new AnimationCurve(),
                              CosFrequencySignal = new AnimationCurve(),
                              ReconstructedTimeSignal = new AnimationCurve();


        public static float[] GetDiscreteTimeSignal(AnimationCurve continuousSignal,
                                                    int nSamples, float endTime = 1.0f)
        {
            if (nSamples < 2)
                throw new ArgumentException("nSamples must be > 1");

            float[] samples = new float[nSamples];
            float increment = endTime / (nSamples - 1);

            for (int sampleI = 0; sampleI < nSamples; ++sampleI)
                samples[sampleI] = continuousSignal.Evaluate(sampleI * increment);

            return samples;
        }

        public void CloneCurve(ref AnimationCurve myCurve)
        {
            var keys = myCurve.keys;
            myCurve = new AnimationCurve(keys.ToArray());
        }
    }
}
