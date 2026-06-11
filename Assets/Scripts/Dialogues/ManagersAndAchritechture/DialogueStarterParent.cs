using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class DialogueStarterParent : NetworkBusListener
{
    [SerializeField][Tooltip("si ca doit etre lié aux directions cardinales, il faut les mettre dans l'ordre nord, sud, est, ouest")] private DialoguesDataSO[] Dialogue;
    [SerializeField] protected float delay;

    [ContextMenu("Start dialogue")]
    protected void PlayDialogue() => StartCoroutine(Delay());
    
    protected void PlayDialogue(int i) => StartCoroutine(Delay(i));
    
    protected void PlayDialogue(cardinalDirection idx) => StartCoroutine(Delay(idx));

    IEnumerator Delay()
    {
        if (delay == 0) yield return new WaitForEndOfFrame();
        else yield return new WaitForSeconds(delay);

        InvokeEvent<OnDialogueStart>(new OnDialogueStart { dialogueData = Dialogue[Random.Range(0, Dialogue.Length)] });
    }
    IEnumerator Delay(cardinalDirection idx)
    {
        if (delay == 0) yield return new WaitForEndOfFrame();
        else yield return new WaitForSeconds(delay);

        InvokeEvent<OnDialogueStart>(new OnDialogueStart { dialogueData = Dialogue[(int)idx] });
    }
    
    IEnumerator Delay(int idx)
    {
        if (delay == 0) yield return new WaitForEndOfFrame();
        else yield return new WaitForSeconds(delay);

        InvokeEvent<OnDialogueStart>(new OnDialogueStart { dialogueData = Dialogue[idx] });
    }
}