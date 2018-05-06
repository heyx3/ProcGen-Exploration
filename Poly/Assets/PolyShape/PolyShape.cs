using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// A concave polygon, defined by its points.
/// </summary>
[Serializable]
public class PolyShape
{
    public struct Point
    {
        public Vector2 P;
        public float Variance;
        public Point(Vector2 p, float variance) { P = p; Variance = variance; }
    }

    private Point[] points;


    public int NPoints { get { return (points == null) ? 0 : points.Length; } }
    public IEnumerable<Vector2> Points { get { return points.Select(p => p.P); } }
    public Vector2 Min { get; private set; }
    public Vector2 Max { get; private set; }
    public Vector2 Size { get { return Max - Min; } }

    public Vector2 GetPoint(int i) { return points[i].P; }
    public float GetVariance(int i) { return points[i].Variance; }


    /// <summary>
    /// Generates a polygon by randomly perturbing points on a circle.
    /// </summary>
    public PolyShape(float radius, float variance, int nPoints, float varianceSpread)
    {
        float radianIncrement = Mathf.PI * 2.0f / nPoints;
        float minDist = radius - variance,
              maxDist = radius + variance;

        points = new Point[nPoints];
        for (int i = 0; i < points.Length; ++i)
        {
            float radians = i * radianIncrement;
            Vector2 pos = new Vector2(Mathf.Cos(radians),
                                      Mathf.Sin(radians));
            pos *= Mathf.Lerp(minDist, maxDist, UnityEngine.Random.value);

            points[i] = new Point(pos, Mathf.Clamp01(0.5f + (MathF.NextGaussian() * varianceSpread)));
        }

        UpdateMinMax();
    }
    /// <summary>
    /// Creates a polygon with the given points.
    /// </summary>
    public PolyShape(Point[] _points)
    {
        points = new Point[_points.Length];
        for (int i = 0; i < points.Length; ++i)
            points[i] = _points[i];

        UpdateMinMax();
    }


    /// <summary>
    /// Divides each edge of this polygon in two,
    ///     using the given method.
    /// </summary>
    /// <param name="splitter">
    /// Given the edge (as a start and end pos)
    ///     and the "variance" (a float to scale the randomness),
    ///     this function should return the new midpoint to split the edge along.
    /// This new point is represented as a position, and a new "variance".
    /// </param>
    public void Subdivide(Func<Vector2, Vector2, float, SplitResult> splitter)
    {
        var newPoints = new Point[points.Length * 2];

        //First copy over all the old points.
        for (int i = 0; i < newPoints.Length; i += 2)
            newPoints[i] = points[i / 2];

        //Next, calculate the new points.
        for (int i = 1; i < newPoints.Length; i += 2)
        {
            int oldPointsI = i / 2,
                oldPointsI2 = (oldPointsI + 1) % points.Length;
            Point a = points[oldPointsI],
                  b = points[oldPointsI2];

            var splitResult = splitter(a.P, b.P, a.Variance);
            newPoints[i - 1] = new Point(newPoints[i - 1].P, splitResult.FirstHalfVariance);
            newPoints[i] = new Point(splitResult.NewMidpoint, splitResult.SecondHalfVariance);
        }

        points = newPoints;

        UpdateMinMax();
    }
    public struct SplitResult
    {
        public Vector2 NewMidpoint;
        public float FirstHalfVariance, SecondHalfVariance;
        public SplitResult(Vector2 newMidpoint, float firstHalfVariance, float secondHalfVariance)
        {
            NewMidpoint = newMidpoint;
            FirstHalfVariance = firstHalfVariance;
            SecondHalfVariance = secondHalfVariance;
        }
    }

    /// <summary>
    /// Draws this polygon as a set of line gizmos.
    /// </summary>
    /// <param name="localToWorld">The transform matrix to apply to this polygon's points.</param>
    public void DrawGizmos(Matrix4x4 localToWorld, float pointRadius = float.NaN)
    {
        if (points == null) return; //Some kind of unity bug.

        //Get the transformed points.
        Vector3[] transformedPoints = new Vector3[NPoints];
        for (int i = 0; i < transformedPoints.Length; ++i)
            transformedPoints[i] = localToWorld.MultiplyPoint(points[i].P);

        //Draw them.
        Color col = Gizmos.color;
        for (int i = 0; i < points.Length; ++i)
        {
            Gizmos.color = col;
            int nextPointI = (i + 1) % points.Length;
            Gizmos.DrawLine(transformedPoints[i], transformedPoints[nextPointI]);

            Gizmos.color = new Color(col.r, col.g, col.b, col.a * 0.5f);
            if (!float.IsNaN(pointRadius))
                Gizmos.DrawSphere(transformedPoints[i], pointRadius);
        }
    }
    /// <summary>
    /// Converts this polygon into a triangle mesh.
    /// </summary>
    /// <param name="outVerts">
    /// This array is automatically initialized if null and resized if the wrong size.
    /// </param>
    /// <param name="outIndices">
    /// This array is automatically initialized if null and resized if the wrong size.
    /// </param>
    public void Triangulate(ref Vector3[] outVerts, ref int[] outIndices)
    {
        //Set up the output vertices array.
        if (outVerts == null || outVerts.Length != NPoints)
            outVerts = new Vector3[NPoints];
        for (int i = 0; i < NPoints; ++i)
            outVerts[i] = points[i].P;
        
        //Slice off individual triangles in the polygon.

        //Create a list of the indices that haven't been sliced off yet.
        //Also create a list of the new triangle slices.
        List<int> mainShape = new List<int>(NPoints),
                  tris = new List<int>(NPoints * 3);
        mainShape.AddRange(NPoints.CountSequence());

        //Keep slicing off triangles until only a triangle is left in the original shape.
        while (mainShape.Count > 3)
        {
            //Find the first index of the first convex triangle, to slice it off.
            //Make sure that the triangle doesn't contain any other points!
            int startI = 0;
            while (!IsConvex(mainShape, startI) || AreOtherPointsInTriSlice(mainShape, startI))
            {
                startI += 1;
                UnityEngine.Assertions.Assert.IsTrue(startI < mainShape.Count,
                                                     "Couldn't find a convex triangle piece!");
            }

            //Slice off the triangle.
            int i2 = (startI + 1) % mainShape.Count,
                i3 = (startI + 2) % mainShape.Count;
            tris.Add(mainShape[startI]);
            tris.Add(mainShape[i2]);
            tris.Add(mainShape[i3]);
            mainShape.RemoveAt(i2);
        }

        //The only thing left should be a triangle. Add it to the triangle list.
        UnityEngine.Assertions.Assert.AreEqual(3, mainShape.Count, "Less than 3 points??");
        tris.AddRange(mainShape);
        mainShape.Clear();

        //Set up the output index array.
        if (outIndices == null || outIndices.Length != tris.Count)
            outIndices = new int[tris.Count];
        for (int i = 0; i < tris.Count; ++i)
            outIndices[i] = tris[i];
    }
    
