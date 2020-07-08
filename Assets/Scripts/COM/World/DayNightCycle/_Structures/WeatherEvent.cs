using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM.World
{
    /// <summary>
    /// Environmental settings for the specific weather event
    /// </summary>
    [System.Serializable]
    public struct WeatherEvent
    {
        public string Name;
        public AnimationCurve SunIntensity;
        public AnimationCurve AmbientIntensity;
        public Gradient FogColor;
        public AnimationCurve FogDensity;
        public List<GameObject> ParticlePrefabs;
    }
}
