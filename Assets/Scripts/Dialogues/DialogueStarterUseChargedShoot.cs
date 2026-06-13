
public struct OnUseChargedShoot_Dialogue
{
    public bool isPositive;
}

public class DialogueStarterChargedShoot : DialogueStarterGeneric<OnUseChargedShoot_Dialogue>
{
    protected override void OnEventTriggered(OnUseChargedShoot_Dialogue e) => PlayDialogue(e.isPositive);
}