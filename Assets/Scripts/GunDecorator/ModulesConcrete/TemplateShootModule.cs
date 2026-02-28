using System;
using UnityEngine;

namespace GunDecorator
{
    public class TemplateShootModule : GunModule, IShootModule
    {
        [SerializeField] private float _fireRate;
        
        [SerializeField] protected MonoBehaviour[] _secondModule;
        ISecondModule[] _additionalEffectModule;

        private void Start()
        {
            foreach (MonoBehaviour module in _secondModule)
            {
                Debug.Log("Set up second module");
                
                ISecondModule secondModule = (ISecondModule)module;

                if(secondModule != null)
                    secondModule.SetUpModule(this);
            }
        }

        public virtual void Shoot()
        {
            Debug.Log("Default gun shooting");

            foreach (ISecondModule secondModule in _additionalEffectModule)
            {
                secondModule.DoAdditionnalEffect();
            }
        }
    }
}