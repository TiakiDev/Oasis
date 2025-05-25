using System.Collections;
using UnityEngine;

public class ToolManager : MonoBehaviour
{
    public static ToolManager instance;
    public Animator toolHolderAnimator;
    public GameObject hitParticlePrefab;

    private Vector3 hitPoint;
    private Vector3 hitNormal;
    
    public float particleOffset = 0.2f;

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

    public void UseTool(Slot slot)
    {
        StartCoroutine(HittingRoutine(slot));
    }

    private IEnumerator HittingRoutine(Slot slot)
    {
        hitPoint = SelectionManager.lastHitPoint;
        hitNormal = SelectionManager.lastHitNormal;
        
        
        if (slot.itemSO.itemType != ItemSO.ItemType.Constructable)
        {
            GlobalState.instance.canUse = false;
            toolHolderAnimator.SetTrigger("Swing");
            
            yield return new WaitForSeconds(0.4f); 
            
            if (slot.itemSO.itemType == ItemSO.ItemType.Axe)
            {
                GameObject selectedTree = SelectionManager.instance.selectedTree;
                GameObject selectedCrate = SelectionManager.instance.selectedCrate;
                
                if (selectedTree != null)
                {
                    selectedTree.GetComponent<ChoppableTree>().GetHit();
                    SpawnHitParticle();
                }
                if (selectedCrate != null)
                {
                    selectedCrate.GetComponent<Crate>().GetHit();
                    SpawnHitParticle();
                }
            }

            if (slot.itemSO.itemType == ItemSO.ItemType.Pickaxe)
            {
                GameObject selectedOre = SelectionManager.instance.selectedOre;
                
                if (selectedOre != null)
                {
                    selectedOre.GetComponent<MineableOre>().GetHit();
                    SpawnHitParticle();
                }
            }

            // Poczekaj na zakończenie animacji
            yield return new WaitForSeconds(0.6f); 
            GlobalState.instance.canUse = true;
        }
    }
    
    private void SpawnHitParticle()
    {
        if (hitParticlePrefab != null)
        {
            Vector3 spawnPosition = hitPoint + hitNormal * particleOffset;
            Instantiate(
                hitParticlePrefab,
                spawnPosition,
                Quaternion.LookRotation(hitNormal)
            );
        }
    }
}