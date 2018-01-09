using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Music
{

    public struct Triad : IEquatable<Triad>
    {
        public static Triad Major(Note baseNote, int inversion = 0)
        { return new Triad(baseNote, baseNote.MoveSteps(4), baseNote.MoveSteps(7), inversion); }
        public static Triad Minor(Note baseNote, int inversion = 0)
        { return new Triad(baseNote, baseNote.MoveSteps(3), baseNote.MoveSteps(7), inversion); }
        public static Triad Diminished(Note baseNote, int inversion = 0)
        { return new Triad(baseNote, baseNote.MoveSteps(3), baseNote.MoveSteps(6), inversion); }
        //TODO: More triads.


        public Note First, Second, Third;
        public int Inversion;


        /// <summary>
        /// Gets the actual notes after doing the inversion.
        /// </summary>
        public Triad AfterInversion
        {
            get
            {
                Triad t = this;
                while (t.Inversion > 0)
                {
                    t.Inversion -= 1;

                    //Move the first note to the top of the chord
                    //    by incrementing its octave.
                    Note f = t.First.UpOctave;
                    t.First = t.Second;
                    t.Second = t.Third;
                    t.Third = f;
                }
                while (t.Inversion < 0)
                {
                    t.Inversion += 1;

                    //Move the third note to the bottom of the chord
                    //    by decrementing its octave.
                    Note th = t.Third.DownOctave;
                    t.Third = t.Second;
                    t.Second = t.First;
                    t.First = th;
                }
                return t;
            }
        }

        public Triad UpInversion { get { return new Triad(First, Second, Third, Inversion + 1); } }
        public Triad DownInversion { get { return new Triad(First, Second, Third, Inversion - 1); } }


        public Triad(Note first, Note second, Note third,
                     int inversion = 0)
        {
            First = first;
            Second = second;
            Third = third;

            Inversion = inversion;
        }


        public override string ToString()
        {
            return "[" + First + " " + Second + " " + Third + ":" + Inversion + "]";
        }
        public override int GetHashCode()
        {
            //TODO: Fix. Copy from Manbil::Vector4f.
            return unchecked(First.GetHashCode() ^
                             Second.GetHashCode() ^
                             Third.GetHashCode() ^
                             Inversion.GetHashCode());
        }
        public override bool Equals(object obj)
        {
            return obj is Triad && Equals((Triad)obj);
        }
        public bool Equals(Triad t)
        {
            return t.First == First & t.Second == Second & t.Third == Third &
                   t.Inversion == Inversion;
        }
        public static bool operator==(Triad t1, Triad t2) { return t1.Equals(t2); }
        public static bool operator !=(Triad t1, Triad t2) { return !(t1 == t2); }
    }
}
