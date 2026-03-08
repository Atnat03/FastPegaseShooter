using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;

namespace GunDecorator
{
    public class FragShootModule : GunModule, IShootModule
    {
        public bool IsFullAuto => _isFullAuto;
        public float FireRate => _fireRate;
        public IAmmoModule AmmoModule => _ammoModule;
        public bool IsExplosed => _isExplosedAmmo;

        [SerializeField] protected MonoBehaviour[] _secondModule;
        List<ISecondModule> _additionalEffectModule;
        
        [SerializeField] MonoBehaviour _ammoType;
        protected IAmmoModule _ammoModule;

        [SerializeField]private bool _isFullAuto;
        [SerializeField]private float _fireRate;
        
        [SerializeField] private float _numberBulletSpread;
        [SerializeField, Range(0, 60)] private float _spreadAngle;
        private bool _isExplosedAmmo = false;
        
        
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

        public void TryShoot()
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

        public void Shooting()
        {
            if (_ammoModule != null)
            {
                for (int i = 0; i < _numberBulletSpread; i++)
                {
                    Vector3 direction = 
                            new Vector3(
                                Random.Range(-_spreadAngle, _spreadAngle),
                                Random.Range(-_spreadAngle, _spreadAngle),
                                0);
                    
                    _ammoModule.SpawnBullet(direction, Vector3.zero);
                }
                
                _ammoModule.ResetBulletData();
                
                PlayShootSoundObserverRpc();
                
            }
        }

        [ObserversRpc]
        private void PlayShootSoundObserverRpc()
        {
            AudioClip clip = SoundManager.GetAudioClip(_gunController._soundData, "Shoot");
            SoundManager.PlaySound(clip, _gunController._source);
        }
        
        public void CancelShooting()
        { }
    }
}