using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceHealthBar : MonoBehaviour
{
    private Slider slider;
    private float currentHealth, maxHealth;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Update()
    {
        currentHealth = GlobalState.instance.resourceHealth;
        maxHealth = GlobalState.instance.resourceMaxHealth;
        
        slider.value = currentHealth / maxHealth;
    }
}
