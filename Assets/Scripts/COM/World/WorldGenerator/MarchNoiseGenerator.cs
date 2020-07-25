using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using COM.Database.World;
using COM.Utils;

namespace COM.World
{
    /// <summary>
    /// GPGPU density generator for the marching cubes algorithm using 3d perlin
    /// </summary>
    [System.Serializable]
    public class MarchNoiseGenerator
    {
        public ComputeShader NoiseShader;

        [Header("General Settings")]
        public Vector3 MapOffset = Vector3.zero;

        [Header("Surface Settings")]
        public int SurfaceOctaves = 4;
        public float SurfaceLacunarity = 1.87f;
        public float SurfaceScale = 30;

        [Header("Cave Settings")]
        public int CaveOctaves = 4;
        public float CaveLacunarity = 1.87f;
        public float CaveScale = 30;

        /*Cache*/
        private int Kernel = -1;

        /*Compute buffers*/
        private ComputeBuffer SurfaceOctaveOffsetsBuffer;
        private ComputeBuffer CaveOctaveOffsetsBuffer;
        private ComputeBuffer GPUSurfaceBiomesBuffer;
        private ComputeBuffer GPUCaveBiomesBuffer;
        private ComputeBuffer GPURegionsBuffer;

        #region Setup
        public void Init(string mapSeed, int cubesPerAxis, float chunkSize, SurfaceBiomeDatabase.SurfaceBiomeList surfaceBiomes, CaveBiomeDatabase.CaveBiomeList caveBiomes, List<Region> regions)
        {
            //GPU
            Vector3[] SurfaceOctaveOffsets = GenerateMapOctaveOffsets(mapSeed, false);
            Vector3[] CaveOctaveOffsets = GenerateMapOctaveOffsets(mapSeed, true);
            GPUSurfaceBiome[] GPUSurfaceBiomes = GPUSurfaceBiome.CreateGPUSurfaceBiomes(surfaceBiomes);
            GPUCaveBiome[] GPUCaveBiomes = GPUCaveBiome.CreateGPUCaveBiomes(caveBiomes);
            GPURegion[] GPURegions = GPURegion.CreateGPURegions(regions);

            //Kernal
            Kernel = NoiseShader.FindKernel("NoiseGenerator");

            //Sets
            NoiseShader.SetInts("ThreadDimensions", new int[3] { cubesPerAxis, cubesPerAxis, cubesPerAxis });
            NoiseShader.SetFloat("ChunkSize", chunkSize);
            NoiseShader.SetFloat("CubesPerAxis", cubesPerAxis);
            NoiseShader.SetInt("SurfaceOctaves", SurfaceOctaves);
            NoiseShader.SetFloat("SurfaceLacunarity", SurfaceLacunarity);
            NoiseShader.SetFloat("SurfaceScale", SurfaceScale);
            NoiseShader.SetInt("CaveOctaves", CaveOctaves);
            NoiseShader.SetFloat("CaveLacunarity", CaveLacunarity);
            NoiseShader.SetFloat("CaveScale", CaveScale);
            NoiseShader.SetInt("SurfaceBiomesCount", GPUSurfaceBiomes.Length);
            NoiseShader.SetInt("CaveBiomesCount", GPUCaveBiomes.Length);
            NoiseShader.SetInt("RegionsCount", GPURegions.Length);

            //StructuredBuffer sets
            SurfaceOctaveOffsetsBuffer = new ComputeBuffer(SurfaceOctaves, Stride.Vector3Stride);
            CaveOctaveOffsetsBuffer = new ComputeBuffer(CaveOctaves, Stride.Vector3Stride);
            GPUSurfaceBiomesBuffer = new ComputeBuffer(GPUSurfaceBiomes.Length, GPUSurfaceBiome.Stride);
            GPUCaveBiomesBuffer = new ComputeBuffer(GPUCaveBiomes.Length, GPUCaveBiome.Stride);
            GPURegionsBuffer = new ComputeBuffer(GPURegions.Length, GPURegion.Stride);
            SurfaceOctaveOffsetsBuffer.SetData(SurfaceOctaveOffsets);
            CaveOctaveOffsetsBuffer.SetData(CaveOctaveOffsets);
            GPUSurfaceBiomesBuffer.SetData(GPUSurfaceBiomes);
            GPUCaveBiomesBuffer.SetData(GPUCaveBiomes);
            GPURegionsBuffer.SetData(GPURegions);
            NoiseShader.SetBuffer(Kernel, "SurfaceOctaveOffsets", SurfaceOctaveOffsetsBuffer);
            NoiseShader.SetBuffer(Kernel, "CaveOctaveOffsets", CaveOctaveOffsetsBuffer);
            NoiseShader.SetBuffer(Kernel, "SurfaceBiomes", GPUSurfaceBiomesBuffer);
            NoiseShader.SetBuffer(Kernel, "CaveBiomes", GPUCaveBiomesBuffer);
            NoiseShader.SetBuffer(Kernel, "Regions", GPURegionsBuffer);
        }
        #endregion

