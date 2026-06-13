using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class DialogueStarterParent : MonoBusListener
{
    [SerializeField]
    [Tooltip("si ca doit etre lié aux directions cardinales, il faut les mettre dans l'ordre nord, sud, est, ouest . \n" +
             "si c'est selon le joueur, il faut mettre d'abbord positif (rouge) puis negatif(bleu)")]
    private DialoguesDataSO[] Dialogue;

    [SerializeField] protected float delay;

    [SerializeField] [Tooltip("si false, joue les lignes une par une avant de revenir au debut")] bool RandomLine;
    [SerializeField] [Tooltip("une fois le dialogue lancé, il faut attendre une certaine durée avant de pouvoir le jouer de nouveau")] bool HaveCooldown;
    [SerializeField] float cooldownDuration;
    [SerializeField] bool playOnce = false;


    private int currentLineIdxSpeaker1 = 0;
    private  int currentLineIdxSpeaker2 = 1;
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

    protected void PlayDialogue(bool isPositiv)
    {
        if (canBePlayed && !(playOnce && alreadyPlayed))StartCoroutine(Delay(isPositiv));
    }

    IEnumerator Delay()
    {
        
        if (delay == 0) yield return new WaitForEndOfFrame();
        else yield return new WaitForSeconds(delay);

        if (RandomLine)
            InvokeEvent<OnDialogueStart>(new OnDialogueStart { dialogueData = Dialogue[Random.Range(0, Dialogue.Length)] });
        else
        {
            InvokeEvent<OnDialogueStart>(new OnDialogueStart { dialogueData = Dialogue[currentLineIdxSpeaker1]});
            currentLineIdxSpeaker1 = currentLineIdxSpeaker1 + 1 > Dialogue.Length - 1 ? 0 : currentLineIdxSpeaker1++;
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
    
    IEnumerator Delay(bool isPositiv)
    {
        if (delay == 0) yield return new WaitForEndOfFrame();
        else yield return new WaitForSeconds(delay);

        InvokeEvent<OnDialogueStart>(new OnDialogueStart { dialogueData = isPositiv ? Dialogue[currentLineIdxSpeaker1] : Dialogue[1] });
        if (HaveCooldown)StartCoroutine(Cooldown());
        alreadyPlayed = true;
        if(isPositiv)currentLineIdxSpeaker1 = currentLineIdxSpeaker1 + 2 > Dialogue.Length - 1 ? 0 : currentLineIdxSpeaker1+2;
        else currentLineIdxSpeaker2 = currentLineIdxSpeaker2 + 2 > Dialogue.Length - 1 ? 0 : currentLineIdxSpeaker2+2;
    }

    IEnumerator Cooldown()
    {
        canBePlayed = false;
        yield return new WaitForSeconds(cooldownDuration);
        canBePlayed = true;
    }

}