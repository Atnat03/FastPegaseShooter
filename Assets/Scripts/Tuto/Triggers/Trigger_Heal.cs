using MyPrint;

namespace Tuto.Triggers
{
    public class Trigger_Heal : BaseTrigger
    {
        public override string DisplayName => "Both Use Heal";
        
        TutoManager manager;

        public override void Initialize(TutoManager tuto)
        {
            manager = tuto;

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
            OnActivated?.Invoke();
        }
    }
}