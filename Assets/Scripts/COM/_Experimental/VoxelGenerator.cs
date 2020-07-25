using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using COM.Utils;

namespace COM.World.Experimental
{
    /// <summary>
    /// GPGPU voxel mesh generator
    /// </summary>
    [System.Serializable]
    public class VoxelGenerator
    {
        public VoxelFaceGenerator FaceLeftGenerator;
        public VoxelFaceGenerator FaceForwardGenerator;
        public VoxelFaceGenerator FaceRightGenerator;
        public VoxelFaceGenerator FaceBackwardGenerator;
        public VoxelFaceGenerator FaceUpGenerator;
        public VoxelFaceGenerator FaceDownGenerator;

        #region Setup
        public void Init(int cubesPerAxis, float cubeSize)
        {
            FaceLeftGenerator.Init(cubesPerAxis, cubeSize);
            FaceForwardGenerator.Init(cubesPerAxis, cubeSize);
            FaceRightGenerator.Init(cubesPerAxis, cubeSize);
            FaceBackwardGenerator.Init(cubesPerAxis, cubeSize);
            FaceUpGenerator.Init(cubesPerAxis, cubeSize);
            FaceDownGenerator.Init(cubesPerAxis, cubeSize);
        }
        #endregion

        #region Output
        public (Vector3[], int[], FragRegionIndex) Result(int chunkCoordX, int chunkCoordY, int chunkCoordZ, int cubesPerAxis, VoxelNoiseGenerator noiseGenerator)
        {
            int chunkOffsetX = chunkCoordX * 3;
            int chunkOffsetY = chunkCoordY * 3;
            int chunkOffsetZ = chunkCoordZ * 3;
            int maxRectangleGPUCount = (cubesPerAxis * cubesPerAxis * cubesPerAxis) * 5;

            //How many to process on a single thread
            int processPerThread = Mathf.CeilToInt(cubesPerAxis / 8.0f);

            //RWStructuredBuffer sets
            ComputeBuffer rectanglesBuffer = new ComputeBuffer(maxRectangleGPUCount, GPURectangle.Stride, ComputeBufferType.Append);
            ComputeBuffer rectCountBuffer = new ComputeBuffer(1, Stride.IntStride, ComputeBufferType.Raw);
            rectanglesBuffer.SetCounterValue(0);

            FragRegionIndex fragRegionIndex = new FragRegionIndex
            {
                Index0 = -1,
                Index1 = -1,
                Index2 = -1,
                Index3 = -1,
            };
            //Generate sub chunks to combine into a whole chunk
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        //Run noise kernal
                        ComputeBuffer noiseShader = noiseGenerator.Result(chunkOffsetX + x, chunkOffsetY + y, chunkOffsetZ + z, cubesPerAxis, ref fragRegionIndex);

                        //Run face kernal
                        FaceLeftGenerator.Result(new Vector3Int(x, y, z), new Vector3Int(processPerThread, 1, 1), noiseShader, rectanglesBuffer);
                        FaceForwardGenerator.Result(new Vector3Int(x, y, z), new Vector3Int(1, 1, processPerThread), noiseShader, rectanglesBuffer);
                        FaceRightGenerator.Result(new Vector3Int(x, y, z), new Vector3Int(processPerThread, 1, 1), noiseShader, rectanglesBuffer);
                        FaceBackwardGenerator.Result(new Vector3Int(x, y, z), new Vector3Int(1, 1, processPerThread), noiseShader, rectanglesBuffer);
                        FaceUpGenerator.Result(new Vector3Int(x, y, z), new Vector3Int(1, processPerThread, 1), noiseShader, rectanglesBuffer);
                        FaceDownGenerator.Result(new Vector3Int(x, y, z), new Vector3Int(1, processPerThread, 1), noiseShader, rectanglesBuffer);

                        //Get rid of buffer data
                        noiseShader.Dispose();
                    }
                }
            }

            //Grab output count
            ComputeBuffer.CopyCount(rectanglesBuffer, rectCountBuffer, 0);
            int[] rectCount = { 0 };
            rectCountBuffer.GetData(rectCount);

            //Outputs
            GPURectangle[] rectangleGPUOutput = new GPURectangle[rectCount[0]];
            Vector3[] verticesOutput = new Vector3[rectangleGPUOutput.Length * 4];
            int[] trianglesOutput = new int[rectangleGPUOutput.Length * 6];

            //Grab outputs
            rectanglesBuffer.GetData(rectangleGPUOutput, 0, 0, rectCount[0]);

            //Get rid of buffer data
            rectanglesBuffer.Dispose();
            rectCountBuffer.Dispose();

            //Process triangles
            for (int i = 0; i < rectangleGPUOutput.Length; i++)
            {
                int vertIndex = i * 4;
                int triIndex = i * 6;

                //Vertices
                verticesOutput[vertIndex] = rectangleGPUOutput[i].VertexA;
                verticesOutput[vertIndex + 1] = rectangleGPUOutput[i].VertexB;
                verticesOutput[vertIndex + 2] = rectangleGPUOutput[i].VertexC;
                verticesOutput[vertIndex + 3] = rectangleGPUOutput[i].VertexD;

                //Triangles
                trianglesOutput[triIndex] = vertIndex;
                trianglesOutput[triIndex + 1] = vertIndex + 1;
                trianglesOutput[triIndex + 2] = vertIndex + 2;
                trianglesOutput[triIndex + 3] = vertIndex + 2;
                trianglesOutput[triIndex + 4] = vertIndex + 3;
                trianglesOutput[triIndex + 5] = vertIndex;
            }
            return (verticesOutput, trianglesOutput, fragRegionIndex);
        }
        #endregion

        #region Utils
        public void Dispose()
        {
            FaceLeftGenerator.Dispose();
            FaceForwardGenerator.Dispose();
            FaceRightGenerator.Dispose();
            FaceBackwardGenerator.Dispose();
            FaceUpGenerator.Dispose();
            FaceDownGenerator.Dispose();
        }
        #endregion
    }
}
