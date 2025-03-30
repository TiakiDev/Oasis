using UnityEngine;

[CreateAssetMenu(fileName = "New Smelting Recipe", menuName = "Smelting/Recipe")]
public class SmeltingRecipeSO : ScriptableObject
{
    public ItemSO inputItem;
    public ItemSO outputItem;
    public float smeltingTime = 5.0f;

    public static SmeltingRecipeSO GetRecipeForInput(ItemSO input)
    {
        foreach (var recipe in Resources.LoadAll<SmeltingRecipeSO>("Recipes"))
        {
            if (recipe.inputItem == input) return recipe;
        }
        return null;
    }
}