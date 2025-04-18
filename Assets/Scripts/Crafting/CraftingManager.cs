using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager instance;
    public List<CraftingRecipeSO> allRecipes;

    private List<Workbench> activeWorkbenches = new List<Workbench>();
    public int currentMaxTier = 0;
    
    //UI
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;
    public Image itemIcon;
    public TMP_Text requirementsText;
    public TMP_Text workbenchTierText;
    public Image craftingButtonEffect;
    
    public CraftingRecipeSO selectedRecipe;
    public Transform slotsContainer;
    public TMP_InputField searchInputField;

    public GameObject slotPrefab;
    
    [Header("Requirements UI")]
    public Transform requirementsContainer; // Kontener dla elementów wymagań
    public GameObject requirementEntryPrefab; // Prefab dla pojedynczego wymagania
    private List<RequirementEntry> currentRequirementEntries = new List<RequirementEntry>();
    
    
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
        GenerateCraftingSlots();
    }
    
    private void GenerateCraftingSlots(string searchQuery = "")
    {
        foreach (Transform child in slotsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (CraftingRecipeSO recipe in allRecipes)
        {
            if(MatchesSearch(recipe, searchQuery))
            {
                GameObject slot = Instantiate(slotPrefab, slotsContainer);
                CraftingSlot slotUI = slot.GetComponent<CraftingSlot>();
                if (slotUI != null)
                {
                    slotUI.Initialize(recipe);
                }
            }
        }
    }

    public void OnSearchInputChanged()
    {
        GenerateCraftingSlots(searchInputField.text);
    }
    
    private bool MatchesSearch(CraftingRecipeSO recipe, string searchQuery)
    {
        if(string.IsNullOrWhiteSpace(searchQuery)) return true;
    
        string query = searchQuery.ToLower();
        return recipe.outputItem.itemName.ToLower().Contains(query) ||
               recipe.outputItem.itemDescription.ToLower().Contains(query);
    }

    public void SelectItemToCraft(CraftingRecipeSO recipe)
    {
        selectedRecipe = recipe;
    
        itemNameText.text = recipe.outputItem.itemName;
        itemDescriptionText.text = recipe.outputItem.itemDescription;
        itemIcon.sprite = recipe.outputItem.itemIcon;

        // Wyczyść poprzednie wymagania
        foreach (Transform child in requirementsContainer)
        {
            Destroy(child.gameObject);
        }
        currentRequirementEntries.Clear();

        // Dodaj nowe wymagania
        foreach (var req in recipe.requiredItems)
        {
            GameObject entry = Instantiate(requirementEntryPrefab, requirementsContainer);
            RequirementEntry entryUI = entry.GetComponent<RequirementEntry>();
        
            entryUI.item = req.item;
            entryUI.icon.sprite = req.item.itemIcon;
        
            int currentAmount = InventoryManager.instance.GetItemQuantity(req.item);
            string color = (currentAmount >= req.quantity) ? "green" : "red";
            entryUI.text.text = $"<color={color}>{req.item.itemName}: {currentAmount}/{req.quantity}</color>";
        
            currentRequirementEntries.Add(entryUI);
        }

        string tierInfo = (recipe.requiredWorkbenchTier > 0) ? $"Workbench tier required: {recipe.requiredWorkbenchTier}" : "No workbench required";
        workbenchTierText.text = tierInfo;
    }

    public void AddActiveWorkbench(Workbench workbench)
    {
        if (!activeWorkbenches.Contains(workbench))
        {
            activeWorkbenches.Add(workbench);
            UpdateMaxTier();
        }
    }

    public void RemoveActiveWorkbench(Workbench workbench)
    {
        if (activeWorkbenches.Contains(workbench))
        {
            activeWorkbenches.Remove(workbench);
            UpdateMaxTier();
        }
    }

    private void UpdateMaxTier()
    {
        currentMaxTier = 0;
        foreach (Workbench bench in activeWorkbenches)
        {
            if (bench.tier > currentMaxTier)
            {
                currentMaxTier = bench.tier;
            }
        }
    }

    public bool CanCraft(CraftingRecipeSO recipe)
    {
        // Sprawdź tier workbencha
        if (recipe.requiredWorkbenchTier > currentMaxTier) return false;

        // Sprawdź dostępność przedmiotów
        foreach (CraftingRecipeSO.ItemRequirement requirement in recipe.requiredItems)
        {
            if (!InventoryManager.instance.HasEnoughItems(requirement.item, requirement.quantity))
            {
                return false;
            }
        }
        return true;
    }

    public void CraftItem()
    {
        if (!CanCraft(selectedRecipe) || selectedRecipe == null) return;

        // Usuń wymagane przedmioty
        foreach (CraftingRecipeSO.ItemRequirement requirement in selectedRecipe.requiredItems)
        {
            InventoryManager.instance.RemoveItem(requirement.item, requirement.quantity);
        }

        // Dodaj wytworzony przedmiot
        StartCoroutine(CraftingCoroutine(1f));
    }

    private IEnumerator CraftingCoroutine(float targetFillAmount)
    {
        float startFillAmount = craftingButtonEffect.fillAmount;
        float elapsedTime = 0f;
        float fillDuration = 0.8f;
        
        while (elapsedTime < fillDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fillDuration);
            
            // Płynna interpolacja wartości fillAmount
            craftingButtonEffect.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, t);
            
            yield return null;
        }
        
        // Upewniamy się, że na końcu mamy dokładnie wartość docelową
        craftingButtonEffect.fillAmount = targetFillAmount;
        
        //* Tutaj jest to co się dzieje potem
        
        InventoryManager.instance.AddItem(selectedRecipe.outputItem, selectedRecipe.outputQuantity);
        
        craftingButtonEffect.fillAmount = 0f;
        UpdateRequirementsText();
    }


    public void UpdateRequirementsText()
    {
        if (selectedRecipe == null) return;

        foreach (RequirementEntry entry in currentRequirementEntries)
        {
            int currentAmount = InventoryManager.instance.GetItemQuantity(entry.item);
            int required = System.Array.Find(selectedRecipe.requiredItems, r => r.item == entry.item).quantity;
            string color = (currentAmount >= required) ? "green" : "red";
            entry.text.text = $"<color={color}>{entry.item.itemName}: {currentAmount}/{required}</color>";
        }
    }
}
