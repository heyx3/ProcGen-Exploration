using System;
using System.Collections.Generic;
using UnityEngine;


namespace Generators.Road
{
    public struct OrthoBasis
    {
        public Vector2 Major, Minor;

        public OrthoBasis(Vector2 major, Vector2 minor) { Major = major; Minor = minor; }
        public OrthoBasis(Vector2 major) : this(major, major.GetPerp()) { }
    }


    /// <summary>
    /// A way of determining the major and minor axes for roads to travel along.
    /// </summary>
    public interface IRoadOrthoBasis
    {
        Vector2 Center { get; }
        float Importance { get; }


        OrthoBasis GetOrthoBasis(Vector2 pos, float height, Vector3 surfaceNormal);
    }
}