using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class Extensions
{
    public static Vector2 ToHorz(this Vector3 v) { return new Vector2(v.x, v.z); }
    public static Vector2 ToHorzN(this Vector3 v) { return v.ToHorz().normalized; }

    public static Vector3 To3D(this Vector2 v, float z = 0.0f) { return new Vector3(v.x, 0.0f, v.y); }

    public static Vector2 GetPerp(this Vector2 v) { return new Vector2(-v.y, v.x); }

    public static float DistSqr(this Vector2 v, Vector2 v2) { return (v - v2).sqrMagnitude; }
    public static float DistSqr(this Vector3 v, Vector3 v2) { return (v - v2).sqrMagnitude; }

    public static float Distance(this Vector2 v, Vector2 v2) { return Vector2.Distance(v, v2); }
    public static float Distance(this Vector3 v, Vector3 v2) { return Vector3.Distance(v, v2); }

    public static Vector2 Rotated(this Vector2 v, float angle)
    {
        float cos = Mathf.Cos(angle),
              sin = Mathf.Sin(angle);
        return new Vector2((v.x * cos) + (v.y * sin),
                           (v.x * sin) + (v.y * cos));
    }


    public static Rect BoundByPoints(this Rect r, Vector2 p1, Vector2 p2)
    {
        return Rect.MinMaxRect(Mathf.Min(p1.x, p2.x),
                               Mathf.Min(p1.y, p2.y),
                               Mathf.Max(p1.x, p2.x),
                               Mathf.Max(p1.y, p2.y));
    }
    public static Rect UnionWith(this Rect r, Rect other)
    {
        return Rect.MinMaxRect(Mathf.Min(r.xMin, other.xMin),
                               Mathf.Min(r.yMin, other.yMin),
                               Mathf.Max(r.xMax, other.xMax),
                               Mathf.Max(r.yMax, other.yMax));
    }


    /// <summary>
    /// Combines all collections into one big collection.
    /// </summary>
    public static IEnumerable<T> Collapse<T>(IEnumerable<IEnumerable<T>> collection)
    {
        foreach (IEnumerable<T> ts in collection)
            foreach (T t in ts)
                yield return t;
    }


    private struct TComparer<T> : IComparer<T>
    {
        public Func<T, T, int> CompareFunc;
        public TComparer(Func<T, T, int> compareFunc) { CompareFunc = compareFunc; }
        public int Compare(T x, T y)
        {
            return CompareFunc(x, y);
        }
    }
    public static void Sort<T>(this List<T> list, Func<T, T, int> comparer)
    {
        list.Sort(new TComparer<T>(comparer));
    }

    public static void Sort<T>(this List<T> list, Func<T, int, T, int, int> comparer)
    {
        List<T> old = new List<T>(list);

        List<int> oldIs = new List<int>(old.Count);
        list.Clear();
        list.Capacity = old.Count;

        for (int i = 0; i < old.Count; ++i)
        {
            int j;
            for (j = 0; j < list.Count; ++j)
                if (comparer(old[i], i, list[j], oldIs[j]) <= 0)
                    break;

            list.Insert(j, old[i]);
            oldIs.Insert(j, i);
        }
    }

    public static T Min<T, N>(this IEnumerable<T> collection, Func<T, N> toNumber)
        where N : IComparable<N>
    {
        bool first = true;
        T val = default(T);
        N num = default(N);

        foreach (T t in collection)
        {
            N tempNum = toNumber(t);
            if (first || tempNum.CompareTo(num) < 0)
            {
                val = t;
                num = tempNum;
                first = false;
            }
        }

        return val;
    }
    public static T Max<T, N>(this IEnumerable<T> collection, Func<T, N> toNumber)
        where N : IComparable<N>
    {
        bool first = true;
        T val = default(T);
        N num = default(N);

        foreach (T t in collection)
        {
            N tempNum = toNumber(t);
            if (first || tempNum.CompareTo(num) > 0)
            {
                val = t;
                num = tempNum;
                first = false;
            }
        }

        return val;
    }
}