using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM.World
{
    /// <summary>
    /// Vertices representing a triangle with gpu compatibility
    /// </summary>
    [System.Serializable]
    public struct GPUTriangle
    {
        public const int Stride = 36;

        public Vector3 vertexA;
        public Vector3 vertexB;
        public Vector3 vertexC;
    }
}
