using System;
using System.Collections.Generic;
using UnityEngine;


namespace Generators.Road
{
    /// <summary>
    /// A junction in the city's roads.
    /// </summary>
    public class Vertex : IEquatable<Vertex>
    {
        public Vector2 Pos;
        public HashSet<Road> RoadsConnectedTo = new HashSet<Road>();
        public HashSet<Vertex> VertsConnectedTo = new HashSet<Vertex>();


        public Vertex(Vector2 pos) { Pos = pos; }


        public void ConnectTo(Vertex other)
        {
            Assert.IsTrue(!other.VertsConnectedTo.Contains(this) && !VertsConnectedTo.Contains(other));
            other.VertsConnectedTo.Add(this);
            VertsConnectedTo.Add(other);
        }
        public void ConnectTo(Road rd)
        {
            Assert.IsTrue(!RoadsConnectedTo.Contains(rd));
            RoadsConnectedTo.Add(rd);
        }
        public void DisconnectFrom(Vertex other)
        {
            Assert.IsTrue(other.VertsConnectedTo.Contains(this) && VertsConnectedTo.Contains(other));
            other.VertsConnectedTo.Remove(this);
            VertsConnectedTo.Remove(other);
        }
        public void DisconnectFrom(Road rd)
        {
            Assert.IsTrue(RoadsConnectedTo.Contains(rd));
            RoadsConnectedTo.Remove(rd);
        }

        public bool Equals(Vertex other)
        {
            return ReferenceEquals(other, this);
        }

        public override string ToString()
        {
            return Pos.ToString();
        }
    }
}