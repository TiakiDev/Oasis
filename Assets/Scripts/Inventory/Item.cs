using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
public class Item : MonoBehaviour
{
    public ItemSO itemSO;
    public int quantity = 1;
    public bool collision = true;

    private InteractableObject interactable;

    private void Awake()
    {
        interactable = GetComponent<InteractableObject>();
    }

    private void Start()
    {
        if(!collision)
        {
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                if (!col.isTrigger)
                {
                    Physics.IgnoreCollision(col,SelectionManager.instance.playerObject.GetComponent<Collider>());
                }
            }
        }
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) && interactable.playerInRange && SelectionManager.instance.currentTarget == interactable)
        {
            InventoryManager.instance.AddItem(itemSO, quantity);
            Destroy(gameObject);
        }
    }
}
