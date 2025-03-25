using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public bool isOpen;

    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject craftingPanel;
    [SerializeField] private GameObject chestPanel;

    [SerializeField] private GameObject crosshairs;
    [SerializeField] private GameObject infoHolder;
    
    private int currentSlotIndex = 0;  // Aktualnie wybrany slot
    private float accumulatedScroll;   // Akumulowana wartość scrolla
    
    public bool canChangeSlots = true;
    
    public List<Slot> itemSlots = new List<Slot>();
    public List<QuickSlot> quickSlots = new List<QuickSlot>();
    
    //chest
    public List<Slot> chestSlots = new List<Slot>();
    public Chest currentChest;
    
    private RectTransform panelRectTransform;

    public void AddItem(ItemSO item, int amount = 1)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].itemSO == item)
            {
                itemSlots[i].AddItem(item, amount);
                return;
            }
        }
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].itemSO == null)
            {
                itemSlots[i].AddItem(item, amount);
                return;
            }
        }
    }

    public void DeselectAllSlots()
    {
        for (int i = 0; i < quickSlots.Count; i++)
        {
            quickSlots[i].isSelected = false;
            quickSlots[i].selectedShader.SetActive(false);
            quickSlots[i].UnequipItem();
        }
        
        ConstructionManager.instance.ExitConstructionMode();
    }
    private void Awake()
    {
        if(instance != null && instance != this)
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
        inventoryPanel.SetActive(false);
        craftingPanel.SetActive(false);
        chestPanel.SetActive(false);

        panelRectTransform = inventoryPanel.GetComponent<RectTransform>();

        panelRectTransform.anchoredPosition = new Vector2(0, 0);
    }
    
    public bool HasEnoughItems(ItemSO item, int requiredAmount)
    {
        int total = 0;
        foreach (Slot slot in itemSlots)
        {
            if (slot.itemSO == item)
            {
                total += slot.quantity;
                if (total >= requiredAmount) return true;
            }
        }
        return false;
    }
    
    public void RemoveItem(ItemSO item, int amount)
    {
        int remaining = amount;
        foreach (Slot slot in itemSlots)
        {
            if (slot.itemSO == item)
            {
                int remove = Mathf.Min(remaining, slot.quantity);
                slot.quantity -= remove;
                remaining -= remove;
                slot.UpdateQuantityText();

                if (slot.quantity <= 0)
                {
                    slot.ClearSlot();
                }

                if (remaining <= 0) break;
            }
        }
    }
    
    public int GetItemQuantity(ItemSO item)
    {
        int total = 0;
        foreach (Slot slot in itemSlots)
        {
            if (slot.itemSO == item)
            {
                total += slot.quantity;
            }
        }
        return total;
    }
    
    public void OpenChest(Chest chest)
    {
        if (currentChest != null) return;
    
        currentChest = chest;
        inventoryPanel.SetActive(true);
        chestPanel.SetActive(true);
        isOpen = true;
        crosshairs.SetActive(false);
        
        //jak chest nie ma slotów to je dodaje
        while(currentChest.slots.Count < chestSlots.Count)
        {
            currentChest.slots.Add(new ChestSlot());
        }
        
        panelRectTransform.anchoredPosition = new Vector2(-331.75f, 0);
    
        RefreshChestUI();
        
        FirstPersonController.instance.lockCursor = false;
        FirstPersonController.instance.cameraCanMove = false;
        FirstPersonController.instance.crosshairObject.gameObject.SetActive(false);
        SelectionManager.instance.interactionText.gameObject.SetActive(false);
            
        ConstructionManager.instance.ExitConstructionMode();
    }
    
    public void RefreshChestUI()
    {
        for (int i = 0; i < chestSlots.Count; i++)
        {
            if (i < currentChest.slots.Count && currentChest.slots[i].itemSO != null)
            {
                chestSlots[i].ForceSetItem(
                    currentChest.slots[i].itemSO,
                    currentChest.slots[i].quantity
                );
            }
            else
            {
                chestSlots[i].ForceSetItem(null, 0);
            }
        }
    }
    
    public void SaveChestData()
    {
        if (currentChest != null)
        {
            currentChest.slots.Clear();
        
            foreach (Slot slot in chestSlots)
            {
                if (slot.itemSO != null && slot.itemSO.itemIcon != null)
                {
                    currentChest.slots.Add(new ChestSlot {
                        itemSO = slot.itemSO,
                        quantity = slot.quantity
                    });
                }
                else
                {
                    currentChest.slots.Add(new ChestSlot());
                }
            }
        }
    }

    private void CloseAllTabs()
    {
        inventoryPanel.SetActive(false);
        craftingPanel.SetActive(false);
        chestPanel.SetActive(false);
        isOpen = false;
            
        FirstPersonController.instance.lockCursor = true;
        FirstPersonController.instance.cameraCanMove = true;
        FirstPersonController.instance.crosshairObject.gameObject.SetActive(true);
        SelectionManager.instance.interactionText.gameObject.SetActive(true);
        TooltipManager.instance.HideTooltip();
        
        panelRectTransform.anchoredPosition = new Vector2(0, 0);
        
        //chest stuff
        chestPanel.SetActive(false);
        chestPanel.SetActive(false);
        SaveChestData();
                
        foreach (Slot slot in chestSlots)
        {
            slot.ClearSlot();
        }
    
        currentChest = null;
        crosshairs.SetActive(true);
    }
    
    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Tab) && isOpen)
        {
            CloseAllTabs();
        }
        else if (Input.GetKeyDown(KeyCode.Tab) && !isOpen)
        {
            inventoryPanel.SetActive(true);
            isOpen = true;
            FirstPersonController.instance.lockCursor = false;
            FirstPersonController.instance.cameraCanMove = false;
            FirstPersonController.instance.crosshairObject.gameObject.SetActive(false);
            SelectionManager.instance.interactionText.gameObject.SetActive(false);
            
            ConstructionManager.instance.ExitConstructionMode();
            
            crosshairs.SetActive(false);
        }

        if (canChangeSlots)
        {
            SlotChangingHandler();
        }
        for (int i = 0; i < quickSlots.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                currentSlotIndex = i;
                quickSlots[i].SelectSlot();
                break;
            }
        }
        
    }

    public void OpenCraftingMenu()
    {
        CloseAllTabs();
        
        craftingPanel.SetActive(true);
        isOpen = true;
        FirstPersonController.instance.lockCursor = false;
        FirstPersonController.instance.cameraCanMove = false;
        FirstPersonController.instance.crosshairObject.gameObject.SetActive(false);
        SelectionManager.instance.interactionText.gameObject.SetActive(false);
            
        ConstructionManager.instance.ExitConstructionMode();
            
        crosshairs.SetActive(false);
        
    }
    
    private void SlotChangingHandler()
    {

        // Obsługa scrolla
        accumulatedScroll += Input.GetAxis("Mouse ScrollWheel");
        
        // Sprawdź czy scroll wystarczająco się przemieścił (0.1 = 1 "kliknięcie" scrolla)
        if (Mathf.Abs(accumulatedScroll) >= 0.1f)
        {
            int steps = (int)(accumulatedScroll * 10); // Każdy "klik" to 0.1, mnożymy przez 10 by dostać liczbę kroków
            steps *= -1;
            currentSlotIndex += steps;

            // Zawijanie indeksu (np. od 0 do 5 dla 6 slotów)
            currentSlotIndex = (currentSlotIndex % quickSlots.Count + quickSlots.Count) % quickSlots.Count;

            quickSlots[currentSlotIndex].SelectSlot();
            accumulatedScroll = 0f; // Reset akumulacji
        }
    }
    
}
