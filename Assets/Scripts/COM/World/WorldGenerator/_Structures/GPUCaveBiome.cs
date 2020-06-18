using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using COM.Database.World;

namespace COM.World
{
    /// <summary>
    /// GPU version of CaveBiomeDatabase.CaveBiome
    /// </summary>
    [System.Serializable]
    public struct GPUCaveBiome
    {
        public const int Stride = 8;

        //General
        public float Persistance;
        //Cave layer
        public float Threshold;
        //Additive
        //Subtractive

        public GPUCaveBiome(CaveBiomeDatabase.CaveBiome caveBiome)
        {
            Persistance = caveBiome.Persistance;
            Threshold = caveBiome.Threshold;
        }

        public static GPUCaveBiome[] CreateGPUCaveBiomes(CaveBiomeDatabase.CaveBiomeList caveBiomes)
        {
            GPUCaveBiome[] gpuCaveBiomes = new GPUCaveBiome[caveBiomes.Count];
            for (int i = 0; i < caveBiomes.Count; i++) gpuCaveBiomes[i] = new GPUCaveBiome(caveBiomes[i]);
            return gpuCaveBiomes;
        }
    }
}