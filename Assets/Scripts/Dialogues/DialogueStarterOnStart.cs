using System;
using System.Collections;
using UnityEngine;

public class DialogueStarterOnStart : DialogueStarterParent
{
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        PlayDialogue();
    }
}