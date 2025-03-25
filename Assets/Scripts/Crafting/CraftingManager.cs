using System;
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
    
    public CraftingRecipeSO selectedRecipe;
    public Transform slotsContainer;
    public TMP_InputField searchInputField;

    public GameObject slotPrefab;
    
    
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
        
        // Tekst wymagań
        string requirements = "";
        foreach (var req in recipe.requiredItems)
        {
            int currentAmount = InventoryManager.instance.GetItemQuantity(req.item);
            string color = (currentAmount >= req.quantity) ? "green" : "red";
            requirements += $"<color={color}>{req.item.itemName}: {currentAmount}/{req.quantity}</color>\n";
        }
        
        string tierInfo = (recipe.requiredWorkbenchTier > 0) ? $"Workbench tier {recipe.requiredWorkbenchTier} required" : "No workbench needed";

        requirementsText.text = requirements;
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
        if (!CanCraft(selectedRecipe)) return;

        // Usuń wymagane przedmioty
        foreach (CraftingRecipeSO.ItemRequirement requirement in selectedRecipe.requiredItems)
        {
            InventoryManager.instance.RemoveItem(requirement.item, requirement.quantity);
        }

        // Dodaj wytworzony przedmiot
        InventoryManager.instance.AddItem(selectedRecipe.outputItem, selectedRecipe.outputQuantity);
        
        UpdateRequirementsText();
    }
    
    private void UpdateRequirementsText()
    {
        if (selectedRecipe == null) return;

        // Aktualizacja wymagań przedmiotów
        string requirements = "";
        foreach (var req in selectedRecipe.requiredItems)
        {
            int currentAmount = InventoryManager.instance.GetItemQuantity(req.item);
            string color = (currentAmount >= req.quantity) ? "green" : "red";
            requirements += $"<color={color}>{req.item.itemName}: {currentAmount}/{req.quantity}</color>\n";
        }
        
        requirementsText.text = requirements;
    }
}
