using System.Collections;

namespace Tuto
{
    public class Event_ChangeFillAmount : BaseEvent
    {
        public override string DisplayName => "Change fill amount";

        public bool activated = true;
        public float maxPercentage = 50;
        public float speedFill = 3f;

        public override IEnumerator Execute()
        {
            manager.FillAmount(maxPercentage, speedFill, activated);
            yield return null;
        }
    }
}