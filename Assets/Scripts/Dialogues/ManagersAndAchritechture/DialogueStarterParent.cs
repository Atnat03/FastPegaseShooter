using FishNet.Object;
using UnityEngine;

public class DialogueStarterParent : NetworkBusListener
{ 
    [SerializeField]private DialoguesDataSO Dialogue;
    
    protected void PlayDialogue()
    {
        InvokeEvent<OnDialogueStart>(new OnDialogueStart { dialogueData = Dialogue });
    }
}
