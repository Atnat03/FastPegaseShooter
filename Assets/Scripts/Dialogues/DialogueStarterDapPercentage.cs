
public class DialogueStarterDapPercentage : DialogueStarterGeneric<OnDapReachPercentage>
{
    protected override void OnEventTriggered(OnDapReachPercentage e) => PlayDialogue(e.percentage%25);
}
public struct OnDapReachPercentage
{
    public int percentage;
}
