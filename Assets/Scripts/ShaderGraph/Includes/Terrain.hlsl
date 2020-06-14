#ifndef Terrain_
#define Terrain_

#include "TextureDecoder.hlsl"
#include "Gradient.hlsl"

/*Consts*/
static const int SurfaceRegionSize = 21;
static const int CaveRegionSize = 18;
static const int RegionMapSize = 11;

/*Structs*/
struct SurfaceRegionFragGPU
{
    float Height;
    float Floor;
    float AdditiveHeightLimit;
    GradientFragGPU GroundGradient;
    GradientFragGPU WallGradient;
};

struct CaveRegionFragGPU
{
    GradientFragGPU GroundGradient;
    GradientFragGPU WallGradient;
};

struct RegionMapFragGPU
{
    float2 Coord;
    int SurfaceBiomeID;
    int CaveBiomeID;
    int NeighborIndex0;
    int NeighborIndex1;
    int NeighborIndex2;
    int NeighborIndex3;
    int NeighborIndex4;
    int NeighborIndex5;
    int NeighborIndex6;
    int NeighborIndex7;
};

/*Mapper*/
int GetSurfaceRegionIndex(int regionIndex)
{
    return regionIndex * SurfaceRegionSize;
}

int GetCaveRegionIndex(int regionIndex)
{
    return regionIndex * CaveRegionSize;
}

int GetRegionMapIndex(int regionIndex)
{
    return regionIndex * RegionMapSize;
}

/*Load from texture*/
//Surface Region
SurfaceRegionFragGPU LoadSurfaceRegion(Texture2D dataTexture, int index, out int nextIndex)
{
    nextIndex = index;
    SurfaceRegionFragGPU retVal;
    retVal.Height = DecodeFloat(dataTexture, nextIndex, nextIndex);
    retVal.Floor = DecodeFloat(dataTexture, nextIndex, nextIndex);
    retVal.AdditiveHeightLimit = DecodeFloat(dataTexture, nextIndex, nextIndex);
    retVal.GroundGradient = LoadGradient(dataTexture, nextIndex, nextIndex);
    retVal.WallGradient = LoadGradient(dataTexture, nextIndex, nextIndex);
    return retVal;
}

SurfaceRegionFragGPU LoadSurfaceRegion(Texture2D dataTexture, int regionIndex)
{
    int index = GetSurfaceRegionIndex(regionIndex);
    return LoadSurfaceRegion(dataTexture, index, index);
}

//Cave Region
CaveRegionFragGPU LoadCaveRegion(Texture2D dataTexture, int index, out int nextIndex)
{
    nextIndex = index;
    CaveRegionFragGPU retVal;
    retVal.GroundGradient = LoadGradient(dataTexture, nextIndex, nextIndex);
    retVal.WallGradient = LoadGradient(dataTexture, nextIndex, nextIndex);
    return retVal;
}

CaveRegionFragGPU LoadCaveRegion(Texture2D dataTexture, int regionIndex)
{
    int index = GetCaveRegionIndex(regionIndex);
    return LoadCaveRegion(dataTexture, index, index);
}

//Region Map
RegionMapFragGPU LoadRegionMap(Texture2D dataTexture, int index, out int nextIndex)
{
    nextIndex = index;
    RegionMapFragGPU retVal;
    retVal.Coord = DecodeFloat2(dataTexture, nextIndex, nextIndex);
    retVal.SurfaceBiomeID = DecodeFloat(dataTexture, nextIndex, nextIndex);
    retVal.CaveBiomeID = DecodeFloat(dataTexture, nextIndex, nextIndex);
    retVal.NeighborIndex0 = DecodeFloat(dataTexture, nextIndex, nextIndex);
    retVal.NeighborIndex1 = DecodeFloat(dataTexture, nextIndex, nextIndex);
    retVal.NeighborIndex2 = DecodeFloat(dataTexture, nextIndex, nextIndex);
    retVal.NeighborIndex3 = DecodeFloat(dataTexture, nextIndex, nextIndex);
    retVal.NeighborIndex4 = DecodeFloat(dataTexture, nextIndex, nextIndex);
    retVal.NeighborIndex5 = DecodeFloat(dataTexture, nextIndex, nextIndex);
    retVal.NeighborIndex6 = DecodeFloat(dataTexture, nextIndex, nextIndex);
    retVal.NeighborIndex7 = DecodeFloat(dataTexture, nextIndex, nextIndex);
    return retVal;
}

RegionMapFragGPU LoadRegionMap(Texture2D dataTexture, int regionIndex)
{
    int index = GetRegionMapIndex(regionIndex);
    return LoadRegionMap(dataTexture, index, index);
}
#endif