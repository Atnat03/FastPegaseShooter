using UnityEngine;

public class DialogueStarterOnDap : DialogueStarterParent
{
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        ListenToEvent<OnDapEvent>(OnDapEventTriggered);
    }

    void OnDapEventTriggered(OnDapEvent dapEvent)
    {
        PlayDialogue();
    }
}