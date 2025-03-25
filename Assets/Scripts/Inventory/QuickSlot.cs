using UnityEngine;
using UnityEngine.Rendering;

public class QuickSlot : MonoBehaviour
{
    private Slot slot;
    public GameObject selectedShader;
    public bool isSelected;
    public GameObject toolHolder;
    public GameObject itemModel;
    private bool isEquipped;

    private void Start()
    {
        slot = GetComponent<Slot>();
        selectedShader.SetActive(false);
        isEquipped = false;
    }
    
    private void Update()
    {
        if (isSelected && (slot.itemSO == null || slot.quantity <= 0))
        {
            UnequipItem();
            isEquipped = false;
        }

        if (isSelected && slot.itemSO != null && !isEquipped)
        {
            SetEquippedItem(slot.itemSO);
            isEquipped = true;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) 
            && isSelected 
            && slot.itemSO != null
            && !InventoryManager.instance.isOpen 
            && !SelectionManager.instance.onTarget 
            && GlobalState.instance.canUse)
        {
            if (slot.itemSO.itemType == ItemSO.ItemType.Consumable)
            {
                ConsumableManager.instance.UseConsumable(slot);
            }
            else if (slot.itemSO.itemType == ItemSO.ItemType.Axe || slot.itemSO.itemType == ItemSO.ItemType.Pickaxe )
            {
                ToolManager.instance.UseTool(slot);
            }
        }
    }
    

    public void SelectSlot()
    {
        InventoryManager.instance.DeselectAllSlots();
        selectedShader.SetActive(true);
        isSelected = true;
        if (slot.itemSO != null)
        {
            SetEquippedItem(slot.itemSO);
        }
    }
    
    public void UnequipItem()
    {
        if (itemModel != null)
        {
            Destroy(itemModel);
            itemModel = null;
        }
    }
    
    private void SetEquippedItem(ItemSO item)
    {
        GameObject modelPrefab;
        
        if (item.itemType == ItemSO.ItemType.Constructable)
        {
            modelPrefab = Resources.Load<GameObject>("EquipableModels/Empty_Model");
        }
        else
        {
            modelPrefab = Resources.Load<GameObject>("EquipableModels/" + item.itemName + "_Model");
        }
        
        if (modelPrefab != null)
        {
            itemModel = Instantiate(
                modelPrefab,
                toolHolder.transform.position, // pozycja
                Quaternion.identity, // rotacja
                toolHolder.transform // rodzic
            );
            
            itemModel.transform.localPosition = new Vector3(0.070f, 0.240f, 0.010f);
            itemModel.transform.localRotation = Quaternion.Euler(-40f, -105, -8f);
            itemModel.gameObject.name = item.itemName;
            
            itemModel.layer = LayerMask.NameToLayer("Tool");
            SetLayerRecursively(itemModel, LayerMask.NameToLayer("Tool"));
            
            var modelRenderer = itemModel.GetComponent<MeshRenderer>();
            
            if (modelRenderer == null)
            {
                modelRenderer = itemModel.GetComponentInChildren<MeshRenderer>();
            }
            if (modelRenderer != null)
            {
                modelRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }

            isEquipped = true;
        }
        else
        {
            Debug.LogError("Model nie został znaleziony: " + item.itemName);
        }

        if (item.itemType == ItemSO.ItemType.Constructable)
        {
            switch (item.itemName)
            {
                case "Foundation":
                    ConstructionManager.instance.ActivateConstructionPlacement("FoundationModel");
                    break;
                case "Wall":
                    ConstructionManager.instance.ActivateConstructionPlacement("WallModel");
                    break;
                case "Chest":
                    ConstructionManager.instance.ActivateConstructionPlacement("ChestModel");
                    break;
                case "Workbench tier 3":
                    ConstructionManager.instance.ActivateConstructionPlacement("Workbench3Model");
                    break;
            }
        }
    }
    
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
