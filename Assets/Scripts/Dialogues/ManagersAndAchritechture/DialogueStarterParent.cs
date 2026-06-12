using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class DialogueStarterParent : MonoBusListener
{
    [SerializeField]
    [Tooltip("si ca doit etre lié aux directions cardinales, il faut les mettre dans l'ordre nord, sud, est, ouest")]
    private DialoguesDataSO[] Dialogue;

    [SerializeField] protected float delay;

    [SerializeField] [Tooltip("si false, joue les lignes une par une avant de revenir au debut")] bool RandomLine;
    [SerializeField] [Tooltip("une fois le dialogue lancé, il faut attendre une certaine durée avant de pouvoir le jouer de nouveau")] bool HaveCooldown;
    [SerializeField] float cooldownDuration;
    [SerializeField] bool playOnce = false;


    private int currentLineIdx = 0;
    private bool canBePlayed = true;
    private bool alreadyPlayed = false;

    [ContextMenu("Start dialogue")]
    protected void PlayDialogue()
    {
        if (canBePlayed && !(playOnce && alreadyPlayed)) StartCoroutine(Delay());
    }
    
    protected void PlayDialogue(int idx)
    { 
        if (canBePlayed && !(playOnce && alreadyPlayed))StartCoroutine(Delay(idx));
    }

    protected void PlayDialogue(cardinalDirection idx)
    {
        if (canBePlayed && !(playOnce && alreadyPlayed))StartCoroutine(Delay(idx));
    } 

    IEnumerator Delay()
    {
        
        if (delay == 0) yield return new WaitForEndOfFrame();
        else yield return new WaitForSeconds(delay);

        if (RandomLine)
            InvokeEvent<OnDialogueStart>(new OnDialogueStart { dialogueData = Dialogue[Random.Range(0, Dialogue.Length)] });
        else
        {
            InvokeEvent<OnDialogueStart>(new OnDialogueStart { dialogueData = Dialogue[currentLineIdx]});
            currentLineIdx = currentLineIdx + 1 > Dialogue.Length - 1 ? 0 : currentLineIdx++;
        }
        
        if (HaveCooldown)StartCoroutine(Cooldown());
        alreadyPlayed = true;
    }
    IEnumerator Delay(int idx)
    {
        if (delay == 0) yield return new WaitForEndOfFrame();
        else yield return new WaitForSeconds(delay);

        InvokeEvent<OnDialogueStart>(new OnDialogueStart { dialogueData = Dialogue[idx] });
        if (HaveCooldown)StartCoroutine(Cooldown());
        alreadyPlayed = true;
    }

    IEnumerator Delay(cardinalDirection idx)
    {
        if (delay == 0) yield return new WaitForEndOfFrame();
        else yield return new WaitForSeconds(delay);

        InvokeEvent<OnDialogueStart>(new OnDialogueStart { dialogueData = Dialogue[(int)idx] });
        if (HaveCooldown)StartCoroutine(Cooldown());
        alreadyPlayed = true;
    }

    IEnumerator Cooldown()
    {
        canBePlayed = false;
        yield return new WaitForSeconds(cooldownDuration);
        canBePlayed = true;
    }

}