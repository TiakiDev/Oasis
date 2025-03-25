using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


public class CraftingSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemIcon;

    [SerializeField] private CraftingRecipeSO recipe;


    private void Start()
    {
        itemIcon.sprite = recipe.outputItem.itemIcon;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CraftingManager.instance.SelectItemToCraft(recipe);
    }
    
    public void Initialize(CraftingRecipeSO newRecipe)
    {
        recipe = newRecipe;
        itemIcon.sprite = recipe.outputItem.itemIcon;
    }
    
    public void UpdateSearchVisibility(string searchQuery)
    {
        bool shouldShow = string.IsNullOrEmpty(searchQuery) || 
                          recipe.outputItem.itemName.ToLower().Contains(searchQuery.ToLower()) ||
                          recipe.outputItem.itemDescription.ToLower().Contains(searchQuery.ToLower());
        
        gameObject.SetActive(shouldShow);
    }
}
