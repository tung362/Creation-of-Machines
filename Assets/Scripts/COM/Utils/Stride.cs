using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM.Utils
{
    /// <summary>
    /// Number of bytes for commonly used types
    /// </summary>
    public static class Stride
    {
        public const int IntStride = 4;
        public const int FloatStride = 4;
        public const int Vector2Stride = 8;
        public const int Vector3Stride = 12;
        public const int Vector4Stride = 16;
    }
}
