using System;
using UnityEngine;

namespace Tuto
{
    [Serializable]
    public abstract class BaseTrigger
    {
        public Action OnActivated;
        public abstract string DisplayName { get; }
        public virtual void Initialize() { }
        public virtual void Dispose() { }
    }
}