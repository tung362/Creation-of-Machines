using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using COM.Utils;

namespace COM.World
{
    /// <summary>
    /// GPGPU marching cubes mesh generator
    /// </summary>
    [System.Serializable]
    public class MarchGenerator
    {
        public ComputeShader ChunkShader;

        /*Cache*/
        private int Kernel = -1;

        #region Setup
        public void Init(int cubesPerAxis)
        {
            //Kernal
            Kernel = ChunkShader.FindKernel("MarchGenerator");

            //Sets
            ChunkShader.SetInts("ThreadDimensions", new int[3] { cubesPerAxis, cubesPerAxis, cubesPerAxis });
            ChunkShader.SetFloat("CubesPerAxis", cubesPerAxis);
        }
        #endregion

        #region Output
        public (Vector3[], int[], FragRegionIndex) Result(int chunkCoordX, int chunkCoordY, int chunkCoordZ, int cubesPerAxis, MarchNoiseGenerator noiseGenerator)
        {
            //Run noise kernal
            (ComputeBuffer, FragRegionIndex) noiseShader = noiseGenerator.Result(chunkCoordX, chunkCoordY, chunkCoordZ, cubesPerAxis);

            //Max TriangleGPU count inside a chunk, 5 possible triangles inside of TriTable, 3 pairs per triangle
            int maxTriangleGPUCount = (cubesPerAxis * cubesPerAxis * cubesPerAxis) * 5;

            //How many to process on a single thread
            int processPerThread = Mathf.CeilToInt(cubesPerAxis / 8.0f);

            //RWStructuredBuffer sets
            ComputeBuffer trianglesBuffer = new ComputeBuffer(maxTriangleGPUCount, GPUTriangle.Stride, ComputeBufferType.Append);
            ComputeBuffer triCountBuffer = new ComputeBuffer(1, Stride.IntStride, ComputeBufferType.Raw);
            trianglesBuffer.SetCounterValue(0);
            ChunkShader.SetBuffer(Kernel, "Triangles", trianglesBuffer);

            //StructuredBuffer sets
            ChunkShader.SetBuffer(Kernel, "NoisePoints", noiseShader.Item1);

            //Run kernal
            ChunkShader.Dispatch(Kernel, processPerThread, processPerThread, processPerThread);

            //Grab output count
            ComputeBuffer.CopyCount(trianglesBuffer, triCountBuffer, 0);
            int[] triCount = { 0 };
            triCountBuffer.GetData(triCount);

            //Outputs
            GPUTriangle[] triangleGPUOutput = new GPUTriangle[triCount[0]];
            Vector3[] verticesOutput = new Vector3[triangleGPUOutput.Length * 3];
            int[] trianglesOutput = new int[triangleGPUOutput.Length * 3];

            //Grab outputs
            trianglesBuffer.GetData(triangleGPUOutput, 0, 0, triCount[0]);

            //Get rid of buffer data
            trianglesBuffer.Dispose();
            triCountBuffer.Dispose();
            noiseShader.Item1.Dispose();

            //Process triangles
            for (int i = 0; i < triangleGPUOutput.Length; i++)
            {
                verticesOutput[i * 3] = triangleGPUOutput[i].vertexA;
                verticesOutput[(i * 3) + 1] = triangleGPUOutput[i].vertexB;
                verticesOutput[(i * 3) + 2] = triangleGPUOutput[i].vertexC;

                trianglesOutput[i * 3] = i * 3;
                trianglesOutput[(i * 3) + 1] = (i * 3) + 1;
                trianglesOutput[(i * 3) + 2] = (i * 3) + 2;
            }

            return (verticesOutput, trianglesOutput, noiseShader.Item2);
        }
        #endregion

        #region Utils
        public void Dispose()
        {

        }
        #endregion
    }
}
