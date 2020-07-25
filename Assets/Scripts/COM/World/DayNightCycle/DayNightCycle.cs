using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace COM.World
{
    /// <summary>
    /// Day and night cycle system
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Skybox Settings")]
        public Material Skybox;
        public Light Sun;
        public GameObject SunOffset;
        public Vector3 SkyDirection = Vector3.left;
        [Header("Time")]
        public bool Paused = false;
        [Range(0.0f, 1.0f)]
        public float CurrentTime = 0;
        public float CycleDuration = 20;

        private float CycleTimer = 0;

        void LateUpdate()
        {
            if (!Paused)
            {
                CycleTimer += Time.deltaTime;
                if (CycleTimer >= CycleDuration) CycleTimer -= CycleDuration;
                CurrentTime = CycleTimer / CycleDuration;
            }

            //Debug.Log(Vector3.Lerp(new Vector3(0, 0, 1), new Vector3(1, 0, 0), 0.5f));
            //Debug.Log(Vector3.Distance(new Vector3(0, 1, 0), new Vector3(0.75f, 0.75f, 0.75f)));

            Sun.transform.rotation = Quaternion.Euler(360 * CurrentTime, 0, 0);
            //Sun.intensity = SunIntensity.Evaluate(CurrentTime);
            //Sun.color = SunColor.Evaluate(CurrentTime);
            //RenderSettings.ambientIntensity = AmbientIntensity.Evaluate(CurrentTime);
            //RenderSettings.fogColor = FogColor.Evaluate(CurrentTime);
            //Debug.Log(Test.transform.forward);
            Skybox.SetVector("_SunDirection", Sun.transform.forward);
            Skybox.SetVector("_SunOffset", SunOffset.transform.forward);
            Skybox.SetVector("_TimeDirection", SkyDirection);
            Skybox.SetFloat("_TimeStep", CurrentTime);
            //Debug.Log(Vector3.Cross(new Vector3(1, 0, 0), Vector3.up).normalized);
            //Debug.Log(Vector3.Cross(Vector3.Cross(new Vector3(1, 0, 0), Vector3.up).normalized, new Vector3(1, 0, 0)).normalized);
        }
    }
}
