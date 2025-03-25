using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireflyManager : MonoBehaviour
{
    [Header("Firefly Settings")]
    public List<ParticleSystem> fireflies;
    public float fadeDuration = 3.0f;
    
    private float previousTime = -1f;
    private bool isActivePeriod;

    private void Start()
    {
        // Wyłącz wszystkie świetliki na starcie gry
        foreach (var system in fireflies)
        {
            if (system == null) continue;
            
            var main = system.main;
            Color color = main.startColor.color;
            color.a = 0f;
            main.startColor = color;
            system.Stop();
        }

        // Sprawdź czy gra zaczyna się w okresie aktywności
        float startTime = DayNightSystem.instance.currentTimeOfDay;
        isActivePeriod = startTime >= 0.85f || startTime < 0.2f;
        
        if (isActivePeriod)
        {
            StartCoroutine(FadeFireflies(true));
        }
    }

    private void Update()
    {
        float currentTime = DayNightSystem.instance.currentTimeOfDay;
        
        // Okres aktywności: 20:24 (0.85) - 4:48 (0.2)
        bool newActiveState = currentTime >= 0.85f || currentTime < 0.1f;

        if (newActiveState != isActivePeriod)
        {
            if (newActiveState)
            {
                StartCoroutine(FadeFireflies(true));
            }
            else
            {
                StartCoroutine(FadeFireflies(false));
            }
            isActivePeriod = newActiveState;
        }

        previousTime = currentTime;
    }

    private IEnumerator FadeFireflies(bool fadeIn)
    {
        float targetAlpha = fadeIn ? 1.0f : 0.0f;
        float startAlpha = GetCurrentAlpha();

        float timer = 0f;

        foreach (var system in fireflies)
        {
            if (system == null) continue;
            
            if (fadeIn && !system.isPlaying)
            {
                system.Play();
            }
        }

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / fadeDuration);
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            foreach (var system in fireflies)
            {
                if (system == null) continue;
                
                var main = system.main;
                Color color = main.startColor.color;
                color.a = currentAlpha;
                main.startColor = color;
            }

            yield return null;
        }

        if (!fadeIn)
        {
            foreach (var system in fireflies)
            {
                if (system != null) system.Stop();
            }
        }
    }

    private float GetCurrentAlpha()
    {
        if (fireflies.Count == 0 || fireflies[0] == null) return 0f;
        return fireflies[0].main.startColor.color.a;
    }
}