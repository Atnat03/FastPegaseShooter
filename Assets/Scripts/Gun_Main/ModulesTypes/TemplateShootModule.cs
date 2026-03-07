using System;
using System.Collections.Generic;
using FishNet.Object;
using ScriptableObjectsDefinitions;
using UnityEngine;

namespace GunDecorator
{
    public class TemplateShootModule : GunModule, IShootModule
    {
        public bool IsFullAuto => _isFullAuto;
        public float FireRate => _fireRate;
        public IAmmoModule AmmoModule => _ammoModule;

        [SerializeField] protected MonoBehaviour[] _secondModule;
        List<ISecondModule> _additionalEffectModule;
        
        [SerializeField] MonoBehaviour _ammoType;
        protected IAmmoModule _ammoModule;

        [SerializeField]private bool _isFullAuto;
        [SerializeField]private float _fireRate;
        
        private BulletData _currentBulletConfig;
        
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
                
                _ammoModule.SpawnBullet(Vector3.zero);
                
                _ammoModule.ResetBulletData();
                
                PlayShootSoundObserverRpc();
                
                return;
            }
            
            Debug.Log("Bullet fired");
        }

        [ObserversRpc]
        private void PlayShootSoundObserverRpc()
        {
            AudioClip clip = SoundManager.GetAudioClip(_gunController._soundData, "Shoot");
            SoundManager.PlaySound(clip, _gunController._source);
        }
    }
}