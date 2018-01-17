using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Testers
{
    public class BVHTester : MonoBehaviour
    {
        public struct Sphere : IEquatable<Sphere>
        {
            public float Radius;
            public Vector2 Pos;
            public Sphere(float rad, Vector2 p) { Radius = rad; Pos = p; }
            public bool Equals(Sphere other) { return Radius == other.Radius && Pos == other.Pos; }
        }
        public class TestBVH : BVH.BVH<Sphere>
        {
            public TestBVH(int threshold) : base(threshold) { }
            public override Rect GetBounds(Sphere data)
            {
                float diam = data.Radius * 2.0f;
                Rect r = new Rect(data.Pos.x - data.Radius,
                                data.Pos.y - data.Radius,
                                diam, diam);
                return r;
            }
        }


        public Color[] DepthDrawCols = new Color[]
        {
            Color.white, Color.green, Color.yellow, Color.red, Color.cyan, Color.magenta,
        };

        public TestBVH MyBVH;

        public int Threshold = 4;

        public float NextSphereR;
        public bool AddSphere = false;

        public bool RemoveNearestSphere = false;


        void Awake()
        {
            MyBVH = new TestBVH(Threshold);
        }
        void Update()
        {
            MyBVH.Threshold = Threshold;

            if (AddSphere)
            {
                AddSphere = false;
                MyBVH.Add(new Sphere(NextSphereR, transform.position));
            }
            if (RemoveNearestSphere)
            {
                RemoveNearestSphere = false;
                if (MyBVH.Count > 0)
                {
                    Vector2 pos = transform.position;
                    Sphere closestSph = MyBVH.GetAll().Min(s => s.Pos.DistSqr(pos));
                    MyBVH.Remove(closestSph);
                }
            }
        }
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            var list = BVH.BVHTester.GetNodeBounds(MyBVH, 0, DepthDrawCols.Length - 1).ToArray();
            foreach (BVH.BVHTester.RectAndDepth rAndD in list)
            {
                Gizmos.color = DepthDrawCols[rAndD.Depth];
                Gizmos.DrawWireCube(rAndD.R.center, rAndD.R.size);
            }
            foreach (Sphere sph in MyBVH.GetAll())
            {
                Gizmos.color = new Color(0.0f, 0.0f, 0.0f, 0.25f);
                Gizmos.DrawSphere(sph.Pos, sph.Radius);
            }
        }
    }
}