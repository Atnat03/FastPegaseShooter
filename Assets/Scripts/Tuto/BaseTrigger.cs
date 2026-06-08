using System;
using UnityEngine;

namespace Tuto
{
    [Serializable]
    public abstract class BaseTrigger
    {
        public Action OnActivated;
        public abstract string DisplayName { get; }
        public virtual void Initialize(TutoManager tuto) { }
        public virtual void Activate() { }
        public virtual void Dispose() { }
        
        public virtual BaseTrigger Clone()
        {
            BaseTrigger copy = (BaseTrigger)MemberwiseClone();
            copy.OnActivated = null;
            return copy;
        }
    }
}