using System.Collections;
using MyPrint;

namespace Tuto
{
    public class Event_Notification : BaseEvent
    {
        public override string DisplayName => "Notification";
        public override IEnumerator Execute()
        {
            Cons.Print("Notification", ColorConsole.Blue);
            
            yield break;
        }
    }
}