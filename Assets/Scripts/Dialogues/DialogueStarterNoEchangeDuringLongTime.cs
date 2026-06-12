
public struct OnDapWaitTooLongWithoutChange
{ }

public class DialogueStarterNoEchangeDuringLongTime : DialogueStarterGeneric<OnDapWaitTooLongWithoutChange>
{
    protected override void OnEventTriggered(OnDapWaitTooLongWithoutChange e) => PlayDialogue();
}