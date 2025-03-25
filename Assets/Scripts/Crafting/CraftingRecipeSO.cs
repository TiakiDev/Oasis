using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class CraftingRecipeSO : ScriptableObject
{
    public ItemSO outputItem;
    public int outputQuantity = 1;
    public ItemRequirement[] requiredItems;
    public int requiredWorkbenchTier; // 0 - brak, 1 - podstawowy, 2 - zaawansowany

    [System.Serializable]
    public class ItemRequirement
    {
        public ItemSO item;
        public int quantity;
    }
}