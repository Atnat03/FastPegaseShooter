using UnityEngine;

public class DialogueStarterOnSubArenaCorosionStarted : DialogueStarterGeneric<OnSubArenaUpdateEvent>
{
    protected override void OnEventTriggered(OnSubArenaUpdateEvent e)
    {
        if((int)Mathf.Clamp01(e.p_overCrowdingPercent) == 1)PlayDialogue(e.p_cardinalDirection);
    }
    
}