using System;
using MyPrint;
using UnityEngine;

namespace Tuto.Triggers
{
    [Serializable]
    public class Trigger_Dapping : BaseTrigger
    {
        public override string DisplayName => "Use DAP";
        
        TutoManager manager;

        public override void Initialize(TutoManager tuto)
        {
            manager = tuto;

            if(manager != null)
                manager.OnDapUsed += Activated;
        }
        
        public override void Dispose()
        {
            if(manager != null)
                manager.OnDapUsed -= Activated;
        }

        private void Activated()
        {
            Cons.Print("Activated", ColorConsole.Pink);
            OnActivated?.Invoke();
        }
    }
}