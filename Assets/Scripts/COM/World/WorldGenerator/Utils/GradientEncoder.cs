using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM.Utils.World
{
    /// <summary>
    /// Texture encoder for encoding Gradients into a texture for shader use
    /// Ensure the following settings for your Texture2D : TextureFormat.RGBAFloat, TextureWrapMode.Clamp, FilterMode.Point;
    /// </summary>
    public static class GradientEncoder
    {
        /*Const*/
        public const int EncodeSize = 9;

        /*Save to texture*/
        public static void EncodeGradient(Gradient encode, Texture2D dataTexture, int index, out int nextIndex)
        {
            nextIndex = index;
            TextureEncoder.EncodeFloat(encode.colorKeys.Length, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeVector4(0 < encode.colorKeys.Length ? new Vector4(encode.colorKeys[0].color.r, encode.colorKeys[0].color.g, encode.colorKeys[0].color.b, encode.colorKeys[0].time) : Vector4.zero, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeVector4(1 < encode.colorKeys.Length ? new Vector4(encode.colorKeys[1].color.r, encode.colorKeys[1].color.g, encode.colorKeys[1].color.b, encode.colorKeys[1].time) : Vector4.zero, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeVector4(2 < encode.colorKeys.Length ? new Vector4(encode.colorKeys[2].color.r, encode.colorKeys[2].color.g, encode.colorKeys[2].color.b, encode.colorKeys[2].time) : Vector4.zero, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeVector4(3 < encode.colorKeys.Length ? new Vector4(encode.colorKeys[3].color.r, encode.colorKeys[3].color.g, encode.colorKeys[3].color.b, encode.colorKeys[3].time) : Vector4.zero, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeVector4(4 < encode.colorKeys.Length ? new Vector4(encode.colorKeys[4].color.r, encode.colorKeys[4].color.g, encode.colorKeys[4].color.b, encode.colorKeys[4].time) : Vector4.zero, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeVector4(5 < encode.colorKeys.Length ? new Vector4(encode.colorKeys[5].color.r, encode.colorKeys[5].color.g, encode.colorKeys[5].color.b, encode.colorKeys[5].time) : Vector4.zero, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeVector4(6 < encode.colorKeys.Length ? new Vector4(encode.colorKeys[6].color.r, encode.colorKeys[6].color.g, encode.colorKeys[6].color.b, encode.colorKeys[6].time) : Vector4.zero, dataTexture, nextIndex, out nextIndex);
            TextureEncoder.EncodeVector4(7 < encode.colorKeys.Length ? new Vector4(encode.colorKeys[7].color.r, encode.colorKeys[7].color.g, encode.colorKeys[7].color.b, encode.colorKeys[7].time) : Vector4.zero, dataTexture, nextIndex, out nextIndex);
        }

        public static Texture2D CreateTexture(List<Gradient> gradients)
        {
            Texture2D texture = new Texture2D(gradients.Count * EncodeSize, 1, TextureFormat.RGBAFloat, false, true);
            int index = 0;
            for (int i = 0; i < gradients.Count; i++) EncodeGradient(gradients[i], texture, index, out index);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return texture;
        }
    }
}
