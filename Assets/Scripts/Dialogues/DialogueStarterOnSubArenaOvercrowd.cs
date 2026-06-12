using UnityEngine;

public class DialogueStarterOnSubArenaOvercrowed : DialogueStarterGeneric<OnSubArenaUpdateEvent>
{
    protected override void OnEventTriggered(OnSubArenaUpdateEvent e)=>PlayDialogue(e.p_cardinalDirection);
}