#ifndef HairyBall_
#define HairyBall_

void HairyBall_float(float3 uvIn, float pi, out float2 uvOut)
{
    float v = uvIn.z * .5 + .5;
    // Remap to [0 : .5]
    float u = uvIn.x * .25 + .25;
    
    uvOut = float2(u, v);

}
#endif