static const int _maxSiteCount = 100;
static const int _maxBiomeCount = 80;
uniform int _Sites;
uniform float2 _Regions[_maxSiteCount];
uniform float SurfaceIDs[_maxSiteCount];
uniform float CaveIDs[_maxSiteCount];
uniform float SurfaceHeight[_maxSiteCount];
uniform float AdditiveHeightLimit[_maxSiteCount];

uniform int SurfaceGroundColorLength[_maxBiomeCount];
uniform float4 SurfaceGroundC0[_maxBiomeCount];
uniform float4 SurfaceGroundC1[_maxBiomeCount];
uniform float4 SurfaceGroundC2[_maxBiomeCount];
uniform float4 SurfaceGroundC3[_maxBiomeCount];
uniform float4 SurfaceGroundC4[_maxBiomeCount];
uniform float4 SurfaceGroundC5[_maxBiomeCount];
uniform float4 SurfaceGroundC6[_maxBiomeCount];
uniform float4 SurfaceGroundC7[_maxBiomeCount];

uniform int SurfaceWallColorLength[_maxBiomeCount];
uniform float4 SurfaceWallC0[_maxBiomeCount];
uniform float4 SurfaceWallC1[_maxBiomeCount];
uniform float4 SurfaceWallC2[_maxBiomeCount];
uniform float4 SurfaceWallC3[_maxBiomeCount];
uniform float4 SurfaceWallC4[_maxBiomeCount];
uniform float4 SurfaceWallC5[_maxBiomeCount];
uniform float4 SurfaceWallC6[_maxBiomeCount];
uniform float4 SurfaceWallC7[_maxBiomeCount];

uniform int CaveGroundColorLength[_maxBiomeCount];
uniform float4 CaveGroundC0[_maxBiomeCount];
uniform float4 CaveGroundC1[_maxBiomeCount];
uniform float4 CaveGroundC2[_maxBiomeCount];
uniform float4 CaveGroundC3[_maxBiomeCount];
uniform float4 CaveGroundC4[_maxBiomeCount];
uniform float4 CaveGroundC5[_maxBiomeCount];
uniform float4 CaveGroundC6[_maxBiomeCount];
uniform float4 CaveGroundC7[_maxBiomeCount];

uniform int CaveWallColorLength[_maxBiomeCount];
uniform float4 CaveWallC0[_maxBiomeCount];
uniform float4 CaveWallC1[_maxBiomeCount];
uniform float4 CaveWallC2[_maxBiomeCount];
uniform float4 CaveWallC3[_maxBiomeCount];
uniform float4 CaveWallC4[_maxBiomeCount];
uniform float4 CaveWallC5[_maxBiomeCount];
uniform float4 CaveWallC6[_maxBiomeCount];
uniform float4 CaveWallC7[_maxBiomeCount];

float InverseLerp(float A, float B, float T)
{
    return (T - A) / (B - A);
}

//float4 BlendGradient(float4 c0, float4 c1)
//{
    
//}


void Voronoi_float(float x, float y, out float noise, out int regionIndex0, out int regionIndex1, out int regionIndex2)
{
    //Get the 3 closest sites, brute-force method, need to create kdtrees
    float closestDistance = 999999;
    float secondClosestDistance = 999999;
    float thirdClosestDistance = 999999;
    float2 closestSiteCoord;
    float2 secondClosestSiteCoord;
    float2 thirdClosestSiteCoord;

    for (int i = 0; i < _Sites; i++)
    {
        float dist = distance(_Regions[i], float2(x, y));

        if (dist < closestDistance)
        {
            thirdClosestDistance = secondClosestDistance;
            thirdClosestSiteCoord = secondClosestSiteCoord;
            regionIndex2 = regionIndex1;

            secondClosestDistance = closestDistance;
            secondClosestSiteCoord = closestSiteCoord;
            regionIndex1 = regionIndex0;

            closestDistance = dist;
            closestSiteCoord = _Regions[i];
            regionIndex0 = i;

        }

        if (dist < secondClosestDistance && dist > closestDistance)
        {
            thirdClosestDistance = secondClosestDistance;
            thirdClosestSiteCoord = secondClosestSiteCoord;
            regionIndex2 = regionIndex1;

            secondClosestDistance = dist;
            secondClosestSiteCoord = _Regions[i];
            regionIndex1 = i;
        }

        if (dist < thirdClosestDistance && dist > secondClosestDistance)
        {
            thirdClosestDistance = dist;
            thirdClosestSiteCoord = _Regions[i];
            regionIndex2 = i;
        }
    }

    noise = 1 - (closestDistance / secondClosestDistance);
}

