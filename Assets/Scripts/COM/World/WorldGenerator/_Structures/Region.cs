using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

namespace COM.World
{
    /// <summary>
    /// Mapper for the specific area created by the world generator
    /// </summary>
    [System.Serializable]
    public class Region
    {
        public string RegionName;
        public int FactionID;
        public Site RegionSite;
        public int SurfaceBiomeID;
        public int CaveBiomeID;
    }
}
