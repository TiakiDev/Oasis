using UnityEngine;

public class Furnace : InteractableObject
{
    [Header("Inventory")]
    public ItemSO inputItem;
    public int inputQuantity;
    public ItemSO fuelItem;
    public int fuelQuantity;
    public ItemSO outputItem;
    public int outputQuantity;

    [Header("Smelting")]
    public float currentSmeltTime;
    public float remainingBurnTime;
    public SmeltingRecipeSO currentRecipe;
    public bool isBurning; // Dodana zmienna stanu

    [Header("Visual effects")]
    public ParticleSystem fire;
    public ParticleSystem smoke;
    public GameObject light;

    void Update()
    {
        UpdateBurnTime();
        UpdateSmelting();
        UpdateParticles();
    }

    void UpdateBurnTime()
    {
        if (remainingBurnTime <= 0 && fuelQuantity > 0)
        {
            ConsumeFuel();
        }
        else
        {
            remainingBurnTime -= Time.deltaTime;
        }
    }

    void ConsumeFuel()
    {
        if (fuelItem == null) return;

        var fuel = fuelItem;
        fuelQuantity--;
        remainingBurnTime = fuel.burnTime;

        if (fuelQuantity <= 0)
        {
            fuelItem = null;
        }
        if (InventoryManager.instance.isOpen && InventoryManager.instance.currentFurnace == this)
        {
            InventoryManager.instance.UpdateFurnaceUI();
        }
    }

    void UpdateSmelting()
    {
        isBurning = CanSmelt() && remainingBurnTime > 0;

        if (isBurning)
        {
            currentSmeltTime += Time.deltaTime;
            if (currentSmeltTime >= currentRecipe.smeltingTime)
            {
                SmeltItem();
            }
        }
        else
        {
            currentSmeltTime = 0;
        }
    }
    
    bool CanSmelt()
    {
        currentRecipe = FurnaceManager.GetRecipe(inputItem);
        if (currentRecipe == null) return false;
        if (outputItem != null && outputItem != currentRecipe.outputItem) return false;
        return inputQuantity > 0 && remainingBurnTime > 0;
    }

    void SmeltItem()
    {
        inputQuantity--;
        currentSmeltTime = 0;

        if (outputItem == null) outputItem = currentRecipe.outputItem;
        outputQuantity++;
        
        if (InventoryManager.instance.isOpen && InventoryManager.instance.currentFurnace == this)
        {
            InventoryManager.instance.UpdateFurnaceUI();
        }
    }
    
    void UpdateParticles()
    {
        if (fire != null)
        {
            if (isBurning && !fire.isPlaying)
            {
                fire.Play();
                light.SetActive(true);
                
            }
            else if (!isBurning && fire.isPlaying)
            {
                fire.Stop();
                light.SetActive(false);
            }
        }
        if (smoke != null)
        {
            if (isBurning && !smoke.isPlaying) smoke.Play();
            else if (!isBurning && smoke.isPlaying) smoke.Stop();
        }
    }

    public void InteractFurnace()
    {
        InventoryManager.instance.OpenFurnace(this);
    }
}