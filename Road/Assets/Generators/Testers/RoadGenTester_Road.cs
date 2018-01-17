using System;
using System.Collections.Generic;
using UnityEngine;


namespace Generators.Tests
{
    public class RoadGenTester_Road : MonoBehaviour
    {
        public List<Vector2> Poses;
        public Color Col;

        private Transform tr;

        void Awake()
        {
            tr = transform;
        }
        void OnDrawGizmosSelected()
        {
            Vector2 p = tr.position;
            p += new Vector2(-1.0f + (2.0f * (Col.r + Col.g) * 0.5f),
                             -1.0f + (2.0f * (Col.g + Col.b) * 0.5f)) * 0.05f;

            Gizmos.color = Col;
            for (int i = 0; i < Poses.Count; ++i)
            {
                Gizmos.DrawSphere(Poses[i] + p, 0.1f);
                if (i > 0)
                    Gizmos.DrawLine(Poses[i - 1] + p, Poses[i] + p);
            }
        }
    }
}