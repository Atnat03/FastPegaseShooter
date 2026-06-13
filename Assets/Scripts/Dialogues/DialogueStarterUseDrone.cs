
public struct OnUseDrone_Dialogue
{
    public bool isPositive;
}

public class DialogueStarterUseDrone : DialogueStarterGeneric<OnUseDrone_Dialogue>
{
    protected override void OnEventTriggered(OnUseDrone_Dialogue e) => PlayDialogue(e.isPositive);
}