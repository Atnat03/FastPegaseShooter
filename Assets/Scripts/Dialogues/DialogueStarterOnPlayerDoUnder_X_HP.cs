
public struct OnPlayerGoUnder_X_PV_Dialogue
{
    public bool isPositive;
}

public class DialogueStarterOnPlayerGoUnder_X_PV : DialogueStarterGeneric<OnPlayerGoUnder_X_PV_Dialogue>
{
    protected override void OnEventTriggered(OnPlayerGoUnder_X_PV_Dialogue e) => PlayDialogue(e.isPositive);
}