using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM.World
{
    /// <summary>
    /// Indexes representing region ids a chunk belongs to for shader use in precomputation closest distance check
    /// </summary>
    [System.Serializable]
    public struct FragRegionIndex
    {
        public int Index0;
        public int Index1;
        public int Index2;
        public int Index3;
    }
}
