#include "/Includes/Math.hlsl"

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

struct RegionFragGPU
{
    float2 Coord;
    int SurfaceBiomeID;
    int CaveBiomeID;
    //float SurfaceHeight;
    //float SurfaceFloor;
    //float SurfaceAdditiveHeightLimit;
    GradientFragGPU SurfaceGroundGradient;
    GradientFragGPU SurfaceWallGradient;
    GradientFragGPU CaveGroundGradient;
    GradientFragGPU CaveWallGradient;
    int NeighborIndex0;
    int NeighborIndex1;
    int NeighborIndex2;
    int NeighborIndex3;
    int NeighborIndex4;
    int NeighborIndex5;
    int NeighborIndex6;
    int NeighborIndex7;
};
struct Test
{
    float SurfaceHeight;
    float SurfaceFloor;
    float SurfaceAdditiveHeightLimit;
};
StructuredBuffer<RegionFragGPU> RegionFragGPUs;

//uniform float SurfaceHeight[100];
//uniform float SurfaceFloor[100];
//uniform float SurfaceAdditiveHeightLimit[100];

Test LoadTest(Texture2D dataTexture, int regionIndex)
{
    Test test;
    test.SurfaceHeight = DecodeFloatRGBA(dataTexture.Load(int3(regionIndex * 3, 0, 0))) * 300.0f;
    test.SurfaceFloor = DecodeFloatRGBA(dataTexture.Load(int3((regionIndex * 3) + 1, 0, 0))) * 300.0f;
    test.SurfaceAdditiveHeightLimit = DecodeFloatRGBA(dataTexture.Load(int3((regionIndex * 3) + 2, 0, 0))) * 300.0f;
    return test;
}

GradientFragGPU Compare(float ifStatementResult, GradientFragGPU trueValue, GradientFragGPU falseValue)
{
    GradientFragGPU gradientFragGPU;
    gradientFragGPU.ColorLength = lerp(falseValue.ColorLength, trueValue.ColorLength, ifStatementResult);
    gradientFragGPU.C0 = lerp(falseValue.C0, trueValue.C0, ifStatementResult);
    gradientFragGPU.C1 = lerp(falseValue.C1, trueValue.C1, ifStatementResult);
    gradientFragGPU.C2 = lerp(falseValue.C2, trueValue.C2, ifStatementResult);
    gradientFragGPU.C3 = lerp(falseValue.C3, trueValue.C3, ifStatementResult);
    gradientFragGPU.C4 = lerp(falseValue.C4, trueValue.C4, ifStatementResult);
    gradientFragGPU.C5 = lerp(falseValue.C5, trueValue.C5, ifStatementResult);
    gradientFragGPU.C6 = lerp(falseValue.C6, trueValue.C6, ifStatementResult);
    gradientFragGPU.C7 = lerp(falseValue.C7, trueValue.C7, ifStatementResult);
    return gradientFragGPU;
}

float BlendDistance(float2 coord, float2 closest, float2 closestK)
{
    float closestDistance = distance(closest, coord);
    float closestKDistance = distance(closestK, coord);
    return 1 - (closestDistance / closestKDistance);
}

