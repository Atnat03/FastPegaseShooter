using System;
using System.Collections.Generic;
using ScriptableObjectsDefinitions;
using UnityEngine;

namespace GunDecorator
{
    public class TemplateShootModule : GunModule, IShootModule
    {
        [SerializeField] protected MonoBehaviour[] _secondModule;
        List<ISecondModule> _additionalEffectModule;
        
        [SerializeField] MonoBehaviour _ammoType;
        protected IAmmoModule _ammoModule;
        
        [SerializeField] private AudioSource _source;
        [SerializeField] private SoundsDataSO _soundData;
        
        private void Start()
        {
            _additionalEffectModule = new List<ISecondModule>();
            
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
                if(_gunController.IsOverload)
                    _ammoModule.SetDamage(_gunController.SurchargeMultiplierDamage);
                
                _ammoModule.SpawnBullet();
                
                AudioClip clip = SoundManager.GetAudioClip(_soundData, "Shoot");
                SoundManager.PlaySound(clip, _source);
                
                return;
            }
            
            Debug.Log("Bullet fired");
        }
    }
}