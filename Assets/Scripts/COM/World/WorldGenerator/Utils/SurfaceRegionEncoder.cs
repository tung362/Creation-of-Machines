﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using COM.Database.World;
using COM.World;

namespace COM.Utils.World
{
    /// <summary>
    /// Texture encoder for encoding SurfaceRegions into a texture for shader use
    /// Ensure the following settings for your Texture2D : TextureFormat.RGBAFloat, TextureWrapMode.Clamp, FilterMode.Point;
    /// </summary>
    public static class SurfaceRegionEncoder
    {
        /*Const*/
        public const int EncodeSize = 21;

        /*Save to texture*/
        public static void EncodeSurfaceRegion(SurfaceBiomeDatabase.SurfaceBiome encode, Texture2D dataTexture, int index, out int nextIndex)
        {
            nextIndex = index;
            TextureEncoder.EncodeFloat(encode.Height, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeFloat(encode.Floor, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeFloat(encode.AdditiveHeightLimit, dataTexture, nextIndex, out nextIndex);
            GradientEncoder.EncodeGradient(encode.GroundPalette, dataTexture, nextIndex, out nextIndex);
            GradientEncoder.EncodeGradient(encode.WallPalette, dataTexture, nextIndex, out nextIndex);
        }

        public static Texture2D CreateTexture(SurfaceBiomeDatabase.SurfaceBiomeList surfaceBiomes)
        {
            Texture2D texture = new Texture2D(surfaceBiomes.Count * EncodeSize, 1, TextureFormat.RGBAFloat, false, true);
            int index = 0;
            for (int i = 0; i < surfaceBiomes.Count; i++) EncodeSurfaceRegion(surfaceBiomes[i], texture, index, out index);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return texture;
        }
    }
}
