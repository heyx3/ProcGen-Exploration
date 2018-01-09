using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Music
{
    /// <summary>
    /// The 7 notes in a single octave of a scale.
    /// </summary>
    public struct Scale
    {
        public static Scale Major(Note tonic)
        {
            return new Scale(tonic, tonic.MoveSteps(2), tonic.MoveSteps(4),
                             tonic.MoveSteps(5), tonic.MoveSteps(7), tonic.MoveSteps(9),
                             tonic.MoveSteps(11));
        }
        public static Scale NaturalMinor(Note tonic)
        {
            return new Scale(tonic, tonic.MoveSteps(2), tonic.MoveSteps(3),
                             tonic.MoveSteps(5), tonic.MoveSteps(7), tonic.MoveSteps(8),
                             tonic.MoveSteps(10));
        }
        public static Scale HarmonicMinor(Note tonic)
        {
            return new Scale(tonic, tonic.MoveSteps(2), tonic.MoveSteps(3),
                             tonic.MoveSteps(5), tonic.MoveSteps(7), tonic.MoveSteps(8),
                             tonic.MoveSteps(11));
        }
        public static Scale JazzMinor(Note tonic)
        {
            return new Scale(tonic, tonic.MoveSteps(2), tonic.MoveSteps(3),
                             tonic.MoveSteps(5), tonic.MoveSteps(7), tonic.MoveSteps(9),
                             tonic.MoveSteps(11));
        }


        public const int NumbNotes = 7;


        public Note Tonic, Supertonic, Mediant, Subdominant,
                    Dominant, Submediant, Subtonic;


        /// <summary>
        /// Gets a note of this scale using a 0-based index.
        /// You can provide values below 0 or above 6 to get the scale notes in different octaves.
        /// </summary>
        public Note this[int i]
        {
            get
            {
                //Get the octave relative to this scale's octave.
                //Found this snippet on StackOverflow -- integer division in a way that rounds to -inf.
                int relativeOctave = (i >= 0) ?
                                         (i / NumbNotes) :
                                         ((i - NumbNotes + 1) / NumbNotes);
                //Get the tone in the scale.
                //I also found THIS snippet on StackOverflow, for handling negative values correctly.
                int _tone = i % NumbNotes,
                    tone = (_tone < 0) ?
                               (_tone + NumbNotes) :
                               _tone;

                var newScale = ChangeOctave((sbyte)relativeOctave);

                switch (tone)
                {
                    case 0: return newScale.Tonic;
                    case 1: return newScale.Supertonic;
                    case 2: return newScale.Mediant;
                    case 3: return newScale.Subdominant;
                    case 4: return newScale.Dominant;
                    case 5: return newScale.Submediant;
                    case 6: return newScale.Subtonic;
                    default: throw new Exception(tone + " not between 0 and " + NumbNotes);
                }
            }
        }


        public Scale(Note tonic, Note supertonic, Note mediant, Note subdominant,
                     Note dominant, Note submediant, Note subtonic)
        {
            Tonic = tonic;
            Supertonic = supertonic;
            Mediant = mediant;
            Subdominant = subdominant;
            Dominant = dominant;
            Submediant = submediant;
            Subtonic = subtonic;
        }


        public Scale ChangeOctave(sbyte octaveChange)
        {
            return new Scale(Tonic.MoveOctave(octaveChange),
                             Supertonic.MoveOctave(octaveChange),
                             Mediant.MoveOctave(octaveChange),
                             Subdominant.MoveOctave(octaveChange),
                             Dominant.MoveOctave(octaveChange),
                             Submediant.MoveOctave(octaveChange),
                             Subtonic.MoveOctave(octaveChange));
        }
        public Scale Transpose(Note key)
        {
            int shift = key.NHalfSteps - Tonic.NHalfSteps;
            return new Scale(Tonic.MoveSteps(shift), Supertonic.MoveSteps(shift),
                             Mediant.MoveSteps(shift), Subdominant.MoveSteps(shift),
                             Dominant.MoveSteps(shift), Submediant.MoveSteps(shift),
                             Subtonic.MoveSteps(shift));
        }

        public override string ToString()
        {
            return "[" + Tonic + " " + Supertonic + " " + Mediant + " " + Subdominant + " " + Dominant + " " + Submediant + " " + Subtonic + "]";
        }
        //TODO: Equals, Equals, GetHashCode, ==, !=
    }
}
