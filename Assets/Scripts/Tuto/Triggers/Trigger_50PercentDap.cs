using System.Collections.Generic;
using System.Linq;
using MyPrint;
using UnityEngine;

namespace Tuto.Triggers
{
    public class Trigger_50PercentDap : BaseTrigger
    {
        public override string DisplayName => "Dap Percentage Reach";

        [SerializeField] private float _percentToReach = 50;
        TutoManager manager;

        public override void Initialize(TutoManager tuto)
        {
            manager = tuto;

            if(manager.DapManagerScript != null)
                manager.DapManagerScript.OnDapReachPercentage += CheckDapPercentage;
        }

        private void CheckDapPercentage(float percent)
        {
            if (manager.DapManagerScript.GetPercentageDap >= _percentToReach)
            {
                OnActivated?.Invoke();
            }
        }

        public override void Dispose()
        {
            if(manager != null)
                manager.DapManagerScript.OnDapReachPercentage -= CheckDapPercentage;
        }
    }
}