#include "Assets/FloatHashers.cginc"

#define PI (3.14159265359)
#define TWO_PI (PI * 2.0)


//These functions generate 1 or 2 Gaussian random values
//    given some uniform random values between 0 and 1.
//The default mean is 0 and the default standard deviation is 1.

float2 gaussian2(float2 uniformRandVals)
{
    //Box-Muller method.
    float d1 = sqrt(-2.0 * log(uniformRandVals.x + 0.000001)),
          d2 = TWO_PI * uniformRandVals.y;
    return float2(d1 * cos(d2), d1 * sin(d2));
}
float2 gaussian2(float mean, float deviation, float2 uniformRandomVals)
{
    return mean + (deviation * gaussian2(uniformRandomVals));
}

float gaussian(float3 uniformRandVals)
{
    float2 gaussianRands = gaussian2(uniformRandVals.xy);
    return (uniformRandVals.z > 0.5) ? gaussianRands.x : gaussianRands.y;
}
float gaussian(float mean, float deviation, float3 uniformRandomVals)
{
    return mean + (deviation * gaussian(uniformRandomVals));
}

#undef PI
#undef TWO_PI