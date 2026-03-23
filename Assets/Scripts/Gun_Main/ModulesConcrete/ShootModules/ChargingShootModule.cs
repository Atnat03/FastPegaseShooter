using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GunDecorator
{
    public class ChargingShootModule : GunModule, IShootModule
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

        [Header("Charging")]
        
        [SerializeField] private float _numberBulletInShootCharge = 5;
        [SerializeField] private float _timeToCharge = 3;
        [SerializeField] private float _startToChargingTime = 1;
        [SerializeField] private float _recoilChargedMultiplier = 1.25f;
        private bool _isCharged = false;
        private bool _charging = false;
        private bool _startCharging = false;
        private float _charginTimer = 0;
        private float _startToChargingTimer = 0;

        [Header("Charging Visual")] 
        [SerializeField] private Vector3 _fullChargeSize;
        [SerializeField] private GameObject _chargingEffect;
        [SerializeField] private Color _chargingColor;
        [SerializeField] private Color _fullChargeColor;
        private Vector3 _startModelScale;
        private Vector3 _startBallVisuelScale;
        
        
        private bool _isExplosedAmmo = false;
        private Vector3 _directionOffset = Vector3.zero;
        private Vector3 _bulletOffset = Vector3.zero;
        
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
            
            _startModelScale = _gunController._model.transform.localScale;
            _startBallVisuelScale = _chargingEffect.transform.localScale;

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
            if(!_isCharged)
            {
                if (_ammoModule != null)
                {
                    if (_gunController.IsOverload)
                        _ammoModule.SetDamage(_gunController.SurchargeMultiplierDamage);

                    _ammoModule.SpawnBullet(_directionOffset, _bulletOffset);

                    _ammoModule.ResetBulletData();

                    AudioClip clip = SoundManager.GetAudioClip(_gunController._soundData,"Shoot");
                    SoundManager.PlaySound(clip, _gunController._source, 0.5f);
                }
                
                _startCharging = true;
                _startToChargingTimer = _startToChargingTime;
            }
        }

        public void CancelShooting()
        {
            if (_charging)
            {
                if (_ammoModule != null)
                {
                    if(_gunController.IsOverload)
                        _ammoModule.SetDamage(_gunController.SurchargeMultiplierDamage);

                    float maxBulletShoot = _numberBulletInShootCharge;
                    int numberBulletShoot = (int)Mathf.Lerp(0, maxBulletShoot, _charginTimer/_timeToCharge);
                    float range = numberBulletShoot * 0.02f;
                    
                    _gunController.RecoilModule.Recoil(_gunController.ModelGun.transform, 0.25f ,_recoilChargedMultiplier);
                    
                    for (int i = 0; i < numberBulletShoot; i++)
                    {
                        Vector3 offset = new Vector3(
                            Random.Range(-range, range),
                            Random.Range(-range, range),
                            0);
                        _ammoModule.SpawnBullet(Vector3.zero, offset);
                        _gunController.SetAmmo(_gunController.GetCurrentAmmo() - 1);
                    }
                
                    _ammoModule.ResetBulletData();
                
                    AudioClip clip = SoundManager.GetAudioClip(_gunController._soundData,"ChargedSound");
                    SoundManager.PlaySound(clip, _gunController._source, 0.5f);
                
                    _isCharged = false;
                }
            }

            _charging = false;
            _startCharging = false;
            _charginTimer = 0;
            _startToChargingTimer = 0;
            _gunController._model.transform.localScale = _startModelScale;
            _chargingEffect.transform.localScale = _startBallVisuelScale;
        }
        
        private void Update()
        {
            _chargingEffect.SetActive(_charging);
            _chargingEffect.GetComponent<MeshRenderer>().material.color = _isCharged ? _fullChargeColor : _chargingColor;
            
            if(!_isCharged)
            {
                if (_startCharging)
                {
                    if (_startToChargingTimer > 0)
                    {
                        _startToChargingTimer -= Time.deltaTime;
                    }
                    else
                    {
                        _startCharging = false;
                        _charging = true;
                        _startToChargingTimer = 0;
                        _charginTimer = 0;
                    }
                }

                if (_charging)
                {
                    _gunController._model.transform.localScale = Vector3.Lerp(_startModelScale, _fullChargeSize, _charginTimer/_timeToCharge);
                    _chargingEffect.transform.localScale = Vector3.Lerp(_startBallVisuelScale, _fullChargeSize, _charginTimer/_timeToCharge);
                    
                    if (_charginTimer < _timeToCharge)
                    {
                        _charginTimer += Time.deltaTime;
                    }
                    else
                    {
                        _isCharged = true;
                    }
                }
            }
        }
        
        public void SetDirectionModifier(Vector3 direction) => _directionOffset =  direction;
        public void SetBulletOffset(Vector3 offset) =>  _bulletOffset = offset;
    }
}