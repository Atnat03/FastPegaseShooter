
public struct OnFiendlyFire
{
    public bool isPositive;
}

public class DialogueStarterFriendlyFire : DialogueStarterGeneric<OnFiendlyFire>
{
    protected override void OnEventTriggered(OnFiendlyFire e) => PlayDialogue(e.isPositive);
}