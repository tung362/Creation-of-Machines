using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM.Utils
{
    /// <summary>
    /// Texture encoder for encoding floats and vectors into a texture for shader use
    /// Ensure the following settings for your Texture2D : TextureFormat.RGBAFloat, TextureWrapMode.Clamp, FilterMode.Point;
    /// </summary>
    public static class TextureEncoder
    {
        public static void EncodeFloat(float encode, Texture2D dataTexture, int index, out int nextIndex)
        {
            nextIndex = index + 1;
            dataTexture.SetPixel(index, 0, new Color(encode, 0, 0, 0));
        }

        public static void EncodeVector2(Vector2 encode, Texture2D dataTexture, int index, out int nextIndex)
        {
            nextIndex = index + 1;
            dataTexture.SetPixel(index, 0, new Color(encode.x, encode.y, 0, 0));
        }

        public static void EncodeVector3(Vector3 encode, Texture2D dataTexture, int index, out int nextIndex)
        {
            nextIndex = index + 1;
            dataTexture.SetPixel(index, 0, new Color(encode.x, encode.y, encode.z, 0));
        }

        public static void EncodeVector4(Vector4 encode, Texture2D dataTexture, int index, out int nextIndex)
        {
            nextIndex = index + 1;
            dataTexture.SetPixel(index, 0, new Color(encode.x, encode.y, encode.z, encode.w));
        }
    }
}
