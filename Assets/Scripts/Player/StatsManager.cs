using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class StatsManager : MonoBehaviour
{
    public static StatsManager instance;


    [Header("Stats")]
    public float maxHealth = 100;
    public float health;
    public float hunger;
    public float thirst;
    public float oxygen;
    [Space]
    [Header("Hunger & Thirst Depletion")]
    [SerializeField] private float hungerDepletionRate = 1f; // 1 punkt na sekundę
    [SerializeField] private float thirstDepletionRate = 1f;
    [SerializeField] private float defaultHungerDepletionInterval = 5f;
    [SerializeField] public float defaultThirstDepletionInterval = 5f;
    [Space]
    [Header("Health Drain Settings")]
    [SerializeField] private int lowHungerThreshold = 10;
    [SerializeField] private int lowThirstThreshold = 10;
    [SerializeField] private float healthDrainInterval = 5f;
    [SerializeField] private int healthDrainAmount = 1;
    [Header("Oxygen Drain Settings")]
    [SerializeField] private float oxygenDrainInterval = 1f;
    [SerializeField] private int oxygenDrainAmount = 1;
    [Space]
    [Header("References")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image foodBar;
    [SerializeField] private Image waterBar;
    [SerializeField] private Image oxygenBar;
    [SerializeField] public GameObject oxygenBarGameObject;
    
    //*Internal variables
    
    //Timers
    private float hungerTimer;
    private float thirstTimer;
    private float healthDrainTimer;
    private float oxygenDrainTimer;
    //Depletion Intervals
    [HideInInspector] public float hungerDepletionInterval;
    [HideInInspector] public float thirstDepletionInterval;
    [HideInInspector] public bool shouldDrainOxygen;

    private void Start()
    {
        
        hungerDepletionInterval = defaultHungerDepletionInterval;
        thirstDepletionInterval = defaultThirstDepletionInterval;
        
        
        health = maxHealth;
        hunger = 100;
        thirst = 100;
        oxygen = 100;
    }
    
    private void Update()
    {
        HandleStatDepletion();
        HandleHealthDrain();
        HandleOxygenDrain();
        
        if(health <= 0)
        {
            Destroy(gameObject);
            Debug.Log("Player died");
        }
    }

    private void UpdateBars()
    {
        healthBar.fillAmount = health / maxHealth;
        foodBar.fillAmount = hunger / 100;
        waterBar.fillAmount = thirst / 100;
        oxygenBar.fillAmount = oxygen / 100;
    }
    
    public void ChangeHealth(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateBars();
    }
    public void ChangeHunger(float amount)
    {
        hunger += amount;
        hunger = Mathf.Clamp(hunger, 0, 100);
        UpdateBars();
    }
    public void ChangeThirst(float amount)
    {
        thirst += amount;
        thirst = Mathf.Clamp(thirst, 0, 100);
        UpdateBars();
    }
    
    public void ChangeOxygen(float amount)
    {
        oxygen += amount;
        oxygen = Mathf.Clamp(oxygen, 0, 100);
        UpdateBars();
    }
    private void HandleHealthDrain()
    {
        bool shouldDrainHealth = hunger <= lowHungerThreshold || thirst <= lowThirstThreshold;

        if (shouldDrainHealth)
        {
            healthDrainTimer += Time.deltaTime;

            if (healthDrainTimer >= healthDrainInterval)
            {
                ChangeHealth(-healthDrainAmount);
                healthDrainTimer = 0f;
            }
        }
        else
        {
            healthDrainTimer = 0f;
        }
    }
    
    private void HandleOxygenDrain()
    {
        if (shouldDrainOxygen)
        {
            oxygen -= oxygenDrainAmount * Time.deltaTime;
            oxygen = Mathf.Clamp(oxygen, 0, 100);
            UpdateBars();
        }
        if(oxygen <= 0)
        {
            ChangeHealth(-healthDrainAmount * Time.deltaTime);
        }
    }
    
    private void HandleStatDepletion()
    {
        hungerTimer += Time.deltaTime;
        thirstTimer += Time.deltaTime;

        if (hungerTimer >= hungerDepletionInterval && hunger > 0)
        {
            hunger -= (int)hungerDepletionRate;
            hungerTimer = 0;
            UpdateBars();
        }

        if (thirstTimer >= thirstDepletionInterval && thirst > 0)
        {
            thirst -= (int)thirstDepletionRate;
            thirstTimer = 0;
            UpdateBars();
        }
        
    }

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
}
