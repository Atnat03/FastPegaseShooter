using UnityEngine;

public class DialogueStarterOnTriggerEnter : DialogueStarterParent
{
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerVisuelBridge>(out PlayerVisuelBridge playerVisuelBridge))
        {
            PlayDialogue();
        }
    }
    
    
}
