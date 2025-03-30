
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    
    public bool isOpen;

    [Header("Panels")]
    [SerializeField] public GameObject inventoryPanel;
    [SerializeField] private GameObject craftingPanel;
    [SerializeField] private GameObject chestPanel;
    [SerializeField] private GameObject furnacePanel;
    [Header("infos")]
    [SerializeField] public GameObject crosshairs;
    [SerializeField] private GameObject infoHolder;
    
    private int currentSlotIndex = 0;  // Aktualnie wybrany slot
    private float accumulatedScroll;   // Akumulowana wartość scrolla
    
    public bool canChangeSlots = true;
    [Header("Slots")]
    public List<Slot> itemSlots = new List<Slot>();
    public List<QuickSlot> quickSlots = new List<QuickSlot>();
    public List<Slot> chestSlots = new List<Slot>();
    [Space]
    public Chest currentChest;
    
    [Header("Furnace")]
    [SerializeField]
    public Slot inputSlot;
    [SerializeField] public Slot fuelSlot;
    [SerializeField] public Slot outputSlot;
    public Furnace currentFurnace;
    
    [Header("Furnace UI")]
    [SerializeField] private Image smeltingProgressImage;
    [SerializeField] private Image fuelProgressImage;

    

    public RectTransform panelRectTransform;

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
    
    public void OpenFurnace(Furnace furnace)
    {
        currentFurnace = furnace;
        inventoryPanel.SetActive(true);
        furnacePanel.SetActive(true);
        isOpen = true;
        crosshairs.SetActive(false);
        
        inputSlot.ForceSetItem(furnace.inputItem, furnace.inputQuantity);
        fuelSlot.ForceSetItem(furnace.fuelItem, furnace.fuelQuantity);
        outputSlot.ForceSetItem(furnace.outputItem, furnace.outputQuantity);
        
        
        panelRectTransform.anchoredPosition = new Vector2(-331.75f, 0);
        
        UpdateFurnaceUI();
        
        FirstPersonController.instance.lockCursor = false;
        FirstPersonController.instance.cameraCanMove = false;
        FirstPersonController.instance.crosshairObject.gameObject.SetActive(false);
        SelectionManager.instance.interactionText.gameObject.SetActive(false);
            
        ConstructionManager.instance.ExitConstructionMode();
    }

    public void UpdateFurnaceUI()
    {
        if (currentFurnace == null) return;

        if (inputSlot != null)
            inputSlot.ForceSetItem(currentFurnace.inputItem, currentFurnace.inputQuantity);
        if (fuelSlot != null)
            fuelSlot.ForceSetItem(currentFurnace.fuelItem, currentFurnace.fuelQuantity);
        if (outputSlot != null)
            outputSlot.ForceSetItem(currentFurnace.outputItem, currentFurnace.outputQuantity);
        
        UpdateSmeltingProgress();
        UpdateFuelProgress();
    }
    
    void UpdateSmeltingProgress()
    {
        if (currentFurnace.currentRecipe != null && currentFurnace.currentRecipe.smeltingTime > 0)
        {
            float progress = currentFurnace.currentSmeltTime / currentFurnace.currentRecipe.smeltingTime;
            smeltingProgressImage.fillAmount = Mathf.Clamp01(progress);
        }
        else
        {
            smeltingProgressImage.fillAmount = 0;
        }
    }

    void UpdateFuelProgress()
    {
        if (currentFurnace.fuelItem != null && currentFurnace.fuelItem.burnTime > 0)
        {
            float progress = currentFurnace.remainingBurnTime / currentFurnace.fuelItem.burnTime;
            fuelProgressImage.fillAmount = Mathf.Clamp01(progress);
        }
        else
        {
            fuelProgressImage.fillAmount = 0;
        }
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

    public void CloseAllTabs()
    {
        inventoryPanel.SetActive(false);
        craftingPanel.SetActive(false);
        chestPanel.SetActive(false);
        furnacePanel.SetActive(false);
        isOpen = false;
            
        FirstPersonController.instance.lockCursor = true;
        FirstPersonController.instance.cameraCanMove = true;
        FirstPersonController.instance.crosshairObject.gameObject.SetActive(true);
        SelectionManager.instance.interactionText.gameObject.SetActive(true);
        TooltipManager.instance.HideTooltip();
        
        panelRectTransform.anchoredPosition = new Vector2(0, 0);
        
        //chest stuff
        SaveChestData();
                
        foreach (Slot slot in chestSlots)
        {
            slot.ClearSlot();
        }
        
        if (currentFurnace != null)
        {
            currentFurnace.inputItem = inputSlot.itemSO;
            currentFurnace.inputQuantity = inputSlot.quantity;
            currentFurnace.fuelItem = fuelSlot.itemSO;
            currentFurnace.fuelQuantity = fuelSlot.quantity;
            currentFurnace.outputItem = outputSlot.itemSO;
            currentFurnace.outputQuantity = outputSlot.quantity;
        }
    
        currentChest = null;
        currentFurnace = null;
        
        crosshairs.SetActive(true);
    }
    
    private void Update()
    {
        if (isOpen && currentFurnace != null)
        {
            UpdateSmeltingProgress();
            UpdateFuelProgress();
        }
        
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
        accumulatedScroll += Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(accumulatedScroll) >= 0.1f)
        {
            int steps = (int)(accumulatedScroll * 10);
            steps *= -1;
            currentSlotIndex += steps;
            currentSlotIndex = (currentSlotIndex % quickSlots.Count + quickSlots.Count) % quickSlots.Count;
            quickSlots[currentSlotIndex].SelectSlot();
            accumulatedScroll = 0f;
        }
    }
    
}