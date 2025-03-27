using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
 
public class ConstructionManager : MonoBehaviour
{
    public static ConstructionManager instance;
 
    [Header("Settings")]
    public float placementOffset = 0.1f;
    public LayerMask groundLayer;
    public LayerMask structureLayer;
    
    public float rotationSpeed = 100f;
    private float currentRotation = 0f;
    
    public GameObject itemToBeConstructed;
    public bool inConstructionMode = false;
    public GameObject constructionHoldingSpot;
 
    public bool isValidPlacement;
 
    public bool selectingAGhost;
    public GameObject selectedGhost;
    
    [Header("Materials")]
    public Material ghostSelectedMat;
    public Material ghostFullTransparentMat;

    public List<GameObject> allGhostsInExistence;
 
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
 
    public void ActivateConstructionPlacement(string itemToConstruct)
    {
        currentRotation = 0f;
        
        GameObject item = Instantiate(Resources.Load<GameObject>(itemToConstruct));
        
        item.name = itemToConstruct;
 
        item.transform.SetParent(constructionHoldingSpot.transform, false);
        itemToBeConstructed = item;
        itemToBeConstructed.gameObject.tag = "activeConstructable";
        
        itemToBeConstructed.GetComponent<Constructable>().solidCollider.enabled = false;
        
        inConstructionMode = true;
    }

 
private void Update()
{
    if (inConstructionMode)
    {
        InventoryManager.instance.canChangeSlots = false;
    }
    else
    {
        InventoryManager.instance.canChangeSlots = true;
    }
    

    if (itemToBeConstructed != null && inConstructionMode)
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            RotateConstruction(scroll);
        }
        
        if (!selectingAGhost && itemToBeConstructed.name == "FoundationModel" || itemToBeConstructed.GetComponent<Constructable>().isProp)
        {
            SnapToSurface();
        }


