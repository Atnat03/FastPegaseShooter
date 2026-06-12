using UnityEngine;

public class DialogueStarterOnSubArenaDangerous : DialogueStarterGeneric<OnSubArenaUpdateEvent>
{
    protected override void OnEventTriggered(OnSubArenaUpdateEvent e)
    {
        if(e.p_state.p_name == "Dangerous" && (int)Mathf.Clamp01(e.p_overCrowdingPercent) != 1)PlayDialogue(e.p_cardinalDirection);
    }
}
