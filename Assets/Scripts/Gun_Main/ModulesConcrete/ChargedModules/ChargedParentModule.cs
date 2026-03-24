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

        [SerializeField] protected bool _isExplosifAmmo = false;
        [SerializeField] protected float _explosionRadius = 1f;
        
        [Header("Charging")]
        [SerializeField] protected float _timeToCharge = 1;
        [SerializeField] protected float _deadZoneStartCharging = 0.5f;
        [SerializeField] protected float _recoilChargedMultiplier = 1.25f;
        [SerializeField] protected float _isFullMultiplicator = 0.9f;
        private bool _fullCharged = false;
        protected bool _charging = false;
        private bool _deadZoneCharge = false;
        
        protected float _charginTimer = 0;
        private float _elapsedTimeDeadZone = 0;
        
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
        }

        private void Update()
        {
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
                
                OnCharging?.Invoke(_charginTimer / _timeToCharge);
                
                IsFullCharged = _charginTimer >= _timeToCharge * _isFullMultiplicator;
            }
        }

        public void TryCharging()
        {
            _deadZoneCharge = true;
        }
        
        public virtual void TryShootCharging()
        {
            ResetCharging();
        }

        protected void ResetCharging()
        {
            _deadZoneCharge = false;
            _charging = false;
            _charginTimer = 0;
            _elapsedTimeDeadZone = 0;
            _triggerActionStart = false;
            
            OnEndCharging?.Invoke();
        }
    }
}