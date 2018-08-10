//Generates 1-4 white noise values simultaneously by hashing a set of 1-4 floats.

//The idea is taken from this shader: https://www.shadertoy.com/view/4djSRW

#define _HASH(p4, swizzle) \
		p4 = frac(p4 * float4(443.897, 441.423, 437.195, 444.129)); \
		p4 += dot(p4, p4.wzxy + 19.19); \
		return frac(dot(p.xyzw, p.zwxy) * p.swizzle);

float _hashTo1(float4 p)
{
    _HASH(p, x);
}
float2 _hashTo2(float4 p)
{
    _HASH(p, xy);
}
float3 _hashTo3(float4 p)
{
    _HASH(p, xyz);
}
float4 _hashTo4(float4 p)
{
    _HASH(p, xyzw);
}
#undef _HASH

float  hashTo1(float p) { return _hashTo1(p.xxxx); }
float  hashTo1(float2 p) { return _hashTo1(p.xyxy); }
float  hashTo1(float3 p) { return _hashTo1(p.xyzx); }
float  hashTo1(float4 p) { return _hashTo1(p); }

float2 hashTo2(float p) { return _hashTo2(p.xxxx); }
float2 hashTo2(float2 p) { return _hashTo2(p.xyxy); }
float2 hashTo2(float3 p) { return _hashTo2(p.xyzx); }
float2 hashTo2(float4 p) { return _hashTo2(p); }

float3 hashTo3(float p) { return _hashTo3(p.xxxx); }
float3 hashTo3(float2 p) { return _hashTo3(p.xyxy); }
float3 hashTo3(float3 p) { return _hashTo3(p.xyzx); }
float3 hashTo3(float4 p) { return _hashTo3(p); }

float4 hashTo4(float p) { return _hashTo4(p.xxxx); }
float4 hashTo4(float2 p) { return _hashTo4(p.xyxy); }
float4 hashTo4(float3 p) { return _hashTo4(p.xyzx); }
float4 hashTo4(float4 p) { return _hashTo4(p); }