void DummyVoronoi_float(float3 worldPosition, int regionIndex0, int regionIndexK, out float noise)
{
    float closestDistance = distance(_Regions[regionIndex0], worldPosition.xz);
    float closestKDistance = distance(_Regions[regionIndexK], worldPosition.xz);
    //noise = 1 - (closestDistance / closestKDistance);
    float why = closestDistance / closestKDistance;
    noise = 1 - why;
}

void BiomeBlend_float(int regionIndex, out int ignore, out Gradient surfaceGroundGradient, out Gradient surfaceWallGradient, out Gradient caveGroundGradient, out Gradient caveWallGradient)
{
    ignore = regionIndex;
    surfaceGroundGradient = NewGradient(0, SurfaceGroundColorLength[regionIndex], 2, SurfaceGroundC0[regionIndex], SurfaceGroundC1[regionIndex], SurfaceGroundC2[regionIndex], SurfaceGroundC3[regionIndex], SurfaceGroundC4[regionIndex], SurfaceGroundC5[regionIndex], SurfaceGroundC6[regionIndex], SurfaceGroundC7[regionIndex], float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    surfaceWallGradient = NewGradient(0, SurfaceWallColorLength[regionIndex], 2, SurfaceWallC0[regionIndex], SurfaceWallC1[regionIndex], SurfaceWallC2[regionIndex], SurfaceWallC3[regionIndex], SurfaceWallC4[regionIndex], SurfaceWallC5[regionIndex], SurfaceWallC6[regionIndex], SurfaceWallC7[regionIndex], float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    caveGroundGradient = NewGradient(0, CaveGroundColorLength[regionIndex], 2, CaveGroundC0[regionIndex], CaveGroundC1[regionIndex], CaveGroundC2[regionIndex], CaveGroundC3[regionIndex], CaveGroundC4[regionIndex], CaveGroundC5[regionIndex], CaveGroundC6[regionIndex], CaveGroundC7[regionIndex], float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    caveWallGradient = NewGradient(0, CaveWallColorLength[regionIndex], 2, CaveWallC0[regionIndex], CaveWallC1[regionIndex], CaveWallC2[regionIndex], CaveWallC3[regionIndex], CaveWallC4[regionIndex], CaveWallC5[regionIndex], CaveWallC6[regionIndex], CaveWallC7[regionIndex], float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
}

void SeperateBiomeLayers_float(float slope, float voronoiBaseHeight, float3 worldPosition, Gradient surfaceGroundGradient, Gradient surfaceWallGradient, Gradient caveGroundGradient, Gradient caveWallGradient, out int ignore, out Gradient outGradient, out float gradientSample)
{
    //float4 c0 = SurfaceGroundC0[regionIndex0];
    ///*Biome blend first pass*/
    //if (voronoiBaseHeight <= 0.3f)
    //{
    //    float sample = InverseLerp(0, 0.3f, voronoiBaseHeight);
        
    //    float3 surfaceGroundGradientMid = (SurfaceGroundC0[regionIndex1].rgb + SurfaceGroundC0[regionIndex0].rgb) * 0.5f;
    //    float3 uka = lerp(surfaceGroundGradientMid, SurfaceGroundC0[regionIndex0].rgb, sample);
    //    c0 = float4(uka.rgb, SurfaceGroundC0[regionIndex0].a);
    //    //surfaceGroundGradient = NewGradient(0, SurfaceGroundColorLength[regionIndex0], 2, float4(uka.rgb, surfaceGroundGradient.colors[0].a), SurfaceGroundC1[regionIndex0], SurfaceGroundC2[regionIndex0], SurfaceGroundC3[regionIndex0], SurfaceGroundC4[regionIndex0], SurfaceGroundC5[regionIndex0], SurfaceGroundC6[regionIndex0], SurfaceGroundC7[regionIndex0], float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
        
    //    ///*Biome blend second pass*/
    //    ////If the closest region's type is the same as the second closest region's type
    //    //if (SurfaceID[regionIndex0] == SurfaceID[regionIndex1])
    //    //{
    //    //    //If the closest region's type is not the same as the third closest region's type
    //    //    if (SurfaceID[regionIndex0] != SurfaceID[regionIndex2])
    //    //    {
    //    //        float secondaryBlendNoise = BlendDistance(worldPosition.xz, _Regions[regionIndex0], _Regions[regionIndex2]);
    //    //        if (secondaryBlendNoise <= 0.3f)
    //    //        {
    //    //            sample = InverseLerp(0, 0.3f, secondaryBlendNoise);
                    
    //    //            surfaceGroundGradientMid = (SurfaceGroundC0[regionIndex2].rgb + surfaceGroundGradient.colors[0].rgb) * 0.5f;
    //    //            uka = lerp(surfaceGroundGradientMid, surfaceGroundGradient.colors[0].rgb, sample);
    //    //            surfaceGroundGradient.colors[0] = float4(uka.r, uka.g, uka.b, surfaceGroundGradient.colors[0].a);
    //    //        }
    //    //    }
    //    //}
    //}
    
    //Gradient surfaceGroundGradient = NewGradient(0, SurfaceGroundColorLength[regionIndex0], 2, c0, SurfaceGroundC1[regionIndex0], SurfaceGroundC2[regionIndex0], SurfaceGroundC3[regionIndex0], SurfaceGroundC4[regionIndex0], SurfaceGroundC5[regionIndex0], SurfaceGroundC6[regionIndex0], SurfaceGroundC7[regionIndex0], float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    //Gradient surfaceWallGradient = NewGradient(0, SurfaceWallColorLength[regionIndex0], 2, SurfaceWallC0[regionIndex0], SurfaceWallC1[regionIndex0], SurfaceWallC2[regionIndex0], SurfaceWallC3[regionIndex0], SurfaceWallC4[regionIndex0], SurfaceWallC5[regionIndex0], SurfaceWallC6[regionIndex0], SurfaceWallC7[regionIndex0], float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    //Gradient caveGroundGradient = NewGradient(0, CaveGroundColorLength[regionIndex0], 2, CaveGroundC0[regionIndex0], CaveGroundC1[regionIndex0], CaveGroundC2[regionIndex0], CaveGroundC3[regionIndex0], CaveGroundC4[regionIndex0], CaveGroundC5[regionIndex0], CaveGroundC6[regionIndex0], CaveGroundC7[regionIndex0], float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    //Gradient caveWallGradient = NewGradient(0, CaveWallColorLength[regionIndex0], 2, CaveWallC0[regionIndex0], CaveWallC1[regionIndex0], CaveWallC2[regionIndex0], CaveWallC3[regionIndex0], CaveWallC4[regionIndex0], CaveWallC5[regionIndex0], CaveWallC6[regionIndex0], CaveWallC7[regionIndex0], float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    
    
    ignore = slope;
    //Ground layer
    if (slope > 0)
    {
        //Surface layer
        if (worldPosition.y > -3)
        {
            gradientSample = InverseLerp(-3, 18, worldPosition.y);
            outGradient = surfaceGroundGradient;
        }

        //Transition
        if (worldPosition.y <= -3 && worldPosition.y >= -3 - 3)
        {
            gradientSample = InverseLerp(-3, -3 - 3, worldPosition.y);
            outGradient = NewGradient(0, 2, 2, float4(surfaceGroundGradient.colors[0].r, surfaceGroundGradient.colors[0].g, surfaceGroundGradient.colors[0].b, 0), float4(caveGroundGradient.colors[0].r, caveGroundGradient.colors[0].g, caveGroundGradient.colors[0].b, 1), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
        }

        //Cave layer
        if (worldPosition.y < -3 - 3)
        {
            gradientSample = InverseLerp(-3 - 3, -60, worldPosition.y);
            outGradient = caveGroundGradient;
        }
    }
    //Wall layer
    else
    {
        //Surface layer
        if (worldPosition.y > -3)
        {
            gradientSample = InverseLerp(-3, 18, worldPosition.y);
            outGradient = surfaceWallGradient;
        }

        //Transition
        if (worldPosition.y <= -3 && worldPosition.y >= -3 - 3)
        {
            gradientSample = InverseLerp(-3, -3 - 3, worldPosition.y);
            outGradient = NewGradient(0, 2, 2, float4(surfaceWallGradient.colors[0].r, surfaceWallGradient.colors[0].g, surfaceWallGradient.colors[0].b, 0), float4(caveWallGradient.colors[0].r, caveWallGradient.colors[0].g, caveWallGradient.colors[0].b, 1), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
        }

        //Cave layer
        if (worldPosition.y < -3 - 3)
        {
            gradientSample = InverseLerp(-3 - 3, -60, worldPosition.y);
            outGradient = caveWallGradient;
        }
    }
}