public class DialogueStarterNoEchangeDuringLongTime : DialogueStarterGeneric<OnDapWaitTooLongWithoutChange>
{
    protected override void OnEventTriggered(OnDapWaitTooLongWithoutChange e) => PlayDialogue();
}
public struct OnDapWaitTooLongWithoutChange
{ }
