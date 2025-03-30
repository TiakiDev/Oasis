using UnityEngine;

[CreateAssetMenu (fileName = "New Item", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    [Header ("General")]
    public string itemName;
    public Sprite itemIcon;
    public int maxStackSize = 64;
    [TextArea (3,10)]
    public string itemDescription;
    
    public enum ItemType
    {
        Consumable,
        Axe,
        Pickaxe,
        Constructable,
        Fuel,
        Other
    }
    
    [Header("Item Type")]
    public ItemType itemType;
    
    [Header("Consumable Settings")]
    public float hungerAmount;
    public float thirstAmount;
    public float healthAmount;
    
    [Header("Fuel Settings")]
    public float burnTime; // czas palenia w sekundach (np. 80s dla węgla)
}
