using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager instance;
    [Header("Refrences")]
    public TMP_Text interactionText;
    public GameObject infoHolder;
    //cursor variables
    [Header("Cursors")]
    public GameObject handCursor;
    public GameObject axeCursor;
    public GameObject pickaxeCursor;
    public GameObject interactionCursor;
    //targets variables
    [Space(2)]
    public InteractableObject currentTarget;
    public bool onTarget;
    //resource variables
    public GameObject selectedTree;
    public GameObject selectedOre;
    public GameObject selectedCrate;
    
    public static Vector3 lastHitPoint;
    public static Vector3 lastHitNormal;
    
    public GameObject playerObject;
    

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
        interactionText.alpha = 0;
    }
    
private void Update()
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    
    RaycastHit hit;
    
    currentTarget = null;
    bool anyCursorActive = false; // Flaga śledząca aktywność kursora

    if (Physics.Raycast(ray, out hit))
    {
        lastHitPoint = hit.point;
        lastHitNormal = hit.normal;
        
        var selectionTransform = hit.transform;

        InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();
        ChoppableTree choppableTree = selectionTransform.GetComponentInParent<ChoppableTree>();
        MineableOre mineableOre = selectionTransform.GetComponentInParent<MineableOre>();
        Crate crate = selectionTransform.GetComponent<Crate>();
        Item item = selectionTransform.GetComponent<Item>();
        Chest chest = selectionTransform.GetComponent<Chest>();
        Workbench workbench = selectionTransform.GetComponent<Workbench>();
        Furnace furnace = hit.transform.GetComponent<Furnace>();

        // Obsługa drzew
        if (choppableTree && choppableTree.playerInRange && !InventoryManager.instance.isOpen)
        {
            choppableTree.canBeChopped = true;
            selectedTree = choppableTree.gameObject;
            infoHolder.gameObject.SetActive(true);
            infoHolder.GetComponentInChildren<TMP_Text>().text = choppableTree.GetName();
            axeCursor.SetActive(true);
            anyCursorActive = true; // Aktywny kursor siekiery
            
            GlobalState.instance.resourceHealth = choppableTree.treeHealth;
            GlobalState.instance.resourceMaxHealth = choppableTree.treeMaxHealth;
        }
        else
        {
            if (selectedTree != null)
            {
                selectedTree.gameObject.GetComponent<ChoppableTree>().canBeChopped = false;
                selectedTree = null;
                infoHolder.gameObject.SetActive(false);
                axeCursor.SetActive(false);
            }
        }
        
        //obsługa mineralów
        if (mineableOre && mineableOre.playerInRange && !mineableOre.hasBeenMined)
        {
            mineableOre.canBeMined = true;
            selectedOre = mineableOre.gameObject;
            infoHolder.gameObject.SetActive(true);
            infoHolder.GetComponentInChildren<TMP_Text>().text = mineableOre.GetName();
            pickaxeCursor.SetActive(true);
            anyCursorActive = true; // Aktywny kursor kilofa
            
            GlobalState.instance.resourceHealth = mineableOre.oreHealth;
            GlobalState.instance.resourceMaxHealth = mineableOre.oreMaxHealth;
        }
        else
        {
            if (selectedOre != null) // This should be selectedOre
            {
                selectedOre.gameObject.GetComponent<MineableOre>().canBeMined = false;
                selectedOre = null;
                infoHolder.gameObject.SetActive(false);
                pickaxeCursor.SetActive(false);
            }
        }
        
        //obsługa crate'ów
        if (crate && crate.playerInRange)
        {
            selectedCrate = crate.gameObject;
            infoHolder.gameObject.SetActive(true);
            infoHolder.GetComponentInChildren<TMP_Text>().text = crate.GetName();
            axeCursor.SetActive(true);
            anyCursorActive = true; // Aktywny kursor kilofa
            
            GlobalState.instance.resourceHealth = crate.health;
            GlobalState.instance.resourceMaxHealth = crate.maxHealth;
        }
        else
        {
            if (selectedCrate != null) 
            {
                selectedCrate = null;
                infoHolder.gameObject.SetActive(false);
                axeCursor.SetActive(false);
            }
        }

        // Obsługa przedmiotów
        if (item && interactable.playerInRange && onTarget && !InventoryManager.instance.isOpen)
        {
            handCursor.SetActive(true);
            anyCursorActive = true; 
        }
        else
        {
            handCursor.SetActive(false);
        }

        // Aktualizacja tekstu interakcji
        if (interactable && interactable.playerInRange)
        {
            onTarget = true;
            currentTarget = interactable;
            interactionText.text = interactable.GetObjectName();
            interactionText.alpha = 1;
        }
        else
        {
            onTarget = false;
            interactionText.alpha = 0;
        }
        
        //obsułga skrzyń i workbecnha
        if ((chest != null || workbench != null || furnace != null) && interactable.playerInRange && onTarget && !InventoryManager.instance.isOpen)
        {
            interactionCursor.SetActive(true);
            anyCursorActive = true;
            
            if (Input.GetKeyDown(KeyCode.Mouse0) && currentTarget != null && currentTarget.playerInRange)
            {
                if (chest != null)
                {
                    chest.Interact();
                }
                if (workbench != null)
                {
                    workbench.Interact();
                }
                if (furnace != null)
                {
                    furnace.InteractFurnace();
                }
            }
        }
        else
        {
            interactionCursor.SetActive(false);
        }
    }
    else
    {
        onTarget = false;
        interactionText.alpha = 0;
        pickaxeCursor.SetActive(false);
        axeCursor.SetActive(false);
        handCursor.SetActive(false);
        interactionCursor.SetActive(false);
        infoHolder.gameObject.SetActive(false); 
    }

    // Ustaw widoczność celownika na podstawie aktywnych kursorów
    FirstPersonController.instance.crosshairObject.gameObject.SetActive(!anyCursorActive);
}
}
