using UnityEngine;

public class DialogueStarterOnSubArenaDangerous : DialogueStarterGeneric<OnSubArenaUpdateEvent>
{
    bool alreadyPlayed = false;
    protected override void OnEventTriggered(OnSubArenaUpdateEvent e)
    {
        if (e.p_state.p_name == "Dangerous" && (int)Mathf.Clamp01(e.p_overCrowdingPercent) != 1 && !alreadyPlayed)
        {
            PlayDialogue(e.p_cardinalDirection);
            alreadyPlayed = true;
        }
        else
        {
            if (e.p_state.p_name != "Dangerous" && (int)Mathf.Clamp01(e.p_overCrowdingPercent) != 1 && alreadyPlayed)
            {
                alreadyPlayed = false;
            }
        }
    }
}
