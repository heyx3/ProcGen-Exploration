using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;


namespace Generators.Road
{
    /// <summary>
    /// Major axis points away from the center.
    /// Minor axis points perpendicularly to that.
    /// </summary>
    public class RadialOrthoBasis : IRoadOrthoBasis, ISerializable
    {
        public Vector2 Center { get; set; }
        public float Importance { get; set; }


        public RadialOrthoBasis() { }


        public OrthoBasis GetOrthoBasis(Vector2 pos, float height, Vector3 surfaceNormal)
        {
            return new OrthoBasis((pos - Center).normalized);
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CenterX", Center.x);
            info.AddValue("CenterY", Center.y);
            info.AddValue("Importance", Importance);
        }
        public RadialOrthoBasis(SerializationInfo info, StreamingContext context)
        {
            Center = new Vector2(info.GetSingle("CenterX"), info.GetSingle("CenterY"));
            Importance = info.GetSingle("Importance");
        }
    }
}