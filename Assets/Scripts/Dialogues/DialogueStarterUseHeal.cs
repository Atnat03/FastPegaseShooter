public class DialogueStarterUseHeal : DialogueStarterGeneric<OnUseHeal_Dialogue>
{
    protected override void OnEventTriggered(OnUseHeal_Dialogue e) => PlayDialogue(e.isPositive);
}
public struct OnUseHeal_Dialogue
{
    public bool isPositive;
}
