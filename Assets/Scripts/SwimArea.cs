using UnityEngine;

public class SwimArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<FirstPersonController>().isSwimming = true;
            StatsManager.instance.oxygenBarGameObject.gameObject.SetActive(true);
            StatsManager.instance.shouldDrainOxygen = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<FirstPersonController>().isSwimming = false;
            StatsManager.instance.oxygenBarGameObject.gameObject.SetActive(false);
            StatsManager.instance.ChangeOxygen(100);
            StatsManager.instance.shouldDrainOxygen = false;
        }
    }
}
