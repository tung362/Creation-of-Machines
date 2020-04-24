using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using csDelaunay;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;
using System;

namespace COM
{
    /*Editor*/
    public struct WGEditorOutputGPU
    {
        public Vector2Int Coord;
        public float VoronoiHeight;
        public float VoronoiMatchHeight;
    };

    /*General*/
    public enum VoronoiGradientMaskType { None, Circle }

    [System.Serializable]
    public class RegionSurfaceBiome
    {
        //Id
        public string Name;
        public int Id;
        //Color
        public Gradient GroundPalette;
        public Gradient WallPalette;
        //General
        public float Persistance;
        //Surface layer
        public float SurfaceHeight;
        public float SurfaceFloor;
        //Additive layer
        public float SurfaceAdditiveHeight;
        public float SurfaceAdditiveHeightLimit;
        public float SurfaceAdditiveOffset;
        //Subtractive
    }

    [System.Serializable]
    public class RegionCaveBiome
    {
        //Id
        public string Name;
        public int Id;
        //Color
        public Gradient GroundPalette;
        public Gradient WallPalette;
        //General
        public float Persistance;
        //Cave layer
        public float CaveThreshold;
        //Additive
        //Subtractive
    }

    [System.Serializable]
    public class Region
    {
        public string RegionName;
        public int FactionID;
        public Site RegionSite;
        public RegionSurfaceBiome SurfaceBiome;
        public RegionCaveBiome CaveBiome;
    }

    /*Compute GPU*/
    [System.Serializable]
    public struct SurfaceBiomeGPU
    {
        //Id
        public int Id;
        //General
        public float Persistance;
        //Surface layer
        public float SurfaceHeight;
        public float SurfaceFloor;
        //Additive layer
        public float SurfaceAdditiveHeight;
        public float SurfaceAdditiveHeightLimit;
        public float SurfaceAdditiveOffset;
        //Subtractive
    };

    [System.Serializable]
    public struct CaveBiomeGPU
    {
        //Id
        public int Id;
        //General
        public float Persistance;
        //Cave layer
        public float CaveThreshold;
        //Additive
        //Subtractive
    };

    public struct RegionGPU
    {
        public Vector2 Coord;
        public SurfaceBiomeGPU SurfaceBiome;
        public CaveBiomeGPU CaveBiome;
    };

    public struct TriangleGPU
    {
        public Vector3 vertexA;
        public Vector3 vertexB;
        public Vector3 vertexC;
    };

    /*Fragment GPU*/
    public struct RegionIndexFragGPU
    {
        public int Index0;
        public int Index1;
        public int Index2;
        public int Index3;
    };

    public struct GradientFragGPU
    {
        public int ColorLength;
        public Vector4 C0;
        public Vector4 C1;
        public Vector4 C2;
        public Vector4 C3;
        public Vector4 C4;
        public Vector4 C5;
        public Vector4 C6;
        public Vector4 C7;
    };

    public struct RegionFragGPU
    {
        public Vector2 Coord;
        public int SurfaceBiomeID;
        public int CaveBiomeID;
        //public float SurfaceHeight;
        //public float SurfaceFloor;
        //public float SurfaceAdditiveHeightLimit;
        public GradientFragGPU SurfaceGroundGradient;
        public GradientFragGPU SurfaceWallGradient;
        public GradientFragGPU CaveGroundGradient;
        public GradientFragGPU CaveWallGradient;
        public int NeighborIndex0;
        public int NeighborIndex1;
        public int NeighborIndex2;
        public int NeighborIndex3;
        public int NeighborIndex4;
        public int NeighborIndex5;
        public int NeighborIndex6;
        public int NeighborIndex7;
    };
}