public abstract class DialogueStarterGeneric<TEvent> : DialogueStarterParent 
    where TEvent : struct
{
    public void Start()
    {
        ListenToEvent<TEvent>(OnEventTriggered);
    }

    protected virtual void OnEventTriggered(TEvent e) => PlayDialogue();
}