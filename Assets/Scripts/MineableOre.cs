using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MineableOre : MonoBehaviour
{
    public bool playerInRange;
    public bool canBeMined;
    public bool hasBeenMined = false;

    public float oreMaxHealth;
    public float oreHealth;
    
    public string oreName;

    public bool isStone;

    public GameObject minedOrePrefab;
    
    

    private void Start()
    {
        oreHealth = oreMaxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (canBeMined)
        {
            GlobalState.instance.resourceHealth = oreHealth;
            GlobalState.instance.resourceMaxHealth = oreMaxHealth;
        }
    }

    public void GetHit()
    {
        StartCoroutine(Hit());
    }

    private void OreIsMined()
    {
        Vector3 orePosition = transform.position;

        hasBeenMined = true;
        if (isStone)
        {
            Destroy(gameObject);
        }
        else
        {
            Destroy(transform.GetChild(0).gameObject);
        }
        canBeMined = false;
        SelectionManager.instance.selectedOre = null;
        SelectionManager.instance.infoHolder.gameObject.SetActive(false);
        SelectionManager.instance.pickaxeCursor.SetActive(false);
        
        //GameObject brokenOre = Instantiate(Resources.Load<GameObject>("Prefabs/MinedOre"), orePosition, Quaternion.identity);
        GameObject brokenOre = Instantiate(minedOrePrefab, orePosition, Quaternion.identity);
    }

    private IEnumerator Hit()
    {
        yield return new WaitForSeconds(0f);
        oreHealth -= 1f;

        if (oreHealth <= 0)
        {
            OreIsMined();
        }
        
    }
    
    public string GetName()
    {
        return oreName;
    }
}
