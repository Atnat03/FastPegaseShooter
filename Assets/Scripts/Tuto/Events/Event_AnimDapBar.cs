using System.Collections;

namespace Tuto
{
    public enum AnimationBar { None, Scale, Vibration }

    public class Event_AnimDapBar : BaseEvent
    {
        public override string DisplayName => "Anim dap bar";

        public AnimationBar animationType;
        public float duration = 1f;

        public override IEnumerator Execute()
        {
            manager.AnimDapBar(animationType, duration);
            yield return null;
        }
    }
}