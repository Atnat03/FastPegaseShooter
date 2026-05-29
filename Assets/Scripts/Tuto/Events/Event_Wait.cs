using System.Collections;
using UnityEngine;

namespace Tuto
{
    public class Event_Wait : BaseEvent
    {
        public float _durationToWait;
        public override string DisplayName => "Delay";

        public override IEnumerator Execute()
        {
            yield return new WaitForSeconds(_durationToWait);
        }
    }
}