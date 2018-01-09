using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Generators.Road;


namespace Generators.Tests
{
    public class RoadGenTester : MonoBehaviour
    {
        public Vector2 CitySize = new Vector2(50.0f, 50.0f);
        public Vector2 SeedPos = new Vector2(25.0f, 25.0f);

        public float RoadSegmentInterval = 1.0f;
        public float SegmentMinimumLength = 0.001f;
        public float VertexMinDist = 0.1f;
        public int VertexBVHThreshold = 100,
                   SegmentBVHThreshold = 100,
                   RoadBVHThreshold = 10;


        void Awake()
        {
            RoadGenTester_Generator gen = new RoadGenTester_Generator();
            gen.Bounds = new Rect(0.0f, 0.0f, CitySize.x, CitySize.y);
            gen.RoadOrthoBases.AddRange(FindObjectsOfType<RoadGenTester_GridOrthoBasis>().Select(g => (IRoadOrthoBasis)g.OrthoBasis));
            gen.RoadOrthoBases.AddRange(FindObjectsOfType<RoadGenTester_RadialOrthoBasis>().Select(r => (IRoadOrthoBasis)r.OrthoBasis));
            gen.RoadStepInterval = RoadSegmentInterval;
            gen.SegmentMinLength = SegmentMinimumLength;
            gen.Vertices.MergeRadius = VertexMinDist;
            gen.Vertices.Threshold = VertexBVHThreshold;
            gen.Segments.Threshold = SegmentBVHThreshold;
            gen.Roads.Threshold = RoadBVHThreshold;

            gen.Run(SeedPos);

            System.Random rng = new System.Random(2345111);
            Transform roadContainer = new GameObject("Roads").transform;
            foreach (Road.Road rd in gen.Roads.GetAll())
            {
                GameObject go = new GameObject("Road");
                go.transform.parent = roadContainer;
                RoadGenTester_Road rgt_r = go.AddComponent<RoadGenTester_Road>();
                rgt_r.Poses = rd.Points.Select(v => v.Pos).ToList();
                rgt_r.Col = new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.25f);
            Gizmos.DrawCube(CitySize * 0.5f, CitySize);
        }
        void OnValidate()
        {
            const float epsilon = 0.00001f;
            
            RoadSegmentInterval = Mathf.Max(RoadSegmentInterval, epsilon);
            SegmentMinimumLength = Mathf.Max(SegmentMinimumLength, epsilon);
            VertexMinDist = Mathf.Max(VertexMinDist, epsilon);

            VertexBVHThreshold = Mathf.Max(VertexBVHThreshold, 2);
            SegmentBVHThreshold = Mathf.Max(SegmentBVHThreshold, 2);
            RoadBVHThreshold = Mathf.Max(RoadBVHThreshold, 2);
        }
    }
}