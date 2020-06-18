#ifndef BiomeSample_
#define BiomeSample_

#include "Includes/Logic.hlsl"
#include "Includes/Gradient.hlsl"
#include "Includes/Terrain.hlsl"

float BlendDistance(float2 coord, float2 closest, float2 closestK)
{
    float closestDistance = distance(closest, coord);
    float closestKDistance = distance(closestK, coord);
    return 1 - (closestDistance / closestKDistance);
}

void SeperateBiomeLayers_float(Texture2D surfaceRegionsTexture, Texture2D caveRegionsTexture, Texture2D regionMapTexture, float slope, float3 worldPosition, float noise, int regionIndex0, int regionIndex1, int regionIndex2, out float gradientSampleOut, out Gradient GradientOut)
{
    //Load data texture
    FragRegionMap test0 = LoadRegionMap(regionMapTexture, regionIndex0);
    FragRegionMap test1 = LoadRegionMap(regionMapTexture, regionIndex1);
    FragRegionMap test2 = LoadRegionMap(regionMapTexture, regionIndex2);
    
    FragSurfaceRegion surfaceRegion0 = LoadSurfaceRegion(surfaceRegionsTexture, test0.SurfaceBiomeID);
    FragSurfaceRegion surfaceRegion1 = LoadSurfaceRegion(surfaceRegionsTexture, test1.SurfaceBiomeID);
    FragSurfaceRegion surfaceRegion2 = LoadSurfaceRegion(surfaceRegionsTexture, test2.SurfaceBiomeID);
    
    FragCaveRegion caveRegion0 = LoadCaveRegion(caveRegionsTexture, test0.CaveBiomeID);
    FragCaveRegion caveRegion1 = LoadCaveRegion(caveRegionsTexture, test1.CaveBiomeID);
    FragCaveRegion caveRegion2 = LoadCaveRegion(caveRegionsTexture, test2.CaveBiomeID);
    
    //Defaults
    float surfaceOffset0 = surfaceRegion0.Floor - surfaceRegion0.Height;
    float surfaceOffset1 = surfaceRegion1.Floor - surfaceRegion1.Height;
    float surfaceOffset2 = surfaceRegion2.Floor - surfaceRegion2.Height;
    
    FragGradient SurfaceGroundGradient = surfaceRegion0.GroundGradient;
    FragGradient SurfaceWallGradient = surfaceRegion0.WallGradient;
    FragGradient CaveGroundGradient = caveRegion0.GroundGradient;
    FragGradient CaveWallGradient = caveRegion0.WallGradient;
    
    float surfaceHeight = surfaceRegion0.Height;
    float surfaceFloor = surfaceRegion0.Floor;
    float surfaceAdditiveHeightLimit = surfaceRegion0.AdditiveHeightLimit;
    float surfaceOffset = surfaceOffset0;
    
    //Surface and cave
    if (noise <= 0.3f)
    {
        /*Biome blend first pass*/
        float sample = InverseLerp(0, 0.3f, noise);
        
        SurfaceGroundGradient = BlendGradient(surfaceRegion1.GroundGradient, surfaceRegion0.GroundGradient, sample);
        SurfaceWallGradient = BlendGradient(surfaceRegion1.WallGradient, surfaceRegion0.WallGradient, sample);
        CaveGroundGradient = BlendGradient(caveRegion1.GroundGradient, caveRegion0.GroundGradient, sample);
        CaveWallGradient = BlendGradient(caveRegion1.WallGradient, caveRegion0.WallGradient, sample);
        
        float surfaceHeightMid = (surfaceRegion1.Height + surfaceRegion0.Height) * 0.5f;
        float surfaceFloorMid = (surfaceRegion1.Floor + surfaceRegion0.Floor) * 0.5f;
        float surfaceAdditiveHeightLimitMid = (surfaceRegion1.AdditiveHeightLimit + surfaceRegion0.AdditiveHeightLimit) * 0.5f;
        float surfaceOffsetMid = (surfaceOffset1 + surfaceOffset0) * 0.5f;
    
        float surfaceHeight = lerp(surfaceHeightMid, surfaceRegion0.Height, sample);
        float surfaceFloor = lerp(surfaceFloorMid, surfaceRegion0.Floor, sample);
        float surfaceAdditiveHeightLimit = lerp(surfaceAdditiveHeightLimitMid, surfaceRegion0.AdditiveHeightLimit, sample);
        float surfaceOffset = lerp(surfaceOffsetMid, surfaceOffset0, sample);
    
        /*Biome blend second pass*/
        float secondaryBlendNoise = BlendDistance(worldPosition.xz, test0.Coord, test2.Coord);
        sample = InverseLerp(0, 0.3f, secondaryBlendNoise);
        
        //Todo: might remove if statement outside of the nest
        //Surface
        if (secondaryBlendNoise <= 0.3f && test0.SurfaceBiomeID == test1.SurfaceBiomeID && test0.SurfaceBiomeID != test2.SurfaceBiomeID)
        {
            
            SurfaceGroundGradient = BlendGradient(surfaceRegion2.GroundGradient, surfaceRegion0.GroundGradient, sample);
            SurfaceWallGradient = BlendGradient(surfaceRegion2.WallGradient, surfaceRegion0.WallGradient, sample);
            surfaceHeightMid = (surfaceRegion2.Height + surfaceRegion0.Height) * 0.5f;
            surfaceFloorMid = (surfaceRegion2.Floor + surfaceRegion0.Floor) * 0.5f;
            surfaceAdditiveHeightLimitMid = (surfaceRegion2.AdditiveHeightLimit + surfaceRegion0.AdditiveHeightLimit) * 0.5f;
            surfaceOffsetMid = (surfaceOffset2 + surfaceOffset0) * 0.5f;
    
            surfaceHeight = lerp(surfaceHeightMid, surfaceRegion0.Height, sample);
            surfaceFloor = lerp(surfaceFloorMid, surfaceRegion0.Floor, sample);
            surfaceAdditiveHeightLimit = lerp(surfaceAdditiveHeightLimitMid, surfaceRegion0.AdditiveHeightLimit, sample);
            surfaceOffset = lerp(surfaceOffsetMid, surfaceOffset0, sample);
        }
        
        //Cave
        if (secondaryBlendNoise <= 0.3f && test0.CaveBiomeID == test1.CaveBiomeID && test0.CaveBiomeID != test2.CaveBiomeID)
        {
            CaveGroundGradient = BlendGradient(caveRegion2.GroundGradient, caveRegion0.GroundGradient, sample);
            CaveWallGradient = BlendGradient(caveRegion2.WallGradient, caveRegion0.WallGradient, sample);
        }
    }
    
    Gradient surfaceGroundGradient = NewGradient(0, SurfaceGroundGradient.ColorLength, 2, SurfaceGroundGradient.C0, SurfaceGroundGradient.C1, SurfaceGroundGradient.C2, SurfaceGroundGradient.C3, SurfaceGroundGradient.C4, SurfaceGroundGradient.C5, SurfaceGroundGradient.C6, SurfaceGroundGradient.C7, float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    Gradient surfaceWallGradient = NewGradient(0, SurfaceWallGradient.ColorLength, 2, SurfaceWallGradient.C0, SurfaceWallGradient.C1, SurfaceWallGradient.C2, SurfaceWallGradient.C3, SurfaceWallGradient.C4, SurfaceWallGradient.C5, SurfaceWallGradient.C6, SurfaceWallGradient.C7, float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    Gradient caveGroundGradient = NewGradient(0, CaveGroundGradient.ColorLength, 2, CaveGroundGradient.C0, CaveGroundGradient.C1, CaveGroundGradient.C2, CaveGroundGradient.C3, CaveGroundGradient.C4, CaveGroundGradient.C5, CaveGroundGradient.C6, CaveGroundGradient.C7, float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    Gradient caveWallGradient = NewGradient(0, CaveWallGradient.ColorLength, 2, CaveWallGradient.C0, CaveWallGradient.C1, CaveWallGradient.C2, CaveWallGradient.C3, CaveWallGradient.C4, CaveWallGradient.C5, CaveWallGradient.C6, CaveWallGradient.C7, float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    
    //Surface and cave
    //float ifStatement = IFLessOrEqual(noise, 0.3f);
    //GradientFragGPU SurfaceGroundGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex1].SurfaceGroundGradient, RegionFragGPUs[regionIndex0].SurfaceGroundGradient, sample), RegionFragGPUs[regionIndex0].SurfaceGroundGradient);
    //GradientFragGPU SurfaceWallGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex1].SurfaceWallGradient, RegionFragGPUs[regionIndex0].SurfaceWallGradient, sample), RegionFragGPUs[regionIndex0].SurfaceWallGradient);
    //GradientFragGPU CaveGroundGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex1].CaveGroundGradient, RegionFragGPUs[regionIndex0].CaveGroundGradient, sample), RegionFragGPUs[regionIndex0].CaveGroundGradient);
    //GradientFragGPU CaveWallGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex1].CaveWallGradient, RegionFragGPUs[regionIndex0].CaveWallGradient, sample), RegionFragGPUs[regionIndex0].CaveWallGradient);
    //float surfaceHeightMid = (test1.SurfaceHeight + test0.SurfaceHeight) * 0.5f;
    //float surfaceFloorMid = (test1.SurfaceFloor + test0.SurfaceFloor) * 0.5f;
    //float surfaceAdditiveHeightLimitMid = (test1.SurfaceAdditiveHeightLimit + test0.SurfaceAdditiveHeightLimit) * 0.5f;
    //float surfaceOffsetMid = (surfaceOffset1 + surfaceOffset0) * 0.5f;
    
    //float surfaceHeight = lerp(test0.SurfaceHeight, lerp(surfaceHeightMid, test0.SurfaceHeight, sample), ifStatement);
    //float surfaceFloor = lerp(test0.SurfaceFloor, lerp(surfaceFloorMid, test0.SurfaceFloor, sample), ifStatement);
    //float surfaceAdditiveHeightLimit = lerp(test0.SurfaceAdditiveHeightLimit, lerp(surfaceAdditiveHeightLimitMid, test0.SurfaceAdditiveHeightLimit, sample), ifStatement);
    //float surfaceOffset = lerp(surfaceOffset0, lerp(surfaceOffsetMid, surfaceOffset0, sample), ifStatement);
    
    ///*Biome blend second pass*/
    //float secondaryBlendNoise = BlendDistance(worldPosition.xz, RegionFragGPUs[regionIndex0].Coord, RegionFragGPUs[regionIndex2].Coord);
    //sample = InverseLerp(0, 0.3f, secondaryBlendNoise);
    
    //Surface
    //ifStatement = And(IFLessOrEqual(secondaryBlendNoise, 0.3f), And(IfEqual(test0.SurfaceBiomeID, test1.SurfaceBiomeID), IfNotEqual(test0.SurfaceBiomeID, test2.SurfaceBiomeID)));
    ////ifStatement = And(IFLessOrEqual(secondaryBlendNoise, 0.3f), And(IfEqual(RegionFragGPUs[regionIndex0].SurfaceBiomeID, RegionFragGPUs[regionIndex1].SurfaceBiomeID), IfNotEqual(RegionFragGPUs[regionIndex0].SurfaceBiomeID, RegionFragGPUs[regionIndex2].SurfaceBiomeID)));
    ////testSampleOut = 0;
    ////if(test2.SurfaceBiomeID == 0) testSampleOut = 0.5f;
    ////if(test2.SurfaceBiomeID == 1) testSampleOut = 1.0f;
    ////if(RegionFragGPUs[regionIndex2].SurfaceBiomeID == 0) testSampleOut = 0.5f; //Red
    ////if(RegionFragGPUs[regionIndex2].SurfaceBiomeID == 1) testSampleOut = 1.0f; //Blue
    
    ////if (ifStatement == 1 && secondaryBlendNoise <= 0.3f && test0.SurfaceBiomeID == test1.SurfaceBiomeID && test0.SurfaceBiomeID != test2.SurfaceBiomeID) testSampleOut = 0.5f;
    ////if (ifStatement == 1 && secondaryBlendNoise <= 0.3f && RegionFragGPUs[regionIndex0].SurfaceBiomeID == RegionFragGPUs[regionIndex1].SurfaceBiomeID && RegionFragGPUs[regionIndex0].SurfaceBiomeID != RegionFragGPUs[regionIndex2].SurfaceBiomeID) testSampleOut = 0.5f;
    
    ////if (ifStatement == 1) testSampleOut = 0.5f;
    
    //SurfaceGroundGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex2].SurfaceGroundGradient, RegionFragGPUs[regionIndex0].SurfaceGroundGradient, sample), SurfaceGroundGradient);
    //SurfaceWallGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex2].SurfaceWallGradient, RegionFragGPUs[regionIndex0].SurfaceWallGradient, sample), SurfaceWallGradient);
    //surfaceHeightMid = (test2.SurfaceHeight + test0.SurfaceHeight) * 0.5f;
    //surfaceFloorMid = (test2.SurfaceFloor + test0.SurfaceFloor) * 0.5f;
    //surfaceAdditiveHeightLimitMid = (test2.SurfaceAdditiveHeightLimit + test0.SurfaceAdditiveHeightLimit) * 0.5f;
    //surfaceOffsetMid = (surfaceOffset2 + surfaceOffset0) * 0.5f;
    
    //surfaceHeight = lerp(surfaceHeight, lerp(surfaceHeightMid, test0.SurfaceHeight, sample), ifStatement);
    //surfaceFloor = lerp(surfaceFloor, lerp(surfaceFloorMid, test0.SurfaceFloor, sample), ifStatement);
    //surfaceAdditiveHeightLimit = lerp(surfaceAdditiveHeightLimit, lerp(surfaceAdditiveHeightLimitMid, test0.SurfaceAdditiveHeightLimit, sample), ifStatement);
    //surfaceOffset = lerp(surfaceOffset, lerp(surfaceOffsetMid, surfaceOffset0, sample), ifStatement);
    
    ////Cave
    //ifStatement = And(IFLessOrEqual(secondaryBlendNoise, 0.3f), And(IfEqual(RegionFragGPUs[regionIndex0].CaveBiomeID, RegionFragGPUs[regionIndex1].CaveBiomeID), IfNotEqual(RegionFragGPUs[regionIndex0].CaveBiomeID, RegionFragGPUs[regionIndex2].CaveBiomeID)));
    //CaveGroundGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex2].CaveGroundGradient, RegionFragGPUs[regionIndex0].CaveGroundGradient, sample), CaveGroundGradient);
    //CaveWallGradient = Compare(ifStatement, BlendGradientFrag(RegionFragGPUs[regionIndex2].CaveWallGradient, RegionFragGPUs[regionIndex0].CaveWallGradient, sample), CaveWallGradient);
    
    //Gradient surfaceGroundGradient = NewGradient(0, SurfaceGroundGradient.ColorLength, 2, SurfaceGroundGradient.C0, SurfaceGroundGradient.C1, SurfaceGroundGradient.C2, SurfaceGroundGradient.C3, SurfaceGroundGradient.C4, SurfaceGroundGradient.C5, SurfaceGroundGradient.C6, SurfaceGroundGradient.C7, float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    //Gradient surfaceWallGradient = NewGradient(0, SurfaceWallGradient.ColorLength, 2, SurfaceWallGradient.C0, SurfaceWallGradient.C1, SurfaceWallGradient.C2, SurfaceWallGradient.C3, SurfaceWallGradient.C4, SurfaceWallGradient.C5, SurfaceWallGradient.C6, SurfaceWallGradient.C7, float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    //Gradient caveGroundGradient = NewGradient(0, CaveGroundGradient.ColorLength, 2, CaveGroundGradient.C0, CaveGroundGradient.C1, CaveGroundGradient.C2, CaveGroundGradient.C3, CaveGroundGradient.C4, CaveGroundGradient.C5, CaveGroundGradient.C6, CaveGroundGradient.C7, float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    //Gradient caveWallGradient = NewGradient(0, CaveWallGradient.ColorLength, 2, CaveWallGradient.C0, CaveWallGradient.C1, CaveWallGradient.C2, CaveWallGradient.C3, CaveWallGradient.C4, CaveWallGradient.C5, CaveWallGradient.C6, CaveWallGradient.C7, float2(1, 0), float2(1, 1), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0));
    
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
#endif