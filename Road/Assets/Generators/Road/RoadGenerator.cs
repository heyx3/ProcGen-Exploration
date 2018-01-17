using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Generators.Road
{
    [Serializable]
    public class RoadGenerator
    {
        public List<IRoadOrthoBasis> RoadOrthoBases = new List<IRoadOrthoBasis>();
        private float[] tempDists = new float[0];

        public RoadBVH Roads = new RoadBVH(10);

        public VertexBVH Vertices = new VertexBVH(0.5f, 10);
        public SegmentBVH Segments = new SegmentBVH(10);

        public float RoadStepInterval = 1.0f,
                     SegmentMinLength = 0.001f;


        private Dictionary<Vertex, OrthoBasis> cachedBases = new Dictionary<Vertex, OrthoBasis>();


        public OrthoBasis GetBasis(Vertex pos)
        {
            if (cachedBases.ContainsKey(pos))
                return cachedBases[pos];
            else
            {
                OrthoBasis b = GetBasisP(pos.Pos);
                cachedBases.Add(pos, b);
                return b;
            }
        }
        public OrthoBasis GetBasisP(Vector2 pos)
        {
            //Get surface data.
            float height;
            Vector3 surfNorm;
            GetSurfaceData(pos, out height, out surfNorm);

            //Get the weight scale for each basis based on its distance.
            if (tempDists.Length < RoadOrthoBases.Count)
            {
                tempDists = new float[RoadOrthoBases.Count];
            }
            for (int i = 0; i < RoadOrthoBases.Count; ++i)
            {
                tempDists[i] = pos.Distance(RoadOrthoBases[i].Center) /
                                Mathf.Max(0.0001f, RoadOrthoBases[i].Importance);
            }
            float invSum = 1.0f / tempDists.Sum();


            //Now calculate the weighted average of the ortho bases.
            OrthoBasis ob = new OrthoBasis(Vector2.zero, Vector2.zero);
            for (int i = 0; i < RoadOrthoBases.Count; ++i)
            {
                float weight = 1.0f - (tempDists[i] * invSum);
                
                //Edge-case.
                if (RoadOrthoBases.Count == 1)
                    weight = 1.0f;

                OrthoBasis tempOB = RoadOrthoBases[i].GetOrthoBasis(pos, height, surfNorm);

                ob.Major += tempOB.Major * weight;
                ob.Minor += tempOB.Minor * weight;
            }

            return ob;
        }

        /// <summary>
        /// Gets the next position along the major or minor axis of the city,
        ///     given the current position and the step length.
        /// </summary>
        public Vector2 GetNext(Vertex pos, float step, Vector2 previousDir, bool useMajor)
        {
            //Use RK4 integration instead of the standard Euler.
            Vector2 b1 = GetDir(GetBasis(pos), useMajor, previousDir),
                    b2 = GetDir(GetBasisP(pos.Pos + (b1 * step / 2.0f)), useMajor, previousDir),
                    b3 = GetDir(GetBasisP(pos.Pos + (b2 * step / 2.0f)), useMajor, previousDir),
                    b4 = GetDir(GetBasisP(pos.Pos + (b3 * step)), useMajor, previousDir);

            Vector2 delta = (b1 / 6.0f) + (b2 / 3.0f) + (b3 / 3.0f) + (b4 / 6.0f);
            delta = delta.normalized * Mathf.Abs(step);
            return pos.Pos + delta;
        }
        private Vector2 GetDir(OrthoBasis basis, bool useMajor, Vector2 previousDir)
        {
            Vector2 dir = (useMajor ? basis.Major : basis.Minor);
            if (previousDir == Vector2.zero || Vector2.Dot(previousDir, dir) >= 0.0f)
                return dir;
            return -dir;
        }


        /// <summary>
        /// Gets the height and surface normal at the given point in the city.
        /// Default behavior: returns a height of 0 and a normal pointing straight-up along the Y.
        /// </summary>
        protected virtual void GetSurfaceData(Vector2 pos, out float height, out Vector3 norm)
        {
            height = 0.0f;
            norm = new Vector3(0.0f, 1.0f, 0.0f);
        }
        /// <summary>
        /// Gets the importance of a specific area in the city.
        /// Default behavior: returns 0 for every position.
        /// </summary>
        protected virtual float GetPriority(Vector2 pos)
        {
            return 0.0f;
        }
        /// <summary>
        /// Gets whether the given position is within the bounds of the city.
        /// Default behavior: returns whether the position is within a bounding box
        ///     from {-500, -500} to {500, 500}.
        /// </summary>
        protected virtual bool IsInBounds(Vector2 pos)
        {
            return pos.x >= -500.0f && pos.x <= 500.0f &&
                   pos.y >= -500.0f && pos.y <= 500.0f;
        }


        private struct VertexSeed : IEquatable<VertexSeed>
        {
            public Vertex V;
            public bool UseMajor;
            public int Dir;

            public VertexSeed(Vertex v, bool useMajor, int dir) { V = v; UseMajor = useMajor; Dir = dir; }

            public bool Equals(VertexSeed other) { return V == other.V && UseMajor == other.UseMajor && Dir == other.Dir; }
            public override bool Equals(object obj) { return obj is VertexSeed && Equals((VertexSeed)obj); }
            public override int GetHashCode() { return (V.Pos.GetHashCode() * 73856093) ^ ((Dir + (UseMajor ? 13 : 83492791)) * 19349663); }
            public override string ToString() { return (UseMajor ? "Major" : "Minor") + " towards " + Dir + " starting from " + V.ToString(); }
        }
        public void Run(Vector2 seed)
        {
            //Here's the algorithm:
            //1. Start at the seed position.
            //2. Trace from that seed across the "field" defined by the road ortho bases until the end is hit or something else stops it.
            //3. If the traced line fits certain constraints, then it is now a road
            //     and the various discrete points it stopped at along the way
            //     are fed in as new potential seeds to start from.
            //   Each new seed point has a "priority" based on something (e.x. a population density map).
            //4. As long as we have new seeds to consider, pick the seed with the highest priority and GOTO 2.

            HashSet<VertexSeed> alreadyTried = new HashSet<VertexSeed>();
            PriorityQueue<VertexSeed> candidates = new PriorityQueue<VertexSeed>(false);

            float priority = 1.0f;// GetPriority(seed);
            Vertex v1 = new Vertex(seed);
            for (int dir = -1; dir < 2; dir += 2)
            {
                VertexSeed vs = new VertexSeed(v1, true, dir);
                candidates.Add(vs, priority);
                alreadyTried.Add(vs);
            }

            while (candidates.Count > 0)
            {
                VertexSeed vs = candidates.Peek(out priority);
                candidates.Pop();

                Road rd = Trace(vs.V, vs.UseMajor, vs.Dir);

                if (rd != null)
                {
                    //Add the road to the city, and add the road's vertices to the new road seed collection.
                    Roads.Add(rd);
                    for (int i = 0; i < rd.Points.Count; ++i)
                    {
                        priority = GetPriority(rd.Points[i].Pos);

                        VertexSeed vSeed = new VertexSeed(rd.Points[i], !vs.UseMajor, 1);
                        if (!alreadyTried.Contains(vSeed))
                        {
                            candidates.Add(vSeed, priority);
                            alreadyTried.Add(vSeed);
                        }

                        vSeed.Dir = -1;
                        if (!alreadyTried.Contains(vSeed))
                        {
                            candidates.Add(vSeed, priority);
                            alreadyTried.Add(vSeed);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new road from the given starting point along the given axis.
        /// Takes care of adding the vertices and segments to their respective BVH.
        /// </summary>
        private Road Trace(Vertex start, bool useMajorAxis, int dir)
        {
            float stepInterval = RoadStepInterval * (float)dir;

            //Step along the road in increments to build up the segments defining it.
            Vector2 currentPos = start.Pos;
            Vector2 currentVel = (float)dir * (useMajorAxis ?
                                                    GetBasis(start).Major :
                                                    GetBasis(start).Minor);

            Road r = new Road();
            r.Points.Add(start);
            start.ConnectTo(r);

            bool keepGoing = true;
            Vertex newVert = start;
            while (keepGoing)
            {
                //Get the next vertex along the road.
                Vector2 nextPos = GetNext(newVert, stepInterval, currentVel, useMajorAxis);
                VertexFindResults vfr = FindOrMakeVertex(r, nextPos);
                newVert = vfr.ToUse;

                if (newVert != null)
                {
                    newVert.ConnectTo(r);
                    r.Points.Add(newVert);
                    Segments.Add(new Segment(r.Points[r.Points.Count - 2], newVert, r));

                    nextPos = newVert.Pos;
                }

                currentVel = nextPos - currentPos;

                currentPos = nextPos;
                keepGoing = !vfr.EndRoad;
            }

            //If the road is a dud, exit.
            if (r.Points.Count < 2)
            {
                foreach (Vertex v in r.Points)
                    v.DisconnectFrom(r);
                return null;
            }

            //Finalize the road's data and return it.
            r.UpdateBoundingBox();
            return r;
        }

        private struct VertexFindResults
        {
            public static VertexFindResults Reject()
            {
                VertexFindResults vfr = new VertexFindResults();
                vfr.ToUse = null;
                vfr.EndRoad = true;
                return vfr;
            }
            public static VertexFindResults Accept(Vertex toUse, bool endRoad)
            {
                VertexFindResults vfr = new VertexFindResults();
                vfr.ToUse = toUse;
                vfr.EndRoad = endRoad;
                return vfr;
            }

            public Vertex ToUse;
            public bool EndRoad;
        }
        /// <summary>
        /// Given a road and a new position to add to the road,
        ///     gets or creates a vertex to add to the road.
        /// The vertex will always already be added to the vertex BVH.
        /// If the returned vertex is null, it shouldn't be added to the road.
        /// If "EndRoad" is true, the road should not continue on.
        /// </summary>
        private VertexFindResults FindOrMakeVertex(Road r, Vector2 nextPos)
        {
            Vertex lastPoint = r.Points[r.Points.Count - 1];

            //Reject if the segment length is too short or its position is outside the city limits.
            if (lastPoint.Pos.DistSqr(nextPos) <= (SegmentMinLength * SegmentMinLength) ||
                !IsInBounds(nextPos))
            {
                return VertexFindResults.Reject();
            }

            //If the next position is near another vertex, use that vertex and end the road there.
            foreach (Vertex v in Vertices.GetAllNearbyPos(nextPos))
            {
                //Don't let the road loop back onto itself.
                if (v.RoadsConnectedTo.Contains(r))
                    continue;

                //Don't create a segment that already exists.
                if (v.VertsConnectedTo.Contains(lastPoint))
                    return VertexFindResults.Reject();

                v.ConnectTo(lastPoint);

                return VertexFindResults.Accept(v, true);
            }


            Rect segBnds = new Rect().BoundByPoints(nextPos, lastPoint.Pos);
            
            //If the segment intersects another segment, turn it into an intersection and continue.
            List<Segment> segs = Segments.GetAllNearbyBnds(segBnds).ToList();
            float t1 = float.NaN,
                  t2 = float.NaN;
            Segment? hitSeg = null;
            //Get any intersected segments.
            //If we intersect with a previous part of this road, reject.
            for (int i = 0; i < segs.Count; ++i)
            {
                float temp1 = float.NaN,
                      temp2 = float.NaN;
                bool intersects = GeneratorUtils.SegmentsIntersect(lastPoint.Pos, nextPos,
                                                                   segs[i].P1.Pos, segs[i].P2.Pos,
                                                                   ref temp1, ref temp2);
                if (segs[i].Owner == r ||
                    segs[i].P1.RoadsConnectedTo.Contains(r) ||
                    segs[i].P2.RoadsConnectedTo.Contains(r))
                {
                    if (intersects)
                    {
                        return VertexFindResults.Reject();
                    }
                }
                else if (intersects)
                {
                    t1 = temp1;
                    t2 = temp2;
                    hitSeg = segs[i];
                }
            }
            if (hitSeg.HasValue)
            {
                //Split up the road that the segment is a part of
                //    by adding a new vertex at the intersection.
                Vertex vtx = new Vertex(hitSeg.Value.P1.Pos +
                                          ((hitSeg.Value.P2.Pos - hitSeg.Value.P1.Pos) * t2));
                vtx.ConnectTo(lastPoint);
                Vertices.Add(vtx);
                SplitSegment(vtx, hitSeg.Value);

                return VertexFindResults.Accept(vtx, false);
            }


            //This segment isn't special in any way.
            Vertex vert = new Vertex(nextPos);
            vert.ConnectTo(lastPoint);
            Vertices.Add(vert);
            return VertexFindResults.Accept(vert, false);
        }

        /// <summary>
        /// Splits the given segment so that the given vertex sits between its end-points.
        /// </summary>
        private void SplitSegment(Vertex v, Segment toSplit)
        {
            v.ConnectTo(toSplit.Owner);

            toSplit.P1.DisconnectFrom(toSplit.P2);
            v.ConnectTo(toSplit.P1);
            v.ConnectTo(toSplit.P2);

            List<Vertex> roadPoints = toSplit.Owner.Points;

            //Get the index of the first vertex in the segment.
            int startI = toSplit.Owner.Points.IndexOf(toSplit.P1);

            //Presumably, the next vertex is the second one in the segment.
            Assert.IsTrue(startI > -1);
            Assert.IsTrue(roadPoints.Count > startI + 1);
            Assert.IsTrue(roadPoints[startI + 1] == toSplit.P2,
                          "Expected " + (startI + 1) + " but got " +
                            roadPoints.IndexOf(toSplit.P2));

            //Remove the segment.
            Segments.Remove(toSplit);

            //Add the vertex to the road.
            roadPoints.Insert(startI + 1, v);
            Segments.Add(new Segment(roadPoints[startI], v, toSplit.Owner));
            Segments.Add(new Segment(v, roadPoints[startI + 2], toSplit.Owner));
        }
    }
}