﻿#ifndef Gradient_
#define Gradient_

#include "TextureDecoder.hlsl"

/*Consts*/
static const int GradientSize = 9;

/*Structs*/
struct GradientFragGPU
{
    int ColorLength;
    float4 C0;
    float4 C1;
    float4 C2;
    float4 C3;
    float4 C4;
    float4 C5;
    float4 C6;
    float4 C7;
};

/*Mapper*/
int GetGradientIndex(int regionIndex)
{
    return regionIndex * GradientSize;
}

/*Load from texture*/
GradientFragGPU LoadGradient(Texture2D dataTexture, int index, out int nextIndex)
{
    nextIndex = index;
    GradientFragGPU retVal;
    retVal.ColorLength = DecodeFloat(dataTexture, nextIndex, nextIndex);
    retVal.C0 = DecodeFloat4(dataTexture, nextIndex, nextIndex);
    retVal.C1 = DecodeFloat4(dataTexture, nextIndex, nextIndex);
    retVal.C2 = DecodeFloat4(dataTexture, nextIndex, nextIndex);
    retVal.C3 = DecodeFloat4(dataTexture, nextIndex, nextIndex);
    retVal.C4 = DecodeFloat4(dataTexture, nextIndex, nextIndex);
    retVal.C5 = DecodeFloat4(dataTexture, nextIndex, nextIndex);
    retVal.C6 = DecodeFloat4(dataTexture, nextIndex, nextIndex);
    retVal.C7 = DecodeFloat4(dataTexture, nextIndex, nextIndex);
    return retVal;
}

GradientFragGPU LoadGradient(Texture2D dataTexture, int regionIndex)
{
    int index = GetGradientIndex(regionIndex);
    return LoadGradient(dataTexture, index, index);
}


/*Logic*/
GradientFragGPU Compare(GradientFragGPU gradient0, GradientFragGPU gradient1, float result)
{
    GradientFragGPU gradientFragGPU;
    gradientFragGPU.ColorLength = lerp(gradient0.ColorLength, gradient1.ColorLength, result);
    gradientFragGPU.C0 = lerp(gradient0.C0, gradient1.C0, result);
    gradientFragGPU.C1 = lerp(gradient0.C1, gradient1.C1, result);
    gradientFragGPU.C2 = lerp(gradient0.C2, gradient1.C2, result);
    gradientFragGPU.C3 = lerp(gradient0.C3, gradient1.C3, result);
    gradientFragGPU.C4 = lerp(gradient0.C4, gradient1.C4, result);
    gradientFragGPU.C5 = lerp(gradient0.C5, gradient1.C5, result);
    gradientFragGPU.C6 = lerp(gradient0.C6, gradient1.C6, result);
    gradientFragGPU.C7 = lerp(gradient0.C7, gradient1.C7, result);
    return gradientFragGPU;
}

/*Utils*/
GradientFragGPU BlendGradient(GradientFragGPU gradient0, GradientFragGPU gradient1, float sample)
{
    float4 C0Mid = (gradient0.C0 + gradient1.C0) * 0.5f;
    float4 C1Mid = (gradient0.C1 + gradient1.C1) * 0.5f;
    float4 C2Mid = (gradient0.C2 + gradient1.C2) * 0.5f;
    float4 C3Mid = (gradient0.C3 + gradient1.C3) * 0.5f;
    float4 C4Mid = (gradient0.C4 + gradient1.C4) * 0.5f;
    float4 C5Mid = (gradient0.C5 + gradient1.C5) * 0.5f;
    float4 C6Mid = (gradient0.C6 + gradient1.C6) * 0.5f;
    float4 C7Mid = (gradient0.C7 + gradient1.C7) * 0.5f;
    GradientFragGPU gradient;
    gradient.ColorLength = lerp(gradient0.ColorLength, gradient1.ColorLength, step(gradient0.ColorLength, gradient1.ColorLength));
    gradient.C0 = lerp(C0Mid, gradient1.C0, sample);
    gradient.C1 = lerp(C1Mid, gradient1.C1, sample);
    gradient.C2 = lerp(C2Mid, gradient1.C2, sample);
    gradient.C3 = lerp(C3Mid, gradient1.C3, sample);
    gradient.C4 = lerp(C4Mid, gradient1.C4, sample);
    gradient.C5 = lerp(C5Mid, gradient1.C5, sample);
    gradient.C6 = lerp(C6Mid, gradient1.C6, sample);
    gradient.C7 = lerp(C7Mid, gradient1.C7, sample);
    return gradient;
}
#endif