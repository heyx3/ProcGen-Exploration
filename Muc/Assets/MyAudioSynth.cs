using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Music
{
    public class MyAudioSynth : MonoBehaviour
    {
        [Serializable]
        public struct SynthNote
        {
            public int Frequency;
            public float Amplitude;
            public int Duration;
            
            public SynthNote(Music.Note note, float amplitude, int duration)
            {
                Frequency = note.Frequency;
                Amplitude = amplitude;
                Duration = duration;
            }
            public SynthNote(int frequency, float amplitude, int duration)
            {
                Frequency = frequency;
                Amplitude = amplitude;
                Duration = duration;
            }

            /// <summary>
            /// Gets the sound contribution from this note.
            /// Ignores the "Duration" field.
            /// </summary>
            public float Evaluate(float t)
            {
                return Mathf.Sin(t * Frequency) * Amplitude;
            }
        }

        public static MyAudioSynth Instance { get; private set; }


        public List<SynthNote> Notes = new List<SynthNote>();

        private volatile int samplesPerSecond;
        private object lock_notes = new object();


        private void Awake()
        {
            Instance = this;
        }
        private void OnDestroy()
        {
            Instance = null;
        }
        private void Update()
        {
            samplesPerSecond = AudioSettings.outputSampleRate;

            //Remove any notes that have expired.
            lock (lock_notes)
            {
                for (int i = 0; i < Notes.Count; ++i)
                {
                    //Remove expired notes.
                    Notes[i] = new SynthNote(Notes[i].Frequency, Notes[i].Amplitude,
                                             Notes[i].Duration - Mathf.RoundToInt(Time.deltaTime *
                                                                                  1000.0f));
                    if (Notes[i].Duration <= 0)
                    {
                        Notes.RemoveAt(i);
                        i -= 1;
                    }
                }
            }
        }

        private int totalSamples = 0;
        private void OnAudioFilterRead(float[] data, int nChannels)
        {
            lock (lock_notes)
            {
                double sineScale = Math.PI * 2.0 / samplesPerSecond;
                for (int i = 0; i < data.Length; i += nChannels)
                {
                    data[i] = 0.0f;

                    foreach (var note in Notes)
                    {
                        data[i] += note.Amplitude *
                                   (float)Math.Sin((totalSamples + i) * sineScale * note.Frequency);

                        //Copy the data to the other channels.
                        for (int j = 1; j < nChannels; ++j)
                            data[i + j] = data[i];
                    }
                }

                totalSamples += data.Length;
            }
        }
    }
}
