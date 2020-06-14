using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using COM.World;

namespace COM.Utils.World
{
    /// <summary>
    /// Texture encoder for encoding CaveRegions into a texture for shader use
    /// Ensure the following settings for your Texture2D : TextureFormat.RGBAFloat, TextureWrapMode.Clamp, FilterMode.Point;
    /// </summary>
    public static class CaveRegionEncoder
    {
        /*Const*/
        public const int EncodeSize = 18;

        /*Save to texture*/
        public static void EncodeCaveRegion(RegionCaveBiome encode, Texture2D dataTexture, int index, out int nextIndex)
        {
            nextIndex = index;
            GradientEncoder.EncodeGradient(encode.GroundPalette, dataTexture, nextIndex, out nextIndex);
            GradientEncoder.EncodeGradient(encode.WallPalette, dataTexture, nextIndex, out nextIndex);
        }

        public static Texture2D CreateTexture(List<RegionCaveBiome> CaveBiomes)
        {
            Texture2D texture = new Texture2D(CaveBiomes.Count * EncodeSize, 1, TextureFormat.RGBAFloat, false, true);
            int index = 0;
            for (int i = 0; i < CaveBiomes.Count; i++) EncodeCaveRegion(CaveBiomes[i], texture, index, out index);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return texture;
        }
    }
}
