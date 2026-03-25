using Unity.VisualScripting;
using UnityEngine;

public class TriggerDoorDetector : MonoBehaviour
{
    public TriggerDoor p_triggerDoor;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))p_triggerDoor.DetectPlayer(other.transform.GetRootTransform().gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))p_triggerDoor.PlayerLeave(other.transform.GetRootTransform().gameObject);
    }
}
