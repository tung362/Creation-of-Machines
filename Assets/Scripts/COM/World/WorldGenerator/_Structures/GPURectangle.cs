using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM.World
{
    /// <summary>
    /// Vertices representing a rectangle with gpu compatibility
    /// </summary>
    [System.Serializable]
    public struct GPURectangle
    {
        public const int Stride = 48;

        public Vector3 VertexA;
        public Vector3 VertexB;
        public Vector3 VertexC;
        public Vector3 VertexD;
    }
}
