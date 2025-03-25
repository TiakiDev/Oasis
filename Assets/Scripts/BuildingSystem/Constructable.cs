using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Constructable : MonoBehaviour
{
    [Header("Settings")]
    public bool canSnapToStructures = false;

    public bool isProp;

    public bool needFoundation;
    
    [Header("Rotation Settings")]
    public bool allowRotation = true;
    
    [Space]
    
    [Header("Other")]
    // Validation
    public bool isGrounded;
    public bool isOverlappingItems;
    public bool isValidToBeBuilt;
    public bool detectedGhostMemeber;

    // Ground check
    public float maxGroundDistance = 1.5f;
    public float surfaceOffset = 0.1f;
    public LayerMask groundLayer;


    // Material related
    private Renderer mRenderer;
    public Material redMaterial;
    public Material greenMaterial;
    public Material defaultMaterial;

    public List<GameObject> ghostList = new List<GameObject>();
    public BoxCollider solidCollider;

    private void Start()
    {
        mRenderer = GetComponentInChildren<Renderer>();
        if(mRenderer == null) Debug.LogError("Renderer not found", this);
        mRenderer.material = defaultMaterial;
        
        foreach(Transform child in transform)
        {
            ghostList.Add(child.gameObject);
        }
    }

    void Update()
    {
        bool isNearSurface = CheckSurfaceProximity();
        
        if(isGrounded && !isOverlappingItems && isNearSurface)
        {
            isValidToBeBuilt = true;
        }
        else
        {
            isValidToBeBuilt = false;
        }
    }

    private bool CheckSurfaceProximity()
    {
        LayerMask checkLayers = groundLayer;
        if(canSnapToStructures)
        {
            checkLayers |= LayerMask.GetMask("Structure");
        }

        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
        
        if(Physics.Raycast(ray, out RaycastHit hit, maxGroundDistance, checkLayers))
        {
            // Uwzględnij ręczną rotację
            Quaternion manualRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * manualRotation;
            return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gameObject.CompareTag("activeConstructable"))
        {
            if (other.CompareTag("Tree") || 
                other.CompareTag("pickable") || 
                other.CompareTag("Structure") || 
                other.CompareTag("Rock"))
            {
                isOverlappingItems = true;
            }

            if (other.CompareTag("Ground"))
            {
                isGrounded = true;
            }

            if (other.CompareTag("ghost"))
            {
                detectedGhostMemeber = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (gameObject.CompareTag("activeConstructable"))
        {
            if (other.CompareTag("Tree") || 
                other.CompareTag("pickable") || 
                other.CompareTag("Structure") ||
                other.CompareTag("Rock"))
            {
                isOverlappingItems = false;
            }

            if (other.CompareTag("Ground"))
            {
                isGrounded = false;
            }

            if (other.CompareTag("ghost"))
            {
                detectedGhostMemeber = false;
            }
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (gameObject.CompareTag("activeConstructable"))
        {
            if (other.CompareTag("Tree") || 
                other.CompareTag("pickable") || 
                other.CompareTag("Structure")|| 
                other.CompareTag("Rock"))
            {
                isOverlappingItems = true;
            }
        }
    }
 
    public void SetInvalidColor()
    {
        if(mRenderer != null)
        {
            mRenderer.material = redMaterial;
        }
    }
 
    public void SetValidColor()
    {
        mRenderer.material = greenMaterial;
    }
 
    public void SetDefaultColor()
    {
        mRenderer.material = defaultMaterial;
    }
 
    public void ExtractGhostMembers()
    {
        foreach (GameObject item in ghostList)
        {
            item.transform.SetParent(transform.parent, true);
            item.gameObject.GetComponent<GhostItem>().isPlaced = true;
        }
    }
}