using System;
using System.Collections;
using UnityEngine;

public class DialogueStarterOnStart : DialogueStarterParent
{
    public float delay;
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        StartCoroutine(Delay());
    }

    IEnumerator Delay()
    {
        if(delay == 0)yield return new WaitForEndOfFrame();
        else yield return new WaitForSeconds(delay);

        PlayDialogue();

    }
}