using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Music
{
    /// <summary>
    /// All the notes in a scale.
    /// Note that duplicate notes (e.x. A# and Bb) are equal.
    /// </summary>
    public enum NoteValues
    {
        A = 0,
        ASharp = 1, BFlat = 1,
        B = 2, CFlat = 2,
        C = 3, BSharp = 3,
        CSharp = 4, DFlat = 4,
        D = 5,
        DSharp = 6, EFlat = 6,
        E = 7, FFlat = 7,
        F = 8, ESharp = 8,
        FSharp = 9, GFlat = 9,
        G = 10,
        GSharp = 11, AFlat = 11,

        /// <summary>
        /// The number of half-steps before the cycle repeats, one "octave" higher.
        /// </summary>
        LENGTH = 12,
    }
    public static partial class Extensions
    {
        /// <summary>
        /// Returns whether this note is an accidental.
        /// </summary>
        /// <param name="includeWeirdAccidentals">
        /// If true, E will be included as "Fb", F will be included as E#, etc.
        /// </param>
        public static bool IsAccidental(this NoteValues note, bool includeWeirdAccidentals)
        {
            switch (note)
            {
                case NoteValues.A:
                case NoteValues.D:
                case NoteValues.G:
                    return false;
                case NoteValues.B:
                case NoteValues.C:
                case NoteValues.E:
                case NoteValues.F:
                    return includeWeirdAccidentals;
                case NoteValues.ASharp:
                case NoteValues.CSharp:
                case NoteValues.DSharp:
                case NoteValues.FSharp:
                case NoteValues.GSharp:
                    return true;

                default: throw new NotImplementedException(note.ToString());
            }
        }
        /// <summary>
        /// Returns whether this note is a "weird" accidental (a.k.a. E#, Fb, B#, Cb).
        /// </summary>]
        public static bool IsWeirdAccidental(this NoteValues note)
        {
            switch (note)
            {
                case NoteValues.B:
                case NoteValues.C:
                case NoteValues.E:
                case NoteValues.F:
                    return true;

                case NoteValues.A:
                case NoteValues.D:
                case NoteValues.G:
                case NoteValues.ASharp:
                case NoteValues.CSharp:
                case NoteValues.DSharp:
                case NoteValues.FSharp:
                case NoteValues.GSharp:
                    return false;

                default: throw new NotImplementedException(note.ToString());
            }
        }
    }

    
    public struct Note : IEquatable<Note>
    {
        /// <summary>
        /// The frequency of A4 (a.k.a. the A just above middle C), in Hz.
        /// Used to tune the entire system.
        /// </summary>
        public static int A4Frequency = 440;
        /// <summary>
        /// A number used when computing the frequency of a note.
        /// </summary>
        public static readonly double Factor = Math.Pow(2.0, 1.0 / 12.0);
        

        public NoteValues Value;
        public sbyte Octave;


        public Note Sharp { get { return MoveSteps(1); } }
        public Note Flat { get { return MoveSteps(-1); } }
        public Note UpOctave { get { return MoveOctave(1); } }
        public Note DownOctave { get { return MoveOctave(-1); } }

        /// <summary>
        /// Gets the minor key with the same key signature as this major key.
        /// May push into a lower octave.
        /// </summary>
        public Note RelativeMinor { get { return MoveSteps(-3); } }
        /// <summary>
        /// Gets the major key with the same key signature as this minor key.
        /// May push into a higher octave.
        /// </summary>
        public Note RelativeMajor { get { return MoveSteps(3); } }

        /// <summary>
        /// The total number of half-steps to go from A0 to this note.
        /// </summary>
        public int NHalfSteps { get { return (Octave * (int)NoteValues.LENGTH) + (int)Value; } }

        public int Frequency
        {
            get
            {
                //Get the number of half-steps we are from the tuning note, A4.
                int nHalfStepsAway = (12 * (Octave - 4)) +              //Octaves
                                     ((int)Value - (int)NoteValues.A);  //Half-steps

                //Calculate frequency using the equation.
                //Refer to http://pages.mtu.edu/~suits/NoteFreqCalcs.html
                return Mathf.RoundToInt((float)(A4Frequency * Math.Pow(Factor, nHalfStepsAway)));
            }
        }


        public Note(NoteValues value, sbyte octave = 4)
        {
            Value = value;
            Octave = octave;
        }


        public Note MoveSteps(int nHalfSteps)
        {
            Note n = this;

            int noteI = (int)n.Value + nHalfSteps;
            //TODO: Use division method from Scale[int i] property.
            while (noteI >= (int)NoteValues.LENGTH)
            {
                noteI -= (int)NoteValues.LENGTH;
                n.Octave += 1;
            }
            while (noteI < 0)
            {
                noteI += (int)NoteValues.LENGTH;
                n.Octave -= 1;
            }

            n.Value = (NoteValues)noteI;
            if (n.Value == NoteValues.LENGTH)
                return MoveSteps(nHalfSteps);
            return n;
        }
        public Note MoveOctave(sbyte nOctaves)
        {
            return new Note(Value, (sbyte)(Octave + nOctaves));
        }

        public override string ToString()
        {
            return Value.ToString().Replace("Sharp", "#").Replace("Flat", "b") +
                   Octave;
        }
        public override bool Equals(object obj)
        {
            return (obj is Note) && Equals((Note)obj);
        }
        public override int GetHashCode()
        {
            return unchecked(((int)Value * 73856093) ^ (Octave * 19349663));
        }
        public bool Equals(Note note)
        {
            return Value == note.Value & Octave == note.Octave;
        }
        public static bool operator==(Note n1, Note n2) { return n1.Equals(n2); }
        public static bool operator!=(Note n1, Note n2) { return !n1.Equals(n2); }
    }
}