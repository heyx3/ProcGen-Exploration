using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;


namespace Generators.Road
{
    /// <summary>
    /// Major and minor vectors are constant values, creating a grid-like road layout.
    /// </summary>
    public class GridOrthoBasis : IRoadOrthoBasis, ISerializable
    {
        public Vector2 Center { get; set; }
        public float Importance { get; set; }

        public Vector2 MajorAxis { get; private set; }
        public Vector2 MinorAxis { get; private set; }

        public float Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                MajorAxis = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
                MinorAxis = MajorAxis.GetPerp();
            }
        }

        private float rotation;


        public GridOrthoBasis(float _rotation = 0.0f)
        {
            Rotation = _rotation;
        }


        public OrthoBasis GetOrthoBasis(Vector2 pos, float height, Vector3 surfaceNormal)
        {
            return new OrthoBasis(MajorAxis, MinorAxis);
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Rotation", rotation);
            info.AddValue("CenterX", Center.x);
            info.AddValue("CenterY", Center.y);
            info.AddValue("Importance", Importance);
        }
        public GridOrthoBasis(SerializationInfo info, StreamingContext context)
        {
            Rotation = info.GetSingle("Rotation");
            Center = new Vector2(info.GetSingle("CenterX"), info.GetSingle("CenterY"));
            Importance = info.GetSingle("Importance");
        }
    }
}