using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class DialogueStarterParent : NetworkBusListener
{ 
    [SerializeField]private DialoguesDataSO Dialogue;
    [SerializeField]protected float delay;

    
    [ContextMenu("Start dialogue")]
    protected void PlayDialogue()
    {
        StartCoroutine(Delay());
    }
    
    IEnumerator Delay()
    {
        if(delay == 0)yield return new WaitForEndOfFrame();
        else yield return new WaitForSeconds(delay);

        InvokeEvent<OnDialogueStart>(new OnDialogueStart { dialogueData = Dialogue });
    }
    
    
}