public class DialogueStarterOnSubArenaOvercrowed : DialogueStarterGeneric<OnSubArenaUpdateEvent>
{
    protected override void OnEventTriggered(OnSubArenaUpdateEvent e)=>PlayDialogue((int)e.p_cardinalDirection);
}