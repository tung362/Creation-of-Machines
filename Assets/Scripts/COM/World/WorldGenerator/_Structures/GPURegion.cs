using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using COM.Database.World;

namespace COM.World
{
    /// <summary>
    /// GPU version of Region
    /// </summary>
    [System.Serializable]
    public struct GPURegion
    {
        public const int Stride = 16;

        public Vector2 Coord;
        public int SurfaceBiomeID;
        public int CaveBiomeID;

        public GPURegion(Region region)
        {
            Coord = region.RegionSite.Coord;
            SurfaceBiomeID = region.SurfaceBiomeID;
            CaveBiomeID = region.CaveBiomeID;
        }

        public static GPURegion[] CreateGPURegions(List<Region> regions)
        {
            GPURegion[] gpuRegions = new GPURegion[regions.Count];
            for (int i = 0; i < regions.Count; i++) gpuRegions[i] = new GPURegion(regions[i]);
            return gpuRegions;
        }
    }
}