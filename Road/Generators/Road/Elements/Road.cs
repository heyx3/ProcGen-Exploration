using System;
using System.Collections.Generic;
using UnityEngine;


namespace Generators.Road
{
    /// <summary>
    /// A collection of connected line segments.
    /// </summary>
    [Serializable]
    public class Road : IEquatable<Road>
    {
        public List<Vertex> Points = new List<Vertex>();

        public Rect BoundingBox { get; private set; }


        /// <summary>
        /// Makes this road update its bounding box.
        /// Should be called after modifications to the Point list.
        /// If the only modification was the addition of a new vertex,
        ///     pass the index of that vertex.
        /// </summary>
        public void UpdateBoundingBox(int newVertex = -1)
        {
            if (Points.Count == 0)
            {
                BoundingBox = new Rect();
            }
            else if (newVertex > -1)
            {
                BoundingBox = Rect.MinMaxRect(Mathf.Min(BoundingBox.xMin, Points[newVertex].Pos.x),
                                              Mathf.Min(BoundingBox.yMin, Points[newVertex].Pos.y),
                                              Mathf.Max(BoundingBox.xMax, Points[newVertex].Pos.x),
                                              Mathf.Max(BoundingBox.yMax, Points[newVertex].Pos.y));
            }
            else
            {
                BoundingBox = Rect.MinMaxRect(Points[0].Pos.x, Points[0].Pos.y,
                                              Points[0].Pos.x, Points[0].Pos.y);
                for (int i = 1; i < Points.Count; ++i)
                {
                    BoundingBox = Rect.MinMaxRect(Mathf.Min(BoundingBox.xMin, Points[i].Pos.x),
                                                  Mathf.Min(BoundingBox.yMin, Points[i].Pos.y),
                                                  Mathf.Max(BoundingBox.xMax, Points[i].Pos.x),
                                                  Mathf.Max(BoundingBox.yMax, Points[i].Pos.y));
                }
            }
        }

        /// <summary>
        /// Gets the earliest intersection point along this road with the given road.
        /// Returns whether any such intersection exists.
        /// Also outputs the position of the intersection,
        ///     and the lower index of the segment from each road that is part of the intersection.
        /// </summary>
        public bool Intersects(Road other, out Vector2 intersectPos,
                               out int thisIndex, out int otherIndex)
        {
            intersectPos = Vector2.zero;
            thisIndex = -1;
            otherIndex = -1;

            if (Points.Count < 2 || other.Points.Count < 2)
                return false;

            for (int i = 0; i < Points.Count - 1; ++i)
            {
                for (int j = 0; j < other.Points.Count - 1; ++j)
                {
                    float t1 = float.NaN,
                          t2 = float.NaN;
                    if (GeneratorUtils.SegmentsIntersect(Points[i].Pos, Points[i + 1].Pos,
                                                         other.Points[j].Pos, other.Points[j + 1].Pos,
                                                         ref t1, ref t2))
                    {
                        intersectPos = Points[i].Pos + ((Points[i + 1].Pos - Points[i].Pos) * t1);
                        thisIndex = i;
                        otherIndex = j;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the distance from this road to the given one.
        /// Also returns the points on each road that are as close as possible to the other road.
        /// Also returns the lower index of the segment of each road that is as close as possible to the other road.
        /// </summary>
        public float DistanceTo(Road other, out Vector2 posOnThis, out Vector2 posOnOther,
                                out int thisIndex, out int otherIndex)
        {
            posOnThis = Vector2.zero;
            posOnOther = Vector2.zero;
            thisIndex = -1;
            otherIndex = -1;

            if (Points.Count < 2 || other.Points.Count < 2)
                return float.NaN;

            float minDist = float.MaxValue;
            for (int i = 0; i < Points.Count - 1; ++i)
            {
                for (int j = 0; j < other.Points.Count - 1; ++j)
                {
                    Vector2 _posOnThis, _posOnOther;
                    float t1 = float.NaN,
                          t2 = float.NaN;
                    float tempDist = GeneratorUtils.DistanceToSegment(Points[i].Pos, Points[i + 1].Pos,
                                                                      other.Points[j].Pos, other.Points[j + 1].Pos,
                                                                      out _posOnThis, out _posOnOther,
                                                                      ref t1, ref t2);
                    if (tempDist < minDist)
                    {
                        minDist = tempDist;
                        posOnThis = _posOnThis;
                        posOnOther = _posOnOther;
                        thisIndex = i;
                        otherIndex = j;
                    }
                }
            }

            return minDist;
        }
		

		public bool Equals(Road other)
		{
			return ReferenceEquals(other, this);
		}

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("||");
            for (int i = 0; i < Points.Count; ++i)
            {
                sb.Append(Points[i].ToString());
                if (i > 20)
                {
                    sb.Append("...||");
                    return sb.ToString();
                }
                else
                {
                    sb.Append(", ");
                }
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append("||");
            return sb.ToString();
        }
    }
}