using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameObject dragIcon;

    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image icon;
    [SerializeField] private Sprite nullImage;
    
    public ItemSO itemSO;
    public int quantity;

    private void Update()
    {
        if(quantity <= 0)
        {
            ClearSlot();
        }
    }

    public void AddItem(ItemSO item, int amount)
    {
        if (item == null)
        {
            Debug.LogError("Próba dodania null item!");
            return;
        }

        if (item.itemIcon == null)
        {
            Debug.LogError($"Item {item.itemName} nie ma przypisanej ikony!");
            return;
        }

        itemSO = item;
        quantity += amount;

        icon.sprite = item.itemIcon;
        icon.enabled = true;
        UpdateQuantityText();
    }
    
    public void RemoveItem(int amount)
    {
        quantity -= amount;
        UpdateQuantityText();
        if (quantity <= 0)
        {
            ClearSlot();
        }
    }

    private void Start()
    {
        UpdateQuantityText();
    }

    public void ClearSlot()
    {
        itemSO = null;
        quantity = 0;
        icon.sprite = nullImage;
        icon.enabled = false;
        UpdateQuantityText();
    }
    
    public void UpdateQuantityText()
    {
        if (quantity > 1)
        {
            quantityText.text = quantity.ToString();
        }
        else
        {
            quantityText.text = "";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(PointerEventData.InputButton.Left == eventData.button && Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log("działa");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    { 
        if(itemSO == null) return;
        
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(transform.root);
        dragIcon.transform.SetAsLastSibling();
        Image dragImage = dragIcon.AddComponent<Image>();
        dragImage.sprite = icon.sprite;
        dragImage.raycastTarget = false;
        
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            icon.sprite = nullImage;
            quantityText.enabled = false; 
        }
        else if (eventData.button == PointerEventData.InputButton.Right && quantity == 1)
        {
            icon.sprite = nullImage;
            quantityText.enabled = false; 
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
{
    
    if (itemSO == null) return;

    quantityText.enabled = true;
    icon.sprite = itemSO.itemIcon;

    if (dragIcon != null)
    {
        Destroy(dragIcon);
    }
    
    //Sloty w piecu
    if (InventoryManager.instance.currentFurnace != null)
    {
        if (this == InventoryManager.instance.inputSlot)
        {
            InventoryManager.instance.currentFurnace.inputItem = itemSO;
            InventoryManager.instance.currentFurnace.inputQuantity = quantity;
        }
        else if (this == InventoryManager.instance.fuelSlot)
        {
            InventoryManager.instance.currentFurnace.fuelItem = itemSO;
            InventoryManager.instance.currentFurnace.fuelQuantity = quantity;
        }
        else if (this == InventoryManager.instance.outputSlot)
        {
            InventoryManager.instance.currentFurnace.outputItem = itemSO;
            InventoryManager.instance.currentFurnace.outputQuantity = quantity;
        }
    }
    //-------------------------------------
    if (eventData.pointerEnter != null)
    {
        Transform current = eventData.pointerEnter.transform;
        while (current != null)
        {
            if (current.TryGetComponent<Slot>(out Slot itemSlot))
            {
                
                if (itemSlot == this) return; // Nie przenoś do samego siebie

                // Przenoszenie do pustego slotu
                if (itemSlot.itemSO == null)
                {
                    if (eventData.button == PointerEventData.InputButton.Left)
                    {
                        itemSlot.ForceSetItem(itemSO, quantity); // Użyj ForceSetItem zamiast AddItem
                        ClearSlot();
                    }
                    else if (eventData.button == PointerEventData.InputButton.Right)
                    {
                        if (quantity <= 1)
                        {
                            itemSlot.ForceSetItem(itemSO, quantity);
                            ClearSlot();
                        }
                        else
                        {
                            int halfAmount = quantity / 2;
                            itemSlot.ForceSetItem(itemSO, halfAmount);
                            quantity -= halfAmount;
                            UpdateQuantityText();
                        }
                    }
                    return;
                }

                // Łączenie przedmiotów tego samego typu
                if (itemSlot.itemSO == itemSO)
                {
                    itemSlot.quantity += quantity;
                    itemSlot.UpdateQuantityText();
                    ClearSlot();
                    return;
                }

                // Zamiana przedmiotów różnych typów
                if (itemSlot.itemSO != itemSO)
                {
                    SwapItems(itemSlot);
                    return;
                }
            }
            current = current.parent;
        }
    }
    icon.sprite = itemSO.itemIcon;
    UpdateQuantityText();
}
    private void SwapItems(Slot otherSlot)
    {
        if (itemSO == null || otherSlot.itemSO == null) return; 
        
        ItemSO tempItem = itemSO;
        int tempQuantity = quantity;

        ClearSlot();  
        AddItem(otherSlot.itemSO, otherSlot.quantity);  

        otherSlot.ClearSlot();  
        otherSlot.AddItem(tempItem, tempQuantity); 

        UpdateQuantityText();
    }
    
    
    //for chests
    public void ForceSetItem(ItemSO item, int amount)
    {
        itemSO = item;
        quantity = amount;
        
        if (InventoryManager.instance.currentFurnace != null)
        {
            if (this == InventoryManager.instance.inputSlot)
            {
                InventoryManager.instance.currentFurnace.inputItem = item;
                InventoryManager.instance.currentFurnace.inputQuantity = amount;
            }
            else if (this == InventoryManager.instance.fuelSlot)
            {
                InventoryManager.instance.currentFurnace.fuelItem = item;
                InventoryManager.instance.currentFurnace.fuelQuantity = amount;
            }
            else if (this == InventoryManager.instance.outputSlot)
            {
                InventoryManager.instance.currentFurnace.outputItem = item;
                InventoryManager.instance.currentFurnace.outputQuantity = amount;
            }
        }
    
        if (item != null)
        {
            icon.sprite = item.itemIcon;
            icon.enabled = true;
        }
        else
        {
            icon.sprite = nullImage;
            icon.enabled = false;
        }
    
        UpdateQuantityText();
    }
}