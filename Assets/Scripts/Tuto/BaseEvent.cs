using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Tuto
{
    [Serializable]
    public abstract class BaseEvent
    {
        protected TutoManager manager;
        public void SetManager(TutoManager m) => manager = m;
        
        public abstract string DisplayName { get; }
        public abstract IEnumerator Execute();
    }
}