using System;
using System.Collections.Generic;
using UnityEngine;

namespace GunDecorator
{
    public abstract class TemplateShootModule : GunModule, IShootModule
    {
        [SerializeField] protected MonoBehaviour[] _secondModule;
        List<ISecondModule> _additionalEffectModule;
        
        [SerializeField] MonoBehaviour _ammoType;
        protected IAmmoModule _ammoModule;

        private void Start()
        {
            _additionalEffectModule = new List<ISecondModule>();
            
            //Set up des modules secondaires
            foreach (MonoBehaviour module in _secondModule)
            {
                ISecondModule secondModule = (ISecondModule)module;

                if(secondModule != null)
                {
                    secondModule.SetUpModule(this);
                    _additionalEffectModule.Add(secondModule);
                }
            }

            if(_ammoType != null)
                _ammoModule = (IAmmoModule)_ammoType;
        }

        public virtual void TryShoot()
        {
            if (_additionalEffectModule.Count == 0)
            {
                Shooting();
                return;
            }

            for (int i = 0; i < _additionalEffectModule.Count - 1; i++)
                _additionalEffectModule[i].SetNext(_additionalEffectModule[i + 1]);

            _additionalEffectModule[^1].SetNext(null);

            _additionalEffectModule[0].DoAdditionnalEffect();
        }

        public virtual void Shooting()
        {
            if (_ammoModule != null)
            {
                _ammoModule.SpawnBullet();
                return;
            }
            
            //Shooting classique
            
            Debug.Log("Bullet fired");
        }
    }
}