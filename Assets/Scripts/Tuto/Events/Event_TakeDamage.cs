using System.Collections;

namespace Tuto
{
    public class Event_TakeDamage : BaseEvent
    {
        public override string DisplayName => "Taking Damage";

        public int damage = 10;
        public int seuilDamage = 50;
        
        public override IEnumerator Execute()
        {
            manager.TakeDamage(damage, seuilDamage);
            
            yield break;
        }
    }
}