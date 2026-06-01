using System.Collections;

namespace Tuto
{
    public class Event_UnlockCapacity : BaseEvent
    {
        public override string DisplayName => "Unlock capacity";
     
        public Capacity capacityToUnlock = Capacity.ChargedShoot;
        
        public override IEnumerator Execute()
        {
            yield break;
        }
    }
}