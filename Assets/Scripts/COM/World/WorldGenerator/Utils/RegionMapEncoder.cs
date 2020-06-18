using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using COM.World;

namespace COM.Utils.World
{
    /// <summary>
    /// Texture encoder for encoding RegionMaps into a texture for shader use
    /// Ensure the following settings for your Texture2D : TextureFormat.RGBAFloat, TextureWrapMode.Clamp, FilterMode.Point;
    /// </summary>
    public static class RegionMapEncoder
    {
        /*Const*/
        public const int EncodeSize = 11;

        /*Save to texture*/
        public static void EncodeRegionMap(Region encode, Texture2D dataTexture, int index, out int nextIndex)
        {
            //Map out closest neighbors
            List<Site> neighborSites = encode.RegionSite.NeighborSites();
            if (neighborSites.Count > 8) Debug.Log("Warning! Neighbor site count greater than 8! Count: " + neighborSites.Count + " @CreateRegionFragGPUs(List<Region> mapRegions)");

            nextIndex = index;
            TextureEncoder.EncodeVector2(encode.RegionSite.Coord, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeFloat(encode.SurfaceBiomeID, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeFloat(encode.CaveBiomeID, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeFloat(0 < neighborSites.Count ? neighborSites[0].SiteIndex : -1, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeFloat(1 < neighborSites.Count ? neighborSites[1].SiteIndex : -1, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeFloat(2 < neighborSites.Count ? neighborSites[2].SiteIndex : -1, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeFloat(3 < neighborSites.Count ? neighborSites[3].SiteIndex : -1, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeFloat(4 < neighborSites.Count ? neighborSites[4].SiteIndex : -1, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeFloat(5 < neighborSites.Count ? neighborSites[5].SiteIndex : -1, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeFloat(6 < neighborSites.Count ? neighborSites[6].SiteIndex : -1, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeFloat(7 < neighborSites.Count ? neighborSites[7].SiteIndex : -1, dataTexture, nextIndex, out nextIndex);
        }

        public static Texture2D CreateTexture(List<Region> regions)
        {
            Texture2D texture = new Texture2D(regions.Count * EncodeSize, 1, TextureFormat.RGBAFloat, false, true);
            int index = 0;
            for (int i = 0; i < regions.Count; i++) EncodeRegionMap(regions[i], texture, index, out index);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return texture;
        }
    }
}
