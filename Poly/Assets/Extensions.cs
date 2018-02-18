using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class Extensions
{
    /// <summary>
    /// Iterates through all values from "start" to [this value] - 1.
    /// </summary>
    public static IEnumerable<int> CountSequence(this int count, int start = 0)
    {
        for (int i = start; i < count; ++i)
            yield return i;
    }
}
public static class MathF
{
    /// <summary>
    /// Gets whether the two given line segments intersect.
    /// </summary>
    public static bool SegmentsIntersect(Vector2 a1, Vector2 b1,
                                         Vector2 a2, Vector2 b2)
    {
        //http://jeffe.cs.illinois.edu/teaching/373/notes/x06-sweepline.pdf

        Vector2 vec1 = b1 - a1,
                vec2 = b2 - a2;
        Vector2 perp1 = new Vector2(-vec1.y, vec1.x),
                perp2 = new Vector2(-vec2.y, vec2.x);
        Vector2 a1ToA2 = a2 - a1,
                a1ToB2 = b2 - a1,
                a2ToB1 = b1 - a2;

        float dir_1a = Mathf.Sign(Vector2.Dot(perp1, a1ToA2)),
              dir_1b = Mathf.Sign(Vector2.Dot(perp1, a1ToB2)),
              dir_2a = Mathf.Sign(Vector2.Dot(perp2, -a1ToA2)),
              dir_2b = Mathf.Sign(Vector2.Dot(perp2, a2ToB1));
        return dir_1a != dir_1b && dir_2a != dir_2b;
    }

    public static Vector2 Lerp(float a, float b, Vector2 t)
    {
        float range = b - a;
        return new Vector2(a + (t.x * range),
                           a + (t.y * range));
    }
    public static Vector2 InverseLerp(Vector2 a, Vector2 b, Vector2 x)
    {
        Vector2 range = b - a,
                startDist = x - a;
        return new Vector2(startDist.x / range.x,
                           startDist.y / range.y);
    }

    /// <summary>
    /// Returns a random float with a normal distribution, using Unity's random number generator.
    /// </summary>
    public static float NextGaussian()
    {
        //Source: https://stackoverflow.com/questions/5817490/implementing-box-mueller-random-number-generator-in-c-sharp

        float u, v, S;

        do
        {
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0);

        float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
        return u * fac;
    }
}