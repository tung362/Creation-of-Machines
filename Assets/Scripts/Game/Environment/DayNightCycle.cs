using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

//Day and night cycle system that controls all the environmental lighting within the game
public class DayNightCycle : MonoBehaviour
{
    public Light Sun;
    public bool Paused = false;
    [Range(0.0f, 1.0f)]
    public float CurrentTime = 0;
    public float CycleDuration = 20;
    public float MaxSunIntensity = 2.5f;
    public float MinAmbientIntensity = 1.2f;
    public float MaxAmbientIntensity = 3.45f;
    public float AuraDayAmbientStrength = 0.5f;
    public float AuraNightAmbientStrength = 0.27f;
    public float AuraDayAmbientDensity = 0.12f;
    public float AuraNightAmbientDensity = 0.3f;
    public AnimationCurve CycleCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public Color AuraDayAmbientColor = Color.white;
    public Color AuraNightAmbientColor = Color.black;

    private float CycleTimer = 0;

    private AuraAPI.Aura VolumetricLight;

    void Start()
    {
        VolumetricLight = GetComponent<AuraAPI.Aura>();
    }

    void LateUpdate()
    {
        if(!Paused)
        {
            CycleTimer += Time.deltaTime;
            if (CycleTimer >= CycleDuration) CycleTimer -= CycleDuration;
            CurrentTime = CycleTimer / CycleDuration;
        }

        float currentTimeValue = CycleCurve.Evaluate(CurrentTime);
        Sun.transform.eulerAngles = Vector3.Lerp(new Vector3(Sun.transform.eulerAngles.x, 31.1f, Sun.transform.eulerAngles.z), new Vector3(Sun.transform.eulerAngles.x, 31.1f + (360 * 2), Sun.transform.eulerAngles.z), CurrentTime);
        Sun.intensity = currentTimeValue * MaxSunIntensity;
        VolumetricLight.frustum.settings.color = Color.Lerp(AuraNightAmbientColor, AuraDayAmbientColor, currentTimeValue);
        VolumetricLight.frustum.settings.density = Mathf.Lerp(AuraNightAmbientDensity, AuraDayAmbientDensity, currentTimeValue);
        VolumetricLight.frustum.settings.colorStrength = Mathf.Lerp(AuraNightAmbientStrength, AuraDayAmbientStrength, currentTimeValue);
        RenderSettings.ambientIntensity = Mathf.Lerp(MinAmbientIntensity, MaxAmbientIntensity, currentTimeValue);
    }
}
