using UnityEngine;

public class DialogueStarterOnTriggerEnter : DialogueStarterParent
{
    void OnTriggerEnter(Collider other)
    {
        if (TryGetComponent<PlayerVisuelBridge>(out PlayerVisuelBridge playerVisuelBridge))
        {
            PlayDialogue();
        }
    }
}
