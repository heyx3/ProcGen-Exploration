using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generators.Tests
{
    public class RoadGenTester_Generator : Road.RoadGenerator
    {
        public Rect Bounds = new Rect(0.0f, 0.0f, 50.0f, 50.0f);


        protected override bool IsInBounds(Vector2 pos)
        {
            return Bounds.Contains(pos);
        }
    }
}