using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Crate : MonoBehaviour
{
    public bool playerInRange;

    public float maxHealth;
    public float health;

    public int itemsAmount;
    
    public GameObject[] itemPrefabs;
    public string crateName;
    
    private void Start()
    {
        health = maxHealth;
    }
    
    private void LateUpdate()
    {
            GlobalState.instance.resourceHealth = health;
            GlobalState.instance.resourceMaxHealth = maxHealth;
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
    
    public void GetHit()
    {
        StartCoroutine(Hit());
    }
    
    private void CrateIsOpened()
    {

        SelectionManager.instance.selectedCrate = null;
        SelectionManager.instance.infoHolder.gameObject.SetActive(false);
        SelectionManager.instance.axeCursor.SetActive(false);
        
        
        for (int i = 0; i < itemsAmount; i++)
        {
            if (itemPrefabs.Length == 0) return;

            int randomIndex = Random.Range(0, itemPrefabs.Length);
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
            Instantiate(itemPrefabs[randomIndex], transform.position + offset, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }

    private IEnumerator Hit()
    {
        yield return new WaitForSeconds(0f);
        health -= 1f;

        if (health <= 0)
        {
            CrateIsOpened();
        }
        
    }
    
    public string GetName()
    {
        return crateName;
    }
}
