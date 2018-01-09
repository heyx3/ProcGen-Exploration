using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Generators.Road
{
    public struct Segment : IEquatable<Segment>
    {
        public Vertex P1 { get; private set; }
        public Vertex P2 { get; private set; }

        public Road Owner { get; private set; }

        public Rect AABB { get; private set; }


        public Segment(Vertex p1, Vertex p2, Road owner)
        {
            P1 = p1;
            P2 = p2;
            Owner = owner;

            AABB = new Rect().BoundByPoints(P1.Pos, P2.Pos);
        }


        public bool Equals(Segment other)
        {
            return P1 == other.P1 && P2 == other.P2 && Owner == other.Owner;
        }
    }
}