using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace COM.World
{
    //Day and night cycle system that controls all the environmental lighting within the game
    public class DayNightCycle : MonoBehaviour
    {
        [Header("General Settings")]
        public Light Sun;
        public bool Paused = false;
        [Range(0.0f, 1.0f)]
        public float CurrentTime = 0;
        public float CycleDuration = 20;
        public AnimationCurve CycleCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [Header("Sun Settings")]
        //public float DaySunIntensity = 2.5f;
        //public float NightSunIntensity = 0.5f;
        public AnimationCurve SunIntensity = AnimationCurve.Linear(0, 0, 1, 1);
        //public Color DaySunColor;
        //public Color NightSunColor;
        public Gradient SunColor;
        [Header("Ambient Settings")]
        //public float DayAmbientIntensity = 1f;
        //public float NightAmbientIntensity = 0.69f;
        public AnimationCurve AmbientIntensity = AnimationCurve.Linear(0, 0, 1, 1);
        [Header("Skybox Settings")]
        public Material DaySkybox;
        public Material NightSkybox;
        [Header("Fog Settings")]
        //public Color DayFogColor;
        //public Color NightFogColor;
        public Gradient FogColor;

        private float CycleTimer = 0;

        void LateUpdate()
        {
            if (!Paused)
            {
                CycleTimer += Time.deltaTime;
                if (CycleTimer >= CycleDuration) CycleTimer -= CycleDuration;
                CurrentTime = CycleTimer / CycleDuration;
            }

            float currentTimeValue = CycleCurve.Evaluate(CurrentTime);
            Sun.transform.eulerAngles = Vector3.Lerp(new Vector3(Sun.transform.eulerAngles.x, -63.0f, Sun.transform.eulerAngles.z), new Vector3(Sun.transform.eulerAngles.x, -63.0f + (360 * 2), Sun.transform.eulerAngles.z), CurrentTime);
            //Sun.intensity = Mathf.Lerp(NightSunIntensity, DaySunIntensity, currentTimeValue);
            //Sun.color = Color.Lerp(NightSunColor, DaySunColor, currentTimeValue);
            //RenderSettings.ambientIntensity = Mathf.Lerp(NightAmbientIntensity, DayAmbientIntensity, currentTimeValue);
            //Material blendedSkybox = new Material(RenderSettings.skybox);
            //blendedSkybox.Lerp(NightSkybox, DaySkybox, currentTimeValue);
            //RenderSettings.skybox = blendedSkybox;
            //RenderSettings.fogColor = Color.Lerp(NightFogColor, DayFogColor, currentTimeValue);

            Sun.intensity = SunIntensity.Evaluate(CurrentTime);
            Sun.color = SunColor.Evaluate(CurrentTime);
            RenderSettings.ambientIntensity = AmbientIntensity.Evaluate(CurrentTime);

            //Material blendedSkybox = new Material(RenderSettings.skybox);
            //blendedSkybox.Lerp(NightSkybox, DaySkybox, currentTimeValue);
            //RenderSettings.skybox = blendedSkybox;

            RenderSettings.fogColor = FogColor.Evaluate(CurrentTime);

        }
    }
}