        isValidPlacement = CheckValidConstructionPosition();


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var selectionTransform = hit.transform;
            if (selectionTransform.gameObject.CompareTag("ghost") && itemToBeConstructed.name == "FoundationModel")
            {
                itemToBeConstructed.SetActive(false);
                selectingAGhost = true;
                selectedGhost = selectionTransform.gameObject;
            }
            else if (selectionTransform.gameObject.CompareTag("wallGhost") && itemToBeConstructed.name == "WallModel")
            {
                itemToBeConstructed.SetActive(false);
                selectingAGhost = true;
                selectedGhost = selectionTransform.gameObject;
            }
            else
            {
                itemToBeConstructed.SetActive(true);
                selectedGhost = null;
                selectingAGhost = false;
            }
        }
    }
    
    
    if (Input.GetMouseButtonDown(0) && inConstructionMode && itemToBeConstructed != null)
    {
        if (isValidPlacement && selectedGhost == null && itemToBeConstructed.name == "FoundationModel" || itemToBeConstructed.GetComponent<Constructable>().isProp && itemToBeConstructed.GetComponent<Constructable>().isValidToBeBuilt)
        {
            PlaceItemFreeStyle();
        }

        if (selectingAGhost)
        {
            PlaceItemInGhostPosition(selectedGhost);
        }
    }
    if (Input.GetMouseButtonDown(1))
    {
        ExitConstructionMode();
    }
}

    public void ExitConstructionMode()
    {
        if (itemToBeConstructed != null)
        {
            Destroy(itemToBeConstructed);
            itemToBeConstructed = null;
        }
        inConstructionMode = false;
        selectedGhost = null;
        selectingAGhost = false;
    }
    
    private void RotateConstruction(float scrollInput)
    {
        currentRotation += scrollInput * rotationSpeed;
        itemToBeConstructed.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
        
        // Aktualizacja pozycji dla zachowania poprawnego snapowania
        if (itemToBeConstructed.GetComponent<Constructable>().isProp)
        {
            SnapToSurface();
        }
    }

    private void SnapToSurface()
    {
        LayerMask snapLayers = groundLayer;
        var constructable = itemToBeConstructed.GetComponent<Constructable>();
        
        if(constructable.canSnapToStructures)
        {
            snapLayers |= structureLayer;
        }

        Ray ray = new Ray(itemToBeConstructed.transform.position + Vector3.up * 2f, Vector3.down);
        
        if(Physics.Raycast(ray, out RaycastHit hit, 5f, snapLayers))
        {
            Vector3 targetPosition = hit.point + Vector3.up * constructable.surfaceOffset;
            itemToBeConstructed.transform.position = Vector3.Lerp(
                itemToBeConstructed.transform.position,
                targetPosition,
                Time.deltaTime * 15f
            );

            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * 
                                        Quaternion.Euler(0f, currentRotation, 0f);
            
            itemToBeConstructed.transform.rotation = Quaternion.Slerp(
                itemToBeConstructed.transform.rotation,
                targetRotation,
                Time.deltaTime * 10f
            );
        }
    }
    
    

    private void PlaceItemInGhostPosition(GameObject copyOfGhost)
    {
        if(itemToBeConstructed == null) return;
    
        Constructable constructable = itemToBeConstructed.GetComponent<Constructable>();
        if(constructable == null) return;
        
        constructable.enabled = false;
        constructable.SetDefaultColor();
        constructable.solidCollider.enabled = true;

        var randomOffset = UnityEngine.Random.Range(0.01f, 0.03f);
        Vector3 ghostPosition = copyOfGhost.transform.position;
    
        selectedGhost.gameObject.SetActive(false);
        itemToBeConstructed.SetActive(true);
        itemToBeConstructed.transform.SetParent(transform.parent, true);
        itemToBeConstructed.transform.position = new Vector3(
            ghostPosition.x, 
            ghostPosition.y, 
            ghostPosition.z + randomOffset
        );
        itemToBeConstructed.transform.rotation = copyOfGhost.transform.rotation;

        constructable.GetComponent<Animator>().SetTrigger("Place");
        
        if(itemToBeConstructed.name == "FoundationModel")
        {
            constructable.ExtractGhostMembers();
            itemToBeConstructed.tag = "placedFoundation";
            GetAllGhosts(itemToBeConstructed);
            PerformGhostDeletionScan();
        }
        else
        {
            itemToBeConstructed.tag = "placedWall";
            Destroy(selectedGhost);
        }

        itemToBeConstructed = null;
        inConstructionMode = false;
        
        ProcessBuildingCost();
    }

    private void PlaceItemFreeStyle()
    {
        if(itemToBeConstructed == null) return;

        Constructable constructable = itemToBeConstructed.GetComponent<Constructable>();
        if(constructable == null) return;

        constructable.GetComponent<Animator>().SetTrigger("Place");
        
        itemToBeConstructed.transform.SetParent(transform.parent, true);
    
        constructable.ExtractGhostMembers();
        constructable.SetDefaultColor();
        constructable.enabled = false;
        constructable.solidCollider.enabled = true;

        if (itemToBeConstructed.name == "FoundationModel")
        {
            itemToBeConstructed.tag = "placedFoundation";
        }
        else
        {
            itemToBeConstructed.tag = "placedProp";
        }
        GetAllGhosts(itemToBeConstructed);
        PerformGhostDeletionScan();

        itemToBeConstructed = null;
        inConstructionMode = false;
        
        
        ProcessBuildingCost();
    }

    private void ProcessBuildingCost()
    {
        foreach (var slot in InventoryManager.instance.quickSlots)
        {
            if (slot.isSelected)
            {
                slot.GetComponent<Slot>().RemoveItem(1);
            }
        }
    }
    
 
    private bool CheckValidConstructionPosition()
    {
        if (itemToBeConstructed != null)
        {
            var constructable = itemToBeConstructed.GetComponent<Constructable>();

            // Sprawdzenie, czy obiekt nie nachodzi na inne struktury
            bool isOverlappingStructures = Physics.CheckBox(
                constructable.transform.position,
                constructable.solidCollider.size / 2,
                constructable.transform.rotation,
                LayerMask.GetMask("Structure")
            );

            bool isValid = constructable.isValidToBeBuilt && !isOverlappingStructures && !constructable.needFoundation;

            // Debugging logs
            Debug.Log($"Constructable: {itemToBeConstructed.name} | isValidToBeBuilt: {constructable.isValidToBeBuilt} | isOverlappingStructures: {isOverlappingStructures} | isValid: {isValid}");

            // Ustawienie koloru w zależności od wyniku
            if (isValid)
            {
                constructable.SetValidColor(); // Zielony kolor
            }
            else if(constructable.isValidToBeBuilt && constructable.isProp)
            {
                constructable.SetValidColor(); // Zielony kolor
            }
            else
            {
                constructable.SetInvalidColor(); // Czerwony kolor
            }

            return isValid;
        }
        return false;
    }
 
    private float XPositionToAccurateFloat(GameObject ghost)
    {
        if (ghost != null)
        {
            // Turning the position to a 2 decimal rounded float
            Vector3 targetPosition = ghost.gameObject.transform.position;
            float pos = targetPosition.x;
            float xFloat = Mathf.Round(pos * 100f) / 100f;
            return xFloat;
        }
        return 0;
    }
 
    private float ZPositionToAccurateFloat(GameObject ghost)
    {
 
        if (ghost != null)
        {
            // Turning the position to a 2 decimal rounded float
            Vector3 targetPosition = ghost.gameObject.transform.position;
            float pos = targetPosition.z;
            float zFloat = Mathf.Round(pos * 100f) / 100f;
            return zFloat;
 
        }
        return 0;
    }
    
    //[GHOSTS] nie rozumiem tego XD
    private void GetAllGhosts(GameObject itemToBeConstructed)
    {
        List<GameObject> ghostlist = itemToBeConstructed.gameObject.GetComponent<Constructable>().ghostList;
 
        foreach (GameObject ghost in ghostlist)
        {
            Debug.Log(ghost);
            allGhostsInExistence.Add(ghost);
        }
    }
    private void PerformGhostDeletionScan()
    {
 
        foreach (GameObject ghost in allGhostsInExistence)
        {
            if (ghost != null)
            {
                if (ghost.GetComponent<GhostItem>().hasSamePosition == false) // if we did not already add a flag
                {
                    foreach (GameObject ghostX in allGhostsInExistence)
                    {
                        // First we check that it is not the same object
                        if (ghost.gameObject != ghostX.gameObject)
                        {
                            // If its not the same object but they have the same position
                            if (XPositionToAccurateFloat(ghost) == XPositionToAccurateFloat(ghostX) && ZPositionToAccurateFloat(ghost) == ZPositionToAccurateFloat(ghostX))
                            {
                                if (ghost != null && ghostX != null)
                                {
                                    // setting the flag
                                    ghostX.GetComponent<GhostItem>().hasSamePosition = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
 
        foreach (GameObject ghost in allGhostsInExistence)
        {
            if (ghost != null)
            {
                if (ghost.GetComponent<GhostItem>().hasSamePosition)
                {
                    DestroyImmediate(ghost);
                }
            }
 
        }
    }
    
    
}