    //Helper functions for "Triangulate".
    private bool IsConvex(List<int> shapeIndices, int triStart)
    {
        Vector2 p1 = points[shapeIndices[triStart]].P,
                p2 = points[shapeIndices[(triStart + 1) % shapeIndices.Count]].P,
                p3 = points[shapeIndices[(triStart + 2) % shapeIndices.Count]].P,
                triCenter = (p1 + p2 + p3) * 0.33333333333333333f;
        Vector2 to12 = p2 - p1,
                to23 = p3 - p2;
        Vector2 perp12 = new Vector2(-to12.y, to12.x),
                perp23 = new Vector2(-to23.y, to23.x);

        UnityEngine.Assertions.Assert.AreEqual(Mathf.Sign(Vector2.Dot(perp12, triCenter - p1)),
                                               Mathf.Sign(Vector2.Dot(perp23, triCenter - p2)),
                                               "Perp calculation must be bad");
        return Vector2.Dot(perp12, triCenter - p1) < 0.0f;
    }
    private bool AreOtherPointsInTriSlice(List<int> shapeIndices, int triStart)
    {
        int tri2 = (triStart + 1) % shapeIndices.Count,
            tri3 = (triStart + 2) % shapeIndices.Count;

        //Pre-calculate stuff for determining whether a point is in the sliced-off triangle.
        Vector2 p1 = points[shapeIndices[triStart]].P,
                p2 = points[shapeIndices[tri2]].P,
                p3 = points[shapeIndices[tri3]].P;
        Vector2 p12Perp = new Vector2(-(p2.y - p1.y), p2.x - p1.x),
                p13Perp = new Vector2(-(p3.y - p1.y), p3.x - p1.x),
                p23Perp = new Vector2(-(p3.y - p2.y), p3.x - p2.x);
        Vector2 triMidpoint = (p1 + p2 + p3) * 0.33333333f;
        float dotSign12 = Mathf.Sign(Vector2.Dot(p12Perp, triMidpoint - p1)),
              dotSign13 = Mathf.Sign(Vector2.Dot(p13Perp, triMidpoint - p1)),
              dotSign23 = Mathf.Sign(Vector2.Dot(p23Perp, triMidpoint - p2));

        for (int i = 0; i < shapeIndices.Count; ++i)
        {
            //Skip the triangle itself.
            if (i == triStart | i == tri2 | i == tri3)
                continue;

            if (IsPointInTriangle(points[shapeIndices[i]].P,
                                  p1, p2, p3, p12Perp, p13Perp, p23Perp,
                                  dotSign12, dotSign13, dotSign23))
            {
                return true;
            }
        }

        return false;
    }
    private bool IsPointInTriangle(Vector2 p,
                                   Vector2 t1, Vector2 t2, Vector2 t3,
                                   Vector2 t12Perp, Vector2 t13Perp, Vector2 t23Perp,
                                   float dotSign12, float dotSign13, float dotSign23)
    {
        float pDotSign12 = Mathf.Sign(Vector2.Dot(t12Perp, p - t1)),
              pDotSign13 = Mathf.Sign(Vector2.Dot(t13Perp, p - t1)),
              pDotSign23 = Mathf.Sign(Vector2.Dot(t23Perp, p - t2));
        return pDotSign12 == dotSign12 && pDotSign13 == dotSign13 && pDotSign23 == dotSign23;
    }
    
    public void UpdateMinMax()
    {
        Min = new Vector2(float.MaxValue, float.MaxValue);
        Max = new Vector2(float.MinValue, float.MinValue);
        foreach (var point in points)
        {
            Min = new Vector2(Math.Min(Min.x, point.P.x),
                              Math.Min(Min.y, point.P.y));
            Max = new Vector2(Math.Max(Max.x, point.P.x),
                              Math.Max(Max.y, point.P.y));
        }
    }
}