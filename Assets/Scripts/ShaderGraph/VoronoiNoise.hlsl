#ifndef VoronoiNoise_
#define VoronoiNoise_

#include "Includes/Logic.hlsl"
#include "Includes/Terrain.hlsl"

void FindClosest3(Texture2D regionMapTexture, float3 worldPosition, int biomeIndex, float closestDistanceIn, float secondClosestDistanceIn, float thirdClosestDistanceIn, int regionIndex0In, int regionIndex1In, int regionIndex2In, out float closestDistanceOut, out float secondClosestDistanceOut, out float thirdClosestDistanceOut, out int regionIndex0Out, out int regionIndex1Out, out int regionIndex2Out)
{
    RegionMapFragGPU regionMap = LoadRegionMap(regionMapTexture, biomeIndex);
    
    closestDistanceOut = closestDistanceIn;
    secondClosestDistanceOut = secondClosestDistanceIn;
    thirdClosestDistanceOut = thirdClosestDistanceIn;
    regionIndex0Out = regionIndex0In;
    regionIndex1Out = regionIndex1In;
    regionIndex2Out = regionIndex2In;
    
    float dist = distance(regionMap.Coord, worldPosition.xz);
    
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

void VoronoiNoise_float(Texture2D regionMapTexture, float3 worldPosition, int biomeIndex0, int biomeIndex1, int biomeIndex2, int biomeIndex3, out float noiseOut, out int regionIndex0Out, out int regionIndex1Out, out int regionIndex2Out)
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
        FindClosest3(regionMapTexture, worldPosition, biomeIndex1, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
        if (biomeIndex0 != -1) FindClosest3(regionMapTexture, worldPosition, biomeIndex0, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
        if (biomeIndex2 != -1) FindClosest3(regionMapTexture, worldPosition, biomeIndex2, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
        if (biomeIndex3 != -1) FindClosest3(regionMapTexture, worldPosition, biomeIndex3, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
        
        //If haven't found third closest then do neighbor search
        if (regionIndex2Out == -1)
        {
            RegionMapFragGPU regionMap = LoadRegionMap(regionMapTexture, regionIndex0Out);
            if(regionMap.NeighborIndex0 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex0, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex1 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex1, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex2 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex2, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex3 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex3, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex4 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex4, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex5 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex5, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex6 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex6, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex7 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex7, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
        }
    }
    else
    {
        //Neighbor search
        if (biomeIndex0 != -1)
        {
            RegionMapFragGPU regionMap = LoadRegionMap(regionMapTexture, biomeIndex0);
            FindClosest3(regionMapTexture, worldPosition, biomeIndex0, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex0 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex0, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex1 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex1, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex2 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex2, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex3 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex3, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex4 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex4, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex5 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex5, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex6 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex6, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
            if(regionMap.NeighborIndex7 != -1) FindClosest3(regionMapTexture, worldPosition, regionMap.NeighborIndex7, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out, closestDistance, secondClosestDistance, thirdClosestDistance, regionIndex0Out, regionIndex1Out, regionIndex2Out);
        }
    }
    noiseOut = 1 - (closestDistance / secondClosestDistance);
}
#endif