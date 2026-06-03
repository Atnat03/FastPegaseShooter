using System.Collections;

namespace Tuto
{
    public enum AnimationBar {None, Scale, Vibration}
    
    public class Event_ChangeFillAmount : BaseEvent
    {
        public override string DisplayName => "Change fill amount";

        public bool activated = true;
        public float maxPercentage = 50;
        public float speedFill = 3;
        
        public AnimationBar animationType;
        
        public override IEnumerator Execute()
        {
            manager.FillAmount(maxPercentage, speedFill, activated, animationType);
            
            yield return null;
        }
    }
}