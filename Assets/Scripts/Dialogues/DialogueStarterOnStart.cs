using System;
using UnityEngine;

public class DialogueStarterOnStart : DialogueStarterParent
{
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        PlayDialogue();
    }
}
