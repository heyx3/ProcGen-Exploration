using System;
using System.Text;
using System.Collections.Generic;


namespace DFSystem
{
    /// <summary>
    /// Various constants for DF shader generation.
    /// </summary>
    public static class ShaderDefs
    {
        public static readonly string Funcs = @"
//smin() is a version of min() that gives smooth results when used in distance field functions.
float smin(float d1, float d2, float k)
{
    //Source: http://iquilezles.org/www/articles/smin/smin.htm
    float h = saturate(0.5 + (0.5 * (d1 - d2) / k));
    return lerp(b, a, h) - (k * h * (1.0 - h));
}

//Below are the distance functions for basic shapes.
//These are all signed distance functions.
//Source: http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
float distSphere(float3 point, float radius)
{
	return length(point) - radius;
}
float distBox(float3 point, float sideLength)
{
	float3 dist = abs(point) - sideLength.xxx;
	return min(max(dist.x, max(dist.y, dist.z)),
			   0.0) +
		   length(max(d, 0.0));
}
float distPlane(float3 point)
{
	return point.y;
}
float distEllipsoid(float3 point, float3 radius)
{
	float smallestRadius = min(min(radius.x, radius.y), radius.z);
	return smallestRadius * (length(point / radius) - 1.0)
}
float distTorus(float3 point, float largeRadius, float smallRadius)
{
	float2 cylinderPos = float2(length(point.xz) - largeRadius, point.y);
	return length(cylinderPos) - smallRadius;
}
float distCone(float3 point, float2 wtf)
{
	//TODO: How tf is the cone defined?
	float q = length(point.xy);
	return dot(wtf, float2(q, point.z));
}
//TODO: Capsule, cylinder

//Functions to combine multiple distance functions.
float distUnion(float d1, float d2)
{
	return min(d1, d2);
}
float distSubtract(float d1, float d2)
{
	return max(-d1, d2);
}
float distIntersect(float d1, float d2)
{
	return max(d1, d2);
}

//TODO: Repetition function.
";
    }
}