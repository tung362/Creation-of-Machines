using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM.World.Experimental
{
    /// <summary>
    /// Cube representing a voxel with gpu compatibility
    /// </summary>
    [System.Serializable]
    public struct GPUVoxel
    {
        public const int Stride = 4;

        //0 = solid, 1 = face left, 2 = face forward, 3 = face right, 4 = face backward, 5 = face up, 6 = face down
        public int FlagMask;

        public bool HasFlag(int flag)
        {
            return ((int)FlagMask & 1 << (int)flag) != 0;
        }

        public void AddFlag(int flag)
        {
            FlagMask |= (int)(1 << (int)flag);
        }

        public void RemoveFlag(int flag)
        {
            FlagMask &= (int)(~(int)(1 << (int)flag));
        }
    }
}
