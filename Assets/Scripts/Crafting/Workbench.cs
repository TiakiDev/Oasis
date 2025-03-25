using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workbench : MonoBehaviour
{
    public int tier = 1;
    [SerializeField] private float interactionRange = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CraftingManager.instance.AddActiveWorkbench(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CraftingManager.instance.RemoveActiveWorkbench(this);
        }
    }

    public void Interact()
    {
        InventoryManager.instance.OpenCraftingMenu();
    }
}
