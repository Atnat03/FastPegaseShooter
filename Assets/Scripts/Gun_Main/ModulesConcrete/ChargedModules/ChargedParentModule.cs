using System;
using System.Collections.Generic;
using UnityEngine;

namespace GunDecorator.ChargedModules
{
    [RequireComponent(typeof(VisualChargedModule))]
    public abstract class ChargedParentModule : GunModule
    {
        #region Properties

        public bool IsCharging => _charging;

        protected bool IsFullCharged
        {
            get => _fullCharged;
            private set
            {
                _fullCharged = value;
                if(_fullCharged)
                {
                    OnFullCharged.Invoke();
                }
            }
        }

        #endregion
        
        #region Variables
        [SerializeField] MonoBehaviour _ammoType;
        protected IAmmoModule _ammoModule;
        protected ShootModule _shootModule;
        IReloadModule _reloadModule;

        [SerializeField] protected bool _isExplosifAmmo = false;
        [SerializeField] protected float _explosionRadius = 1f;
        
        [Header("Charging")]
        [SerializeField] protected float _timeToCharge = 1;
        [SerializeField] protected float _deadZoneStartCharging = 0.5f;
        [SerializeField] protected float _recoilChargedMultiplier = 1.25f;
        [SerializeField] protected float _recoilX = 2f;
        [SerializeField] protected float _isFullMultiplicator = 0.9f;
        [SerializeField] protected int _numberBulletInCharge = 10;
        [SerializeField] protected float _coolDownCharge = 0.5f;
        private bool _fullCharged = false;
        protected bool _charging = false;
        private bool _deadZoneCharge = false;
        private bool _canCharge = true;
        
        protected float _charginTimer = 0;
        private float _elapsedTimeDeadZone = 0;
        private float _elapsedCooldown = 0;
        
        //Action
        public Action OnStartCharging;
        public Action OnEndCharging;
        public Action<float> OnCharging;
        public Action OnFullCharged;
        private bool _triggerActionStart = false;
        
        #endregion
        
        private void Start()
        {
            if(_ammoType != null)
                _ammoModule = (IAmmoModule)_ammoType;

            _reloadModule = GetComponent<ReloadModule>();
            _shootModule = GetComponent<ShootModule>();
        }

        private void Update()
        {
            if (_elapsedCooldown > 0)
            {
                _elapsedCooldown -= Time.deltaTime;
                _canCharge = false;
                
                if (_elapsedCooldown <= 0)
                {
                    _canCharge = true;
                }
            }
            
            
            if (_reloadModule.IsReloading)
            {
                ResetCharging();
            }
            
            if (_deadZoneCharge)
            {
                _elapsedTimeDeadZone += Time.deltaTime;

                _charging = _elapsedTimeDeadZone >= _deadZoneStartCharging;
            }

            if (_charging)
            {
                if(!_triggerActionStart)
                {
                    OnStartCharging?.Invoke();
                    _triggerActionStart = true;
                }
                
                _charginTimer += Time.deltaTime;

                float ratio = _charginTimer / _timeToCharge;
                
                OnCharging?.Invoke(ratio);
                _gunController?.OnCharging?.Invoke(ratio);
                
                IsFullCharged = _charginTimer >= _timeToCharge * _isFullMultiplicator;
            }
        }

        public void TryCharging()
        {
            if (!_canCharge) return;
            if (_reloadModule.IsReloading) return;
            
            _deadZoneCharge = true;
        }
        
        public virtual void TryShootCharging()
        {
            if (!_canCharge) return;
            if (_reloadModule.IsReloading) return;
            _elapsedCooldown = _coolDownCharge;
        }

        protected void ResetCharging()
        {
            _gunController.RecoilModule?.SetIsRecoil(false);
            
            _deadZoneCharge = false;
            _charging = false;
            _charginTimer = 0;
            _elapsedTimeDeadZone = 0;
            _triggerActionStart = false;
            
            OnEndCharging?.Invoke();
            _gunController?.OnStopCharging?.Invoke();
        }
    }
}