GradientFragGPU BlendGradientFrag(GradientFragGPU gradientFrag1, GradientFragGPU gradientFrag2, float sample)
{
    GradientFragGPU gradientFragGPU;
    float4 C0Mid = (gradientFrag1.C0 + gradientFrag2.C0) * 0.5f;
    float4 C1Mid = (gradientFrag1.C1 + gradientFrag2.C1) * 0.5f;
    float4 C2Mid = (gradientFrag1.C2 + gradientFrag2.C2) * 0.5f;
    float4 C3Mid = (gradientFrag1.C3 + gradientFrag2.C3) * 0.5f;
    float4 C4Mid = (gradientFrag1.C4 + gradientFrag2.C4) * 0.5f;
    float4 C5Mid = (gradientFrag1.C5 + gradientFrag2.C5) * 0.5f;
    float4 C6Mid = (gradientFrag1.C6 + gradientFrag2.C6) * 0.5f;
    float4 C7Mid = (gradientFrag1.C7 + gradientFrag2.C7) * 0.5f;
    
    gradientFragGPU.ColorLength = gradientFrag2.ColorLength;
    gradientFragGPU.C0 = lerp(C0Mid, gradientFrag2.C0, sample);
    gradientFragGPU.C1 = lerp(C1Mid, gradientFrag2.C1, sample);
    gradientFragGPU.C2 = lerp(C2Mid, gradientFrag2.C2, sample);
    gradientFragGPU.C3 = lerp(C3Mid, gradientFrag2.C3, sample);
    gradientFragGPU.C4 = lerp(C4Mid, gradientFrag2.C4, sample);
    gradientFragGPU.C5 = lerp(C5Mid, gradientFrag2.C5, sample);
    gradientFragGPU.C6 = lerp(C6Mid, gradientFrag2.C6, sample);
    gradientFragGPU.C7 = lerp(C7Mid, gradientFrag2.C7, sample);
    
    return gradientFragGPU;
}

void FindClosest3(float3 worldPosition, int biomeIndex, float closestDistanceIn, float secondClosestDistanceIn, float thirdClosestDistanceIn, int regionIndex0In, int regionIndex1In, int regionIndex2In, out float closestDistanceOut, out float secondClosestDistanceOut, out float thirdClosestDistanceOut, out int regionIndex0Out, out int regionIndex1Out, out int regionIndex2Out)
{
    closestDistanceOut = closestDistanceIn;
    secondClosestDistanceOut = secondClosestDistanceIn;
    thirdClosestDistanceOut = thirdClosestDistanceIn;
    regionIndex0Out = regionIndex0In;
    regionIndex1Out = regionIndex1In;
    regionIndex2Out = regionIndex2In;
    
    float dist = distance(RegionFragGPUs[biomeIndex].Coord, worldPosition.xz);
    
    if (dist < closestDistanceOut)
    {
        thirdClosestDistanceOut = secondClosestDistanceOut;
        regionIndex2Out = regionIndex1Out;

        secondClosestDistanceOut = closestDistanceOut;
        regionIndex1Out = regionIndex0Out;

        closestDistanceOut = dist;
        regionIndex0Out = biomeIndex;
    }

    if (dist < secondClosestDistanceOut && dist > closestDistanceOut)
    {
        thirdClosestDistanceOut = secondClosestDistanceOut;
        regionIndex2Out = regionIndex1Out;

        secondClosestDistanceOut = dist;
        regionIndex1Out = biomeIndex;
    }

    if (dist < thirdClosestDistanceOut && dist > secondClosestDistanceOut)
    {
        thirdClosestDistanceOut = dist;
        regionIndex2Out = biomeIndex;
    }
}

