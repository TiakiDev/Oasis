using UnityEngine;

public class GlobalState : MonoBehaviour
{
    public static GlobalState instance;

    public float resourceHealth;
    public float resourceMaxHealth;

    public bool canUse;
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

    private void Start()
    {
        canUse = true;
    }
}
