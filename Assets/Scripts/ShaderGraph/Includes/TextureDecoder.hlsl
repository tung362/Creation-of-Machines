#ifndef Texture_Decoder_
#define Texture_Decoder_

/*Decode data from texture*/
float DecodeFloat(Texture2D dataTexture, int index, out int nextIndex)
{
    nextIndex = index + 1;
    return dataTexture.Load(int3(index, 0, 0)).x;
}

float2 DecodeFloat2(Texture2D dataTexture, int index, out int nextIndex)
{
    nextIndex = index + 1;
    return dataTexture.Load(int3(index, 0, 0)).xy;
}

float3 DecodeFloat3(Texture2D dataTexture, int index, out int nextIndex)
{
    nextIndex = index + 1;
    return dataTexture.Load(int3(index, 0, 0)).xyz;
}

float4 DecodeFloat4(Texture2D dataTexture, int index, out int nextIndex)
{
    nextIndex = index + 1;
    return dataTexture.Load(int3(index, 0, 0)).xyzw;
}
#endif