using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightSystem : MonoBehaviour
{
    public static DayNightSystem instance;
    
    public Light directionalLight;
    
    [Header("Time Settings")]
    public float dayDurationInSeconds = 24f;
    public int currentHour;
    public float currentTimeOfDay = 0.55f;

    [Header("Light Settings")]
    public AnimationCurve lightIntensityCurve;
    public float maxIntensity = 1.0f;

    [Header("Skybox Settings")]
    public List<SkyboxTimeMapping> timeMappings; 
    
    private float blendedValue = 0f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    
    private void Start()
    {
            lightIntensityCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.3f),
                new Keyframe(0.25f, 0.5f),
                new Keyframe(0.5f, 1.0f),
                new Keyframe(0.75f, 0.5f),
                new Keyframe(1.0f, 0.3f)
            );
            lightIntensityCurve.SmoothTangents(1, 0.5f);
            lightIntensityCurve.SmoothTangents(2, 0.5f);
    }

    private void Update()
    {
        UpdateTime();
        UpdateSunLight();
        UpdateSkybox();
    }

    private void UpdateTime()
    {
        currentTimeOfDay += Time.deltaTime / dayDurationInSeconds;
        currentTimeOfDay %= 1;
        currentHour = Mathf.FloorToInt(currentTimeOfDay * 24);
    }

    private void UpdateSunLight()
    {
        // Aktualizacja rotacji światła
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3((currentTimeOfDay * 360) - 90, 170, 0));
        
        // Aktualizacja intensywności światła
        float intensity = lightIntensityCurve.Evaluate(currentTimeOfDay) * maxIntensity;
        directionalLight.intensity = intensity;
    }

    private void UpdateSkybox()
    {
        Material currentSkybox = null;
        foreach (SkyboxTimeMapping mapping in timeMappings)
        {
            if (currentHour == mapping.hour)
            {
                currentSkybox = mapping.skyboxMaterial;

                if (currentSkybox.shader != null && currentSkybox.shader.name == "Custom/SkyboxTransition")
                {
                    blendedValue += Time.deltaTime;
                    blendedValue = Mathf.Clamp01(blendedValue);
                    currentSkybox.SetFloat("_TransitionFactor", blendedValue);
                }
                else
                {
                    blendedValue = 0f;
                }
                break;
            }
        }

        if (currentSkybox != null)
        {
            RenderSettings.skybox = currentSkybox;
        }
    } 
}

[System.Serializable]
public class SkyboxTimeMapping
{
    public string phaseName;
    public int hour;
    public Material skyboxMaterial;
}