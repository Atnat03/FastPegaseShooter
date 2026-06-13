public class DialogueStarterFriendlyFire : DialogueStarterGeneric<OnFiendlyFire>
{
    protected override void OnEventTriggered(OnFiendlyFire e) => PlayDialogue(e.isPositive);
}
public struct OnFiendlyFire
{
    public bool isPositive;
}