void VoronoiNoise_float(float3 worldPosition, int biomeIndex0, int biomeIndex1, int biomeIndex2, int biomeIndex3, out float noiseOut, out int regionIndex0Out, out int regionIndex1Out, out int regionIndex2Out)
{
    float closestDistance = 999999;
    float secondClosestDistance = 999999;
    float thirdClosestDistance = 999999;
    regionIndex0Out = -1;
    regionIndex1Out = -1;
    regionIndex2Out = -1;
    
    //If there is more than 1 closest region (default closest and second closest should be found)
    if (biomeIndex1 != -1)
    {
        FindClosest3(worldPosition, biomeIndex1, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
        if (biomeIndex0 != -1) FindClosest3(worldPosition, biomeIndex0, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
        if (biomeIndex2 != -1) FindClosest3(worldPosition, biomeIndex2, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
        if (biomeIndex3 != -1) FindClosest3(worldPosition, biomeIndex3, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
        
        //If haven't found third closest then do neighbor search
        if (regionIndex2Out == -1)
        {
            if(RegionFragGPUs[regionIndex0Out].NeighborIndex0 != -1) FindClosest3(worldPosition, RegionFragGPUs[regionIndex0Out].NeighborIndex0, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[regionIndex0Out].NeighborIndex1 != -1) FindClosest3(worldPosition, RegionFragGPUs[regionIndex0Out].NeighborIndex1, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[regionIndex0Out].NeighborIndex2 != -1) FindClosest3(worldPosition, RegionFragGPUs[regionIndex0Out].NeighborIndex2, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[regionIndex0Out].NeighborIndex3 != -1) FindClosest3(worldPosition, RegionFragGPUs[regionIndex0Out].NeighborIndex3, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[regionIndex0Out].NeighborIndex4 != -1) FindClosest3(worldPosition, RegionFragGPUs[regionIndex0Out].NeighborIndex4, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[regionIndex0Out].NeighborIndex5 != -1) FindClosest3(worldPosition, RegionFragGPUs[regionIndex0Out].NeighborIndex5, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[regionIndex0Out].NeighborIndex6 != -1) FindClosest3(worldPosition, RegionFragGPUs[regionIndex0Out].NeighborIndex6, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[regionIndex0Out].NeighborIndex7 != -1) FindClosest3(worldPosition, RegionFragGPUs[regionIndex0Out].NeighborIndex7, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
        }
    }
    else
    {
        //Neighbor search
        if (biomeIndex0 != -1)
        {
            FindClosest3(worldPosition, biomeIndex0, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[biomeIndex0].NeighborIndex0 != -1) FindClosest3(worldPosition, RegionFragGPUs[biomeIndex0].NeighborIndex0, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[biomeIndex0].NeighborIndex1 != -1) FindClosest3(worldPosition, RegionFragGPUs[biomeIndex0].NeighborIndex1, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[biomeIndex0].NeighborIndex2 != -1) FindClosest3(worldPosition, RegionFragGPUs[biomeIndex0].NeighborIndex2, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[biomeIndex0].NeighborIndex3 != -1) FindClosest3(worldPosition, RegionFragGPUs[biomeIndex0].NeighborIndex3, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[biomeIndex0].NeighborIndex4 != -1) FindClosest3(worldPosition, RegionFragGPUs[biomeIndex0].NeighborIndex4, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[biomeIndex0].NeighborIndex5 != -1) FindClosest3(worldPosition, RegionFragGPUs[biomeIndex0].NeighborIndex5, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[biomeIndex0].NeighborIndex6 != -1) FindClosest3(worldPosition, RegionFragGPUs[biomeIndex0].NeighborIndex6, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(RegionFragGPUs[biomeIndex0].NeighborIndex7 != -1) FindClosest3(worldPosition, RegionFragGPUs[biomeIndex0].NeighborIndex7, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
        }
    }
    noiseOut = 1 - (closestDistance / secondClosestDistance);
}

void SeperateBiomeLayers_float(Texture2D test, float slope, float3 worldPosition, float noise, int regionIndex0, int regionIndex1, int regionIndex2, out float gradientSampleOut, out Gradient GradientOut/*, out float testSampleOut*/)
{
    Test test0 = LoadTest(test, regionIndex0);
    Test test1 = LoadTest(test, regionIndex1);
    Test test2 = LoadTest(test, regionIndex2);
    float surfaceOffset0 = test0.SurfaceFloor - test0.SurfaceHeight;
    float surfaceOffset1 = test1.SurfaceFloor - test1.SurfaceHeight;
    float surfaceOffset2 = test2.SurfaceFloor - test2.SurfaceHeight;
    /*Biome blend first pass*/
    float sample = InverseLerp(0, 0.3f, noise);
    
    //Surface and cave
    float ifStatement = IFLessOrEqual(noise, 0.3f);
    GradientFragGPU SurfaceGroundGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex1].SurfaceGroundGradient, RegionFragGPUs[regionIndex0].SurfaceGroundGradient, sample), RegionFragGPUs[regionIndex0].SurfaceGroundGradient);
    GradientFragGPU SurfaceWallGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex1].SurfaceWallGradient, RegionFragGPUs[regionIndex0].SurfaceWallGradient, sample), RegionFragGPUs[regionIndex0].SurfaceWallGradient);
    GradientFragGPU CaveGroundGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex1].CaveGroundGradient, RegionFragGPUs[regionIndex0].CaveGroundGradient, sample), RegionFragGPUs[regionIndex0].CaveGroundGradient);
    GradientFragGPU CaveWallGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex1].CaveWallGradient, RegionFragGPUs[regionIndex0].CaveWallGradient, sample), RegionFragGPUs[regionIndex0].CaveWallGradient);
    float surfaceHeightMid = (test1.SurfaceHeight + test0.SurfaceHeight) * 0.5f;
    float surfaceFloorMid = (test1.SurfaceFloor + test0.SurfaceFloor) * 0.5f;
    float surfaceAdditiveHeightLimitMid = (test1.SurfaceAdditiveHeightLimit + test0.SurfaceAdditiveHeightLimit) * 0.5f;
    float surfaceOffsetMid = (surfaceOffset1 + surfaceOffset0) * 0.5f;
    
    float surfaceHeight = lerp(test0.SurfaceHeight, lerp(surfaceHeightMid, test0.SurfaceHeight, sample), ifStatement);
    float surfaceFloor = lerp(test0.SurfaceFloor, lerp(surfaceFloorMid, test0.SurfaceFloor, sample), ifStatement);
    float surfaceAdditiveHeightLimit = lerp(test0.SurfaceAdditiveHeightLimit, lerp(surfaceAdditiveHeightLimitMid, test0.SurfaceAdditiveHeightLimit, sample), ifStatement);
    float surfaceOffset = lerp(surfaceOffset0, lerp(surfaceOffsetMid, surfaceOffset0, sample), ifStatement);
    
    /*Biome blend second pass*/
    float secondaryBlendNoise = BlendDistance(worldPosition.xz, RegionFragGPUs[regionIndex0].Coord, RegionFragGPUs[regionIndex2].Coord);
    sample = InverseLerp(0, 0.3f, secondaryBlendNoise);
    
    //Surface
    ifStatement = And(IFLessOrEqual(secondaryBlendNoise, 0.3f), And(IfEqual(RegionFragGPUs[regionIndex0].SurfaceBiomeID, RegionFragGPUs[regionIndex1].SurfaceBiomeID), IfNotEqual(RegionFragGPUs[regionIndex0].SurfaceBiomeID, RegionFragGPUs[regionIndex2].SurfaceBiomeID)));
    SurfaceGroundGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex2].SurfaceGroundGradient, RegionFragGPUs[regionIndex0].SurfaceGroundGradient, sample), SurfaceGroundGradient);
    SurfaceWallGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex2].SurfaceWallGradient, RegionFragGPUs[regionIndex0].SurfaceWallGradient, sample), SurfaceWallGradient);
    surfaceHeightMid = (test2.SurfaceHeight + test0.SurfaceHeight) * 0.5f;
    surfaceFloorMid = (test2.SurfaceFloor + test0.SurfaceFloor) * 0.5f;
    surfaceAdditiveHeightLimitMid = (test2.SurfaceAdditiveHeightLimit + test0.SurfaceAdditiveHeightLimit) * 0.5f;
    surfaceOffsetMid = (surfaceOffset2 + surfaceOffset0) * 0.5f;
    
    surfaceHeight = lerp(surfaceHeight, lerp(surfaceHeightMid, test0.SurfaceHeight, sample), ifStatement);
    surfaceFloor = lerp(surfaceFloor, lerp(surfaceFloorMid, test0.SurfaceFloor, sample), ifStatement);
    surfaceAdditiveHeightLimit = lerp(surfaceAdditiveHeightLimit, lerp(surfaceAdditiveHeightLimitMid, test0.SurfaceAdditiveHeightLimit, sample), ifStatement);
    surfaceOffset = lerp(surfaceOffset, lerp(surfaceOffsetMid, surfaceOffset0, sample), ifStatement);
    
    //if (RegionFragGPUs[regionIndex0].SurfaceBiomeID != RegionFragGPUs[regionIndex1].SurfaceBiomeID)
    //{
    //    if (RegionFragGPUs[regionIndex0].SurfaceBiomeID != RegionFragGPUs[regionIndex2].SurfaceBiomeID)
    //    {
    //        if (RegionFragGPUs[regionIndex1].SurfaceBiomeID != RegionFragGPUs[regionIndex2].SurfaceBiomeID)
    //        {
    //            float thirdBlendNoise = BlendDistance(worldPosition.xz, RegionFragGPUs[regionIndex1].Coord, RegionFragGPUs[regionIndex2].Coord);
    //            float sample1 = InverseLerp(0, 0.3f, noise);
    //            float sample2 = InverseLerp(0, 0.3f, secondaryBlendNoise);
    //            float sample3 = InverseLerp(0, 0.3f, thirdBlendNoise);
    //            GradientFragGPU testBlend1 = BlendGradientFrag(RegionFragGPUs[regionIndex1].SurfaceGroundGradient, RegionFragGPUs[regionIndex0].SurfaceGroundGradient, sample1);
    //            GradientFragGPU testBlend2 = BlendGradientFrag(RegionFragGPUs[regionIndex2].SurfaceGroundGradient, RegionFragGPUs[regionIndex0].SurfaceGroundGradient, sample2);
    //            GradientFragGPU testBlend3 = BlendGradientFrag(RegionFragGPUs[regionIndex2].SurfaceGroundGradient, RegionFragGPUs[regionIndex1].SurfaceGroundGradient, sample3);
                
    //            GradientFragGPU result = BlendGradientFrag(RegionFragGPUs[regionIndex2].SurfaceGroundGradient, testBlend1, sample2);
                
    //            if (noise <= 0.3f /*&& secondaryBlendNoise <= 0.3f*/ && thirdBlendNoise <= 0.3f)
    //            {
    //                SurfaceGroundGradient = BlendGradientFrag(RegionFragGPUs[regionIndex2].SurfaceGroundGradient, RegionFragGPUs[regionIndex1].SurfaceGroundGradient, sample3);
    //                SurfaceGroundGradient = BlendGradientFrag(SurfaceGroundGradient, RegionFragGPUs[regionIndex0].SurfaceGroundGradient, sample1);
    //            }
    //        }
    //    }
    //}
    
    //Cave
    ifStatement = And(IFLessOrEqual(secondaryBlendNoise, 0.3f), And(IfEqual(RegionFragGPUs[regionIndex0].CaveBiomeID, RegionFragGPUs[regionIndex1].CaveBiomeID), IfNotEqual(RegionFragGPUs[regionIndex0].CaveBiomeID, RegionFragGPUs[regionIndex2].CaveBiomeID)));
    CaveGroundGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex2].CaveGroundGradient, RegionFragGPUs[regionIndex0].CaveGroundGradient, sample), CaveGroundGradient);
    CaveWallGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex2].CaveWallGradient, RegionFragGPUs[regionIndex0].CaveWallGradient, sample), CaveWallGradient);
    
    Gradient surfaceGroundGradient = NewGradient(0, SurfaceGroundGradient.ColorLength, 2, SurfaceGroundGradient.C0, SurfaceGroundGradient.C1, SurfaceGroundGradient.C2, SurfaceGroundGradient.C3, SurfaceGroundGradient.C4, SurfaceGroundGradient.C5, SurfaceGroundGradient.C6, SurfaceGroundGradient.C7, float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    Gradient surfaceWallGradient = NewGradient(0, SurfaceWallGradient.ColorLength, 2, SurfaceWallGradient.C0, SurfaceWallGradient.C1, SurfaceWallGradient.C2, SurfaceWallGradient.C3, SurfaceWallGradient.C4, SurfaceWallGradient.C5, SurfaceWallGradient.C6, SurfaceWallGradient.C7, float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    Gradient caveGroundGradient = NewGradient(0, CaveGroundGradient.ColorLength, 2, CaveGroundGradient.C0, CaveGroundGradient.C1, CaveGroundGradient.C2, CaveGroundGradient.C3, CaveGroundGradient.C4, CaveGroundGradient.C5, CaveGroundGradient.C6, CaveGroundGradient.C7, float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    Gradient caveWallGradient = NewGradient(0, CaveWallGradient.ColorLength, 2, CaveWallGradient.C0, CaveWallGradient.C1, CaveWallGradient.C2, CaveWallGradient.C3, CaveWallGradient.C4, CaveWallGradient.C5, CaveWallGradient.C6, CaveWallGradient.C7, float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    
    //gradientSampleOut = 0;
    //GradientOut = surfaceGroundGradient;
    
    ///*Ground layer*/
    //float ifSlope = IfGreater(slope, 0);
    
    ////Surface layer
    //ifStatement = And(ifSlope, IfGreater(worldPosition.y, -3));
    //gradientSampleOut = lerp(gradientSampleOut, InverseLerp(-3, 18, worldPosition.y), ifStatement);
    //GradientOut = Compare(ifStatement, surfaceGroundGradient, GradientOut);
    
    ////Transition
    //ifStatement = And(ifSlope, And(IFLessOrEqual(worldPosition.y, -3), IFGreaterOrEqual(worldPosition.y, -3 - 3)));
    //gradientSampleOut = lerp(gradientSampleOut, InverseLerp(-3, -3 - 3, worldPosition.y), ifStatement);
    //GradientOut = Compare(ifStatement, NewGradient(0, 2, 2, float4(surfaceGroundGradient.colors[0].r, surfaceGroundGradient.colors[0].g, surfaceGroundGradient.colors[0].b, 0), float4(caveGroundGradient.colors[0].r, caveGroundGradient.colors[0].g, caveGroundGradient.colors[0].b, 1), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0)), GradientOut);
    
    ////Cave layer
    //ifStatement = And(ifSlope, IfLess(worldPosition.y, -3 - 3));
    //gradientSampleOut = lerp(gradientSampleOut, InverseLerp(-3 - 3, -60, worldPosition.y), ifStatement);
    //GradientOut = Compare(ifStatement, caveGroundGradient, GradientOut);
    
    ///*Wall layer*/
    //ifSlope = IFLessOrEqual(slope, 0);
    
    ////Surface layer
    //ifStatement = And(ifSlope, IfGreater(worldPosition.y, -3));
    //gradientSampleOut = lerp(gradientSampleOut, InverseLerp(-3, 18, worldPosition.y), ifStatement);
    //GradientOut = Compare(ifStatement, surfaceWallGradient, GradientOut);
    
    ////Transition
    //ifStatement = And(ifSlope, And(IFLessOrEqual(worldPosition.y, -3), IFGreaterOrEqual(worldPosition.y, -3 - 3)));
    //gradientSampleOut = lerp(gradientSampleOut, InverseLerp(-3, -3 - 3, worldPosition.y), ifStatement);
    //GradientOut = Compare(ifStatement, NewGradient(0, 2, 2, float4(surfaceWallGradient.colors[0].r, surfaceWallGradient.colors[0].g, surfaceWallGradient.colors[0].b, 0), float4(caveWallGradient.colors[0].r, caveWallGradient.colors[0].g, caveWallGradient.colors[0].b, 1), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0)), GradientOut);
    
    ////Cave layer
    //ifStatement = And(ifSlope, IfLess(worldPosition.y, -3 - 3));
    //gradientSampleOut = lerp(gradientSampleOut, InverseLerp(-3 - 3, -60, worldPosition.y), ifStatement);
    //GradientOut = Compare(ifStatement, caveWallGradient, GradientOut);
    
    ////Ground layer
    //if (slope > 0)
    //{
    //    //Surface layer
    //    if (worldPosition.y > -3)
    //    {
    //        gradientSampleOut = InverseLerp(-3, 18, worldPosition.y);
    //        GradientOut = surfaceGroundGradient;
    //    }

    //    //Transition
    //    if (worldPosition.y <= -3 && worldPosition.y >= -3 - 3)
    //    {
    //        gradientSampleOut = InverseLerp(-3, -3 - 3, worldPosition.y);
    //        GradientOut = NewGradient(0, 2, 2, float4(surfaceGroundGradient.colors[0].r, surfaceGroundGradient.colors[0].g, surfaceGroundGradient.colors[0].b, 0), float4(caveGroundGradient.colors[0].r, caveGroundGradient.colors[0].g, caveGroundGradient.colors[0].b, 1), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    //    }

    //    //Cave layer
    //    if (worldPosition.y < -3 - 3)
    //    {
    //        gradientSampleOut = InverseLerp(-3 - 3, -60, worldPosition.y);
    //        GradientOut = caveGroundGradient;
    //    }
    //}
    ////Wall layer
    //else
    //{
    //    //Surface layer
    //    if (worldPosition.y > -3)
    //    {
    //        gradientSampleOut = InverseLerp(-3, 18, worldPosition.y);
    //        GradientOut = surfaceWallGradient;
    //    }

    //    //Transition
    //    if (worldPosition.y <= -3 && worldPosition.y >= -3 - 3)
    //    {
    //        gradientSampleOut = InverseLerp(-3, -3 - 3, worldPosition.y);
    //        GradientOut = NewGradient(0, 2, 2, float4(surfaceWallGradient.colors[0].r, surfaceWallGradient.colors[0].g, surfaceWallGradient.colors[0].b, 0), float4(caveWallGradient.colors[0].r, caveWallGradient.colors[0].g, caveWallGradient.colors[0].b, 1), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    //    }

    //    //Cave layer
    //    if (worldPosition.y < -3 - 3)
    //    {
    //        gradientSampleOut = InverseLerp(-3 - 3, -60, worldPosition.y);
    //        GradientOut = caveWallGradient;
    //    }
    //}
    
    
    
    
    
    
    
    
    //gradientSampleOut = 0;
    //GradientOut = surfaceGroundGradient;
    
    ///*Ground layer*/
    //float ifSlope = IfGreater(slope, 0);
    
    ////Surface layer
    //ifStatement = And(ifSlope, IfGreater(worldPosition.y, surfaceOffset));
    //gradientSampleOut = lerp(gradientSampleOut, InverseLerp(surfaceOffset, RegionFragGPUs[regionIndex0].SurfaceAdditiveHeightLimit, worldPosition.y), ifStatement);
    //GradientOut = Compare(ifStatement, surfaceGroundGradient, GradientOut);
    
    ////Transition
    //ifStatement = And(ifSlope, And(IFLessOrEqual(worldPosition.y, surfaceOffset), IFGreaterOrEqual(worldPosition.y, surfaceOffset - 3)));
    //gradientSampleOut = lerp(gradientSampleOut, InverseLerp(surfaceOffset, surfaceOffset - 3, worldPosition.y), ifStatement);
    //GradientOut = Compare(ifStatement, NewGradient(0, 2, 2, float4(surfaceGroundGradient.colors[0].r, surfaceGroundGradient.colors[0].g, surfaceGroundGradient.colors[0].b, 0), float4(caveGroundGradient.colors[0].r, caveGroundGradient.colors[0].g, caveGroundGradient.colors[0].b, 1), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0)), GradientOut);
    
    ////Cave layer
    //ifStatement = And(ifSlope, IfLess(worldPosition.y, surfaceOffset - 3));
    //gradientSampleOut = lerp(gradientSampleOut, InverseLerp(surfaceOffset - 3, -60, worldPosition.y), ifStatement);
    //GradientOut = Compare(ifStatement, caveGroundGradient, GradientOut);
    
    ///*Wall layer*/
    //ifSlope = IFLessOrEqual(slope, 0);
    
    ////Surface layer
    //ifStatement = And(ifSlope, IfGreater(worldPosition.y, surfaceOffset));
    //gradientSampleOut = lerp(gradientSampleOut, InverseLerp(surfaceOffset, RegionFragGPUs[regionIndex0].SurfaceAdditiveHeightLimit, worldPosition.y), ifStatement);
    //GradientOut = Compare(ifStatement, surfaceWallGradient, GradientOut);
    
    ////Transition
    //ifStatement = And(ifSlope, And(IFLessOrEqual(worldPosition.y, surfaceOffset), IFGreaterOrEqual(worldPosition.y, surfaceOffset - 3)));
    //gradientSampleOut = lerp(gradientSampleOut, InverseLerp(surfaceOffset, surfaceOffset - 3, worldPosition.y), ifStatement);
    //GradientOut = Compare(ifStatement, NewGradient(0, 2, 2, float4(surfaceWallGradient.colors[0].r, surfaceWallGradient.colors[0].g, surfaceWallGradient.colors[0].b, 0), float4(caveWallGradient.colors[0].r, caveWallGradient.colors[0].g, caveWallGradient.colors[0].b, 1), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0)), GradientOut);
    
    //Cave layer
    //ifStatement = And(ifSlope, IfLess(worldPosition.y, surfaceOffset - 3));
    //gradientSampleOut = lerp(gradientSampleOut, InverseLerp(surfaceOffset - 3, -60, worldPosition.y), ifStatement);
    //GradientOut = Compare(ifStatement, caveWallGradient, GradientOut);
    
    //Ground layer
    if (slope > 0)
    {
        //Surface layer
        if (worldPosition.y > surfaceOffset)
        {
            gradientSampleOut = InverseLerp(surfaceOffset, surfaceAdditiveHeightLimit, worldPosition.y);
            GradientOut = surfaceGroundGradient;
        }

        //Transition
        if (worldPosition.y <= surfaceOffset && worldPosition.y >= surfaceOffset - 3)
        {
            gradientSampleOut = InverseLerp(surfaceOffset, surfaceOffset - 3, worldPosition.y);
            GradientOut = NewGradient(0, 2, 2, float4(surfaceGroundGradient.colors[0].r, surfaceGroundGradient.colors[0].g, surfaceGroundGradient.colors[0].b, 0), float4(caveGroundGradient.colors[0].r, caveGroundGradient.colors[0].g, caveGroundGradient.colors[0].b, 1), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
        }

        //Cave layer
        if (worldPosition.y < surfaceOffset - 3)
        {
            gradientSampleOut = InverseLerp(surfaceOffset - 3, -60, worldPosition.y);
            GradientOut = caveGroundGradient;
        }
    }
    //Wall layer
    else
    {
        //Surface layer
        if (worldPosition.y > surfaceOffset)
        {
            gradientSampleOut = InverseLerp(surfaceOffset, surfaceAdditiveHeightLimit, worldPosition.y);
            GradientOut = surfaceWallGradient;
        }

        //Transition
        if (worldPosition.y <= surfaceOffset && worldPosition.y >= surfaceOffset - 3)
        {
            gradientSampleOut = InverseLerp(surfaceOffset, surfaceOffset - 3, worldPosition.y);
            GradientOut = NewGradient(0, 2, 2, float4(surfaceWallGradient.colors[0].r, surfaceWallGradient.colors[0].g, surfaceWallGradient.colors[0].b, 0), float4(caveWallGradient.colors[0].r, caveWallGradient.colors[0].g, caveWallGradient.colors[0].b, 1), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
        }

        //Cave layer
        if (worldPosition.y < surfaceOffset - 3)
        {
            gradientSampleOut = InverseLerp(surfaceOffset - 3, -60, worldPosition.y);
            GradientOut = caveWallGradient;
        }
    }
}