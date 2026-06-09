using MyPrint;
using UnityEngine;

namespace Tuto.Triggers
{
    public class Trigger_Dapping : BaseTrigger
    {
        public override string DisplayName => "Use DAP";

        [SerializeField] private int _numberUseOfHealNeed = 1;
        private int _currentNumberHealNeed = 0;
        
        TutoManager manager;

        public override void Initialize(TutoManager tuto)
        {
            manager = tuto;
            _currentNumberHealNeed = 0;

            if(manager != null)
                manager.OnBothUseHeal += Activated;
        }
        
        public override void Dispose()
        {
            if(manager != null)
                manager.OnBothUseHeal -= Activated;
        }

        private void Activated()
        {
            _currentNumberHealNeed++;
            
            if(_currentNumberHealNeed == _numberUseOfHealNeed * 2)
                OnActivated?.Invoke();
        }
    }
}