        #region Output
        public (ComputeBuffer, FragRegionIndex) Result(int chunkCoordX, int chunkCoordY, int chunkCoordZ, int cubesPerAxis)
        {
            //Total number of cubes inside a chunk
            int cubesPerChunk = cubesPerAxis * cubesPerAxis * cubesPerAxis;
            int indexesPerChunk = cubesPerAxis * cubesPerAxis;

            //How many to process on a single thread
            int processPerThread = Mathf.CeilToInt(cubesPerAxis / 8.0f);

            //RWStructuredBuffer sets
            ComputeBuffer noisePointsBuffer = new ComputeBuffer(cubesPerChunk, Stride.Vector4Stride);
            ComputeBuffer fragRegionIndexsBuffer = new ComputeBuffer(indexesPerChunk, Stride.IntStride);
            NoiseShader.SetBuffer(Kernel, "NoisePoints", noisePointsBuffer);
            NoiseShader.SetBuffer(Kernel, "RegionIndexes", fragRegionIndexsBuffer);

            //Sets
            NoiseShader.SetInts("ChunkCoord", new int[3] { chunkCoordX, chunkCoordY, chunkCoordZ });

            //Run kernal
            NoiseShader.Dispatch(Kernel, processPerThread, processPerThread, processPerThread);

            //Output
            int[] fragRegionIndexOutput = new int[indexesPerChunk];
            fragRegionIndexsBuffer.GetData(fragRegionIndexOutput);

            //Process indexes for fragment shader
            FragRegionIndex fragRegionIndex = new FragRegionIndex
            {
                Index0 = -1,
                Index1 = -1,
                Index2 = -1,
                Index3 = -1,
            };
            for (int i = 0; i < fragRegionIndexOutput.Length; i++)
            {
                if (fragRegionIndex.Index0 == fragRegionIndexOutput[i]) continue;
                if (fragRegionIndex.Index0 == -1)
                {
                    fragRegionIndex.Index0 = fragRegionIndexOutput[i];
                    continue;
                }

                if (fragRegionIndex.Index1 == fragRegionIndexOutput[i]) continue;
                if (fragRegionIndex.Index1 == -1)
                {
                    fragRegionIndex.Index1 = fragRegionIndexOutput[i];
                    continue;
                }

                if (fragRegionIndex.Index2 == fragRegionIndexOutput[i]) continue;
                if (fragRegionIndex.Index2 == -1)
                {
                    fragRegionIndex.Index2 = fragRegionIndexOutput[i];
                    continue;
                }

                if (fragRegionIndex.Index3 == fragRegionIndexOutput[i]) continue;
                if (fragRegionIndex.Index3 == -1)
                {
                    fragRegionIndex.Index3 = fragRegionIndexOutput[i];
                    break;
                }
            }

            //Get rid of buffer data
            fragRegionIndexsBuffer.Dispose();

            return (noisePointsBuffer, fragRegionIndex);
        }
        #endregion

        #region Utils
        public Vector3[] GenerateMapOctaveOffsets(string mapSeed, bool isSubtractive)
        {
            Random.InitState(isSubtractive ? -mapSeed.GetHashCode() : mapSeed.GetHashCode());
            Vector3[] mapOctaveOffsets = new Vector3[isSubtractive ? CaveOctaves : SurfaceOctaves];
            for (int i = 0; i < (isSubtractive ? CaveOctaves : SurfaceOctaves); i++)
            {
                float offsetX = Random.Range(-100000.0f, 100000.0f) + MapOffset.x;
                float offsetY = Random.Range(-100000.0f, 100000.0f) + MapOffset.y;
                float offsetZ = Random.Range(-100000.0f, 100000.0f) + MapOffset.z;
                mapOctaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);
            }
            return mapOctaveOffsets;
        }

        public void Dispose()
        {
            SurfaceOctaveOffsetsBuffer.Dispose();
            CaveOctaveOffsetsBuffer.Dispose();
            GPUSurfaceBiomesBuffer.Dispose();
            GPUCaveBiomesBuffer.Dispose();
            GPURegionsBuffer.Dispose();
        }
        #endregion
    }
}
