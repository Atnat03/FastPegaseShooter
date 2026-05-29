using System.Collections;

namespace Tuto
{
    public class Event_UnlockShootEnergy : BaseEvent
    {
        public override string DisplayName => "Unlock shoot energy";
        
        public override IEnumerator Execute()
        {
            yield break;
        }
    }
}