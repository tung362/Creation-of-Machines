using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace COM.Manager
{
    //All options menu settings are configured here
    public class OptionSettings
    {
        /*Graphics Settings*/
        public static Vector2 Resolution = new Vector2(1920, 1080);
        public static bool Fullscreen = true;
        public static bool Vsync = true;
        //Post Processing
        public static bool Bloom = true;
        public static float BloomIntensity = 0.3f;
        public static bool Antialiasing = true;
        //public static AntialiasingModel.Method AntialiasingMethod = AntialiasingModel.Method.Fxaa;
        public static bool DepthOfField = true;

        /*Audio Settings*/
        public static float MusicVolume = 1.0f;
        public static float SoundVolume = 1.0f;

        /*Controls*/
        public static KeyCode CameraForward = KeyCode.W;
        public static KeyCode CameraBackward = KeyCode.S;
        public static KeyCode CameraLeft = KeyCode.A;
        public static KeyCode CameraRight = KeyCode.D;

        public static void Load()
        {

        }

        public static void Save()
        {

        }
    }
}
