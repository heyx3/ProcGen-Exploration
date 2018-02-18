//Efficiently samples from a linear-filtered texture
//    to get a 5-tap gaussian blur at the given uv value.
//This should be used in two passes, one pass along each axis.
float4 gaussian5Line(sampler2D tex, float2 uv, float2 texel, float2 passDir)
{
    float4 color = tex2D(tex, uv) * 0.29411764705882354;

    const float secondCoefficient = 0.35294117647058826;
    float2 offset = 1.3333333333 * passDir * texel;
    color += tex2D(tex, uv + offset) * secondCoefficient;
    color += tex2D(tex, uv - offset) * secondCoefficient;

    return color;
}

//Efficiently samples from a linear-filtered texture
//    to get a 9-tap gaussian blur at the given uv value.
//This should be used in two passes, one pass along each axis.
float4 gaussian9Line(sampler2D tex, float2 uv, float2 texel, float2 passDir)
{
    float4 color = tex2D(tex, uv) * 0.227027027;

    const float secondCoefficient = 0.3162162162;
    float2 offset1 = 1.3846153846 * passDir * texel;
    color += tex2D(tex, uv + offset1) * secondCoefficient;
    color += tex2D(tex, uv - offset1) * secondCoefficient;

    const float thirdCoefficient = 0.07027027027;
    float2 offset2 = 3.2307692308 * passDir * texel;
    color += tex2D(tex, uv + offset2) * thirdCoefficient;
    color += tex2D(tex, uv - offset2) * thirdCoefficient;

    return color;
}