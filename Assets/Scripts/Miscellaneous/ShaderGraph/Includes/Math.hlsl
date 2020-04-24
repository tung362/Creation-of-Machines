
/*If statement operators, 0 = false, 1 = true*/
float4 IfEqual(float4 x, float4 y)
{
    return 1.0 - abs(sign(x - y));
}

float4 IfNotEqual(float4 x, float4 y)
{
    return abs(sign(x - y));
}

float4 IfGreater(float4 x, float4 y)
{
    return max(sign(x - y), 0.0);
}

float4 IfLess(float4 x, float4 y)
{
    return max(sign(y - x), 0.0);
}

float4 IFGreaterOrEqual(float4 x, float4 y)
{
    return 1.0 - IfLess(x, y);
}

float4 IFLessOrEqual(float4 x, float4 y)
{
    return 1.0 - IfGreater(x, y);
}

float4 And(float4 a, float4 b)
{
    return a * b;
}

float4 Or(float4 a, float4 b)
{
    return min(a + b, 1.0);
}

float4 XOr(float4 a, float4 b)
{
    return (a + b) % 2.0;
}

float4 Not(float4 a)
{
    return 1.0 - a;
}

/*Math*/
float InverseLerp(float A, float B, float T)
{
    return (T - A) / (B - A);
}

/*Compare*/
Gradient Compare(float ifStatementResult, Gradient trueValue, Gradient falseValue)
{
    float colorLength = lerp(falseValue.colorsLength, trueValue.colorsLength, ifStatementResult);
    float4 c0 = lerp(falseValue.colors[0], trueValue.colors[0], ifStatementResult);
    float4 c1 = lerp(falseValue.colors[1], trueValue.colors[1], ifStatementResult);
    float4 c2 = lerp(falseValue.colors[2], trueValue.colors[2], ifStatementResult);
    float4 c3 = lerp(falseValue.colors[3], trueValue.colors[3], ifStatementResult);
    float4 c4 = lerp(falseValue.colors[4], trueValue.colors[4], ifStatementResult);
    float4 c5 = lerp(falseValue.colors[5], trueValue.colors[5], ifStatementResult);
    float4 c6 = lerp(falseValue.colors[6], trueValue.colors[6], ifStatementResult);
    float4 c7 = lerp(falseValue.colors[7], trueValue.colors[7], ifStatementResult);
    Gradient gradient = NewGradient(0, colorLength, 2, c0, c1, c2, c3, c4, c5, c6, c7, float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    return gradient;
}

inline float4 EncodeFloatRGBA(float v)
{
    uint vi = (uint) (v * (256.0f * 256.0f * 256.0f * 256.0f));
    int ex = (int) (vi / (256 * 256 * 256) % 256);
    int ey = (int) ((vi / (256 * 256)) % 256);
    int ez = (int) ((vi / (256)) % 256);
    int ew = (int) (vi % 256);
    float4 e = float4(ex / 255.0f, ey / 255.0f, ez / 255.0f, ew / 255.0f);
    return e;
}

inline float DecodeFloatRGBA(float4 encode)
{
    uint ex = (uint) (encode.x * 255);
    uint ey = (uint) (encode.y * 255);
    uint ez = (uint) (encode.z * 255);
    uint ew = (uint) (encode.w * 255);
    uint v = (ex << 24) + (ey << 16) + (ez << 8) + ew;
    return v / (256.0f * 256.0f * 256.0f * 256.0f);
}