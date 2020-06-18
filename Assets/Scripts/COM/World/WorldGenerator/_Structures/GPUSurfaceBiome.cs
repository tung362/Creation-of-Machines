using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using COM.Database.World;

namespace COM.World
{
    /// <summary>
    /// GPU version of SurfaceBiomeDatabase.SurfaceBiome
    /// </summary>
    [System.Serializable]
    public struct GPUSurfaceBiome
    {
        public const int Stride = 24;

        //General
        public float Persistance;
        //Surface layer
        public float Height;
        public float Floor;
        //Additive layer
        public float AdditiveHeight;
        public float AdditiveHeightLimit;
        public float AdditiveOffset;
        //Subtractive

        public GPUSurfaceBiome(SurfaceBiomeDatabase.SurfaceBiome surfaceBiome)
        {
            Persistance = surfaceBiome.Persistance;
            Height = surfaceBiome.Height;
            Floor = surfaceBiome.Floor;
            AdditiveHeight = surfaceBiome.AdditiveHeight;
            AdditiveHeightLimit = surfaceBiome.AdditiveHeightLimit;
            AdditiveOffset = surfaceBiome.AdditiveOffset;
        }

        public static GPUSurfaceBiome[] CreateGPUSurfaceBiomes(SurfaceBiomeDatabase.SurfaceBiomeList surfaceBiomes)
        {
            GPUSurfaceBiome[] gpuSurfaceBiomes = new GPUSurfaceBiome[surfaceBiomes.Count];
            for (int i = 0; i < surfaceBiomes.Count; i++) gpuSurfaceBiomes[i] = new GPUSurfaceBiome(surfaceBiomes[i]);
            return gpuSurfaceBiomes;
        }
    }
}