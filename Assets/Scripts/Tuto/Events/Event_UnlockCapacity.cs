using System;
using System.Collections;

namespace Tuto
{
    public enum Capacity_TUTO
    {
        EnergyShoot, ChargedShoot, Drone, Heal
    }
    
    public class Event_UnlockCapacity : BaseEvent
    {
        public override string DisplayName => "Unlock capacity";
     
        public Capacity_TUTO capacityToUnlock;
        
        public override IEnumerator Execute()
        {
            manager.AskForUnlockCapa(capacityToUnlock);
            
            yield break;
        }
    }
}