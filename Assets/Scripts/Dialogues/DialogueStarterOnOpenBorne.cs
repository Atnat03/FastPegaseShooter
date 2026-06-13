using UnityEngine;

public struct OnOpenBorne
{
    public bool p_playerPositive;
}

public class DialogueStarterOnOpenBorne : DialogueStarterGeneric<OnOpenBorne>
{
    
    protected override void OnEventTriggered(OnOpenBorne e) => PlayDialogue(e.p_playerPositive);
    
}
