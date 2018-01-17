using System;
using System.Collections.Generic;
using UnityEngine;


namespace Generators
{
    public static class GeneratorUtils
    {
        public static bool LinesIntersect(Vector2 line1P, Vector2 line1Dir, Vector2 line2P, Vector2 line2Dir,
                                          ref float line1IntersectT, ref float line2IntersectT)
        {
            //http://www.ahinson.com/algorithms_general/Sections/Geometry/ParametricLineIntersection.pdf
            Vector2 line1P2 = line1P + line1Dir,
                    line2P2 = line2P + line2Dir;
            float x21 = line1P2.x - line1P.x,
                  y21 = line1P2.y - line1P.y,
                  x43 = line2P2.x - line2P.x,
                  y43 = line2P2.y - line2P.y;
            float determinant = (x43 * y21) - (x21 * y43);

            if (determinant == 0.0f)
                return false;

            float invDet = 1.0f / determinant;
            float x31 = line2P.x - line1P.x,
                  y31 = line2P.y - line1P.y;
            line1IntersectT = invDet * ((x43 * y31) - (x31 * y43));
            line2IntersectT = invDet * ((x21 * y31) - (x31 * y21));

            return true;
        }
        public static bool SegmentsIntersect(Vector2 line1_P1, Vector2 line1_P2,
                                             Vector2 line2_P1, Vector2 line2_P2,
                                             ref float segment1IntersectT, ref float segment2IntersectT)
        {
            Vector2 delta1 = line1_P2 - line1_P1,
                    delta2 = line2_P2 - line2_P1;
            float t1 = float.NaN,
                  t2 = float.NaN;
            if (LinesIntersect(line1_P1, delta1, line2_P1, delta2, ref t1, ref t2) &&
                t1 >= 0.0f && t1 <= 1.0f && t2 >= 0.0f && t2 <= 1.0f)
            {
                segment1IntersectT = t1;
                segment2IntersectT = t2;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the distance between two line segments.
        /// Also outputs the position on each segment that is as close as possible to the other segment,
        ///    and the "T" value for those positions
        ///    (i.e. posOnLine1 = line1_P1 + (line1T * (line1_P2 - line1_P1))).
        /// </summary>
        public static float DistanceToSegment(Vector2 line1_P1, Vector2 line1_P2,
                                              Vector2 line2_P1, Vector2 line2_P2,
                                              out Vector2 posOnLine1, out Vector2 posOnLine2,
                                              ref float line1T, ref float line2T)
        {
            Vector2 delta1 = line1_P2 - line1_P1,
                    delta2 = line2_P2 - line2_P1;

            //Edge case: lines are parallel.
            if (delta1 == delta2 || delta1 == -delta2)
            {
                float p11 = line1_P1.DistSqr(line2_P1),
                      p12 = line1_P1.DistSqr(line2_P2),
                      p21 = line1_P2.DistSqr(line2_P1),
                      p22 = line1_P2.DistSqr(line2_P2);
                float min;
                if (p11 <= p12 && p11 <= p21 && p11 <= p22)
                {
                    min = p11;
                    posOnLine1 = line1_P1;
                    line1T = 0.0f;
                    posOnLine2 = line2_P1;
                    line2T = 0.0f;
                }
                else if (p12 <= p11 && p12 <= p21 && p12 <= p22)
                {
                    min = p12;
                    posOnLine1 = line1_P1;
                    line1T = 0.0f;
                    posOnLine2 = line2_P2;
                    line2T = 1.0f;
                }
                else if (p21 <= p11 && p21 <= p12 && p21 <= p22)
                {
                    min = p21;
                    posOnLine1 = line1_P2;
                    line1T = 1.0f;
                    posOnLine2 = line2_P1;
                    line2T = 0.0f;
                }
                else
                {
                    min = p22;
                    posOnLine1 = line1_P2;
                    line1T = 1.0f;
                    posOnLine2 = line2_P2;
                    line2T = 1.0f;
                }

                return Mathf.Sqrt(min);
            }

            //Not parallel, so the lines themselves definitely intersect somewhere.
            LinesIntersect(line1_P1, delta1, line2_P2, delta2, ref line1T, ref line2T);

            //Clamp the points on both lines to be in the actual segment.
            line1T = Mathf.Clamp01(line1T);
            line2T = Mathf.Clamp01(line2T);

            //Compute and return the distance.
            posOnLine1 = line1_P1 + (delta1 * line1T);
            posOnLine2 = line2_P1 + (delta2 * line2T);
            return posOnLine1.DistSqr(posOnLine2);
        }


        /// <summary>
        /// Adds mesh data for the given road segment to the end of the given collection of mesh data.
        /// </summary>
        public static void GenerateRoad(Vector3 start, Vector3 end, float width,
                                        List<Vector3> outPoses, List<Vector3> outNormals,
                                        List<Vector2> outUVs, float uvStartY, float uvEndY,
                                        List<int> outIndices)
        {
            int startI = outPoses.Count;

            Vector3 perp = Vector3.Cross(start, end),
                    perpW = perp * width;
            Vector3 normal = new Vector3(0.0f, 1.0f, 0.0f);

            outPoses.AddRange(new Vector3[] { start - perpW, start + perpW,
                                              end - perpW, end + perpW });
            outNormals.AddRange(new Vector3[] { normal, normal,
                                                normal, normal });
            outUVs.AddRange(new Vector2[] { new Vector2(0.0f, uvStartY), new Vector2(1.0f, uvStartY),
                                            new Vector2(0.0f, uvEndY), new Vector2(1.0f, uvEndY) });
            outIndices.AddRange(new int[] { startI, startI + 1, startI + 2,
				                            startI + 1, startI + 2, startI + 3 });
        }
    }
}