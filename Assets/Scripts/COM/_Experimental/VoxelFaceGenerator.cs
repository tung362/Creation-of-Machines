using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using COM.Utils;

namespace COM.World.Experimental
{
    /// <summary>
    /// GPGPU voxel face mesh generator
    /// </summary>
    [System.Serializable]
    public class VoxelFaceGenerator
    {
        public ComputeShader FaceShader;

        /*Cache*/
        private int Kernel = -1;

        #region Setup
        public void Init(int cubesPerAxis, float cubeSize)
        {
            //Kernal
            Kernel = FaceShader.FindKernel("FaceGenerator");

            //Sets
            FaceShader.SetInts("VoxelDimensions", new int[3] { cubesPerAxis, cubesPerAxis, cubesPerAxis });
            FaceShader.SetFloat("CubeSize", cubeSize);
            FaceShader.SetInt("CubesPerAxis", cubesPerAxis);
        }
        #endregion

        #region Output
        public void Result(Vector3Int subChunkCoord, Vector3Int dispatchDim, ComputeBuffer voxelsBuffer, ComputeBuffer rectanglesBuffer)
        {
            //Sets
            FaceShader.SetInts("SubChunkCoord", new int[3] { subChunkCoord.x, subChunkCoord.y, subChunkCoord.z });

            //RWStructuredBuffer sets
            FaceShader.SetBuffer(Kernel, "Voxels", voxelsBuffer);
            FaceShader.SetBuffer(Kernel, "Rectangles", rectanglesBuffer);

            //Run kernal
            FaceShader.Dispatch(Kernel, dispatchDim.x, dispatchDim.y, dispatchDim.z);
        }
        #endregion

        #region Utils
        public void Dispose()
        {

        }
        #endregion
    }
}
