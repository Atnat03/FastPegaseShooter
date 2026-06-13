

public class DialogueStarterOnPlayerDie : DialogueStarterGeneric<OnPlayerDie_Dialogue>
{
    protected override void OnEventTriggered(OnPlayerDie_Dialogue e) => PlayDialogue(e.isPositive);
}
public struct OnPlayerDie_Dialogue
{
    public bool isPositive;
}
