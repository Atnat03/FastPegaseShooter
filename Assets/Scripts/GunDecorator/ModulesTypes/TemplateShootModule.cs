using System;
using System.Collections.Generic;
using UnityEngine;

namespace GunDecorator
{
    public abstract class TemplateShootModule : GunModule, IShootModule
    {
        [SerializeField] protected MonoBehaviour[] _secondModule;
        List<ISecondModule> _additionalEffectModule;

        private void Start()
        {
            _additionalEffectModule = new List<ISecondModule>();
            
            foreach (MonoBehaviour module in _secondModule)
            {
                Debug.Log("Set up second module");
                
                ISecondModule secondModule = (ISecondModule)module;

                if(secondModule != null)
                {
                    secondModule.SetUpModule(this);
                    _additionalEffectModule.Add(secondModule);
                }
            }
        }

        public virtual void TryShoot()
        {
            if(_secondModule.Length == 0)
                Shooting();
            else
            {
                foreach (ISecondModule secondModule in _additionalEffectModule)
                {
                    secondModule?.DoAdditionnalEffect();
                }
            }
        }

        public virtual void Shooting()
        {
            Debug.Log("Bullet fired");
        }
    }
}