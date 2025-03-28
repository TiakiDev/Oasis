using UnityEngine;

[CreateAssetMenu (fileName = "New Item", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    [Header ("General")]
    public string itemName;
    public Sprite itemIcon;
    [TextArea (3,10)]
    public string itemDescription;
    
    public enum ItemType
    {
        Consumable,
        Axe,
        Pickaxe,
        Constructable,
        Other
    }
    
    [Header("Item Type")]
    public ItemType itemType;
    
    [Header("Consumable Settings")]
    public float hungerAmount;
    public float thirstAmount;
    public float healthAmount;
}
