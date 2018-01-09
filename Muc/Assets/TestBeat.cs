using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Music
{
    public class TestBeat : MonoBehaviour
    {
        public float Amplitude = 0.1f;


        private System.Collections.IEnumerator Start()
        {
            const int NoteLengths = 500;

            //Play inverted triads.
            var triad = Music.Triad.Diminished(new Note(NoteValues.F, 2));
            while (false)
            {
                MyAudioSynth.Instance.Notes.Add(new MyAudioSynth.SynthNote(triad.First.Frequency,
                                                                           Amplitude, NoteLengths));
                MyAudioSynth.Instance.Notes.Add(new MyAudioSynth.SynthNote(triad.Second.Frequency,
                                                                           Amplitude, NoteLengths));
                MyAudioSynth.Instance.Notes.Add(new MyAudioSynth.SynthNote(triad.Third.Frequency,
                                                                           Amplitude, NoteLengths));

                triad = triad.UpInversion.AfterInversion;

                yield return new WaitForSeconds(NoteLengths / 1000.0f);
            }

            //Move up through a scale, forever.
            const int IntersectionLengths = 150;
            var scale = Scale.Major(new Note(NoteValues.FSharp, 3));
            int i = 0;
            while (true)
            {
                var note = scale[i];
                i += 1;

                MyAudioSynth.Instance.Notes.Add(new MyAudioSynth.SynthNote(note.Frequency,
                                                                           Amplitude,
                                                                           NoteLengths));
                yield return new WaitForSeconds((NoteLengths - IntersectionLengths) / 1000.0f);
            }
        }
    }
}
