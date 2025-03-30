// SmeltingManager.cs
using System.Collections.Generic;
using UnityEngine;

public class FurnaceManager : MonoBehaviour
{
    public static FurnaceManager instance;
    public List<SmeltingRecipeSO> recipes = new List<SmeltingRecipeSO>();

    void Awake() => instance = this;

    public static SmeltingRecipeSO GetRecipe(ItemSO input)
    {
        foreach (var recipe in instance.recipes)
        {
            if (recipe.inputItem == input) return recipe;
        }
        return null;
    }
}