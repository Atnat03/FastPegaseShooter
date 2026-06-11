public abstract class DialogueStarterGeneric<TEvent> : DialogueStarterParent 
    where TEvent : struct
{
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        ListenToEvent<TEvent>(OnEventTriggered);
    }

    void OnEventTriggered(TEvent e) => PlayDialogue();
}