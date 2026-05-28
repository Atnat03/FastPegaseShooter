using System;
using System.Collections.Generic;
using MyPrint;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GunDecorator.ChargedModules
{
    [RequireComponent(typeof(VisualChargedModule))]
    public abstract class ChargedParentModule : GunModule
    {
        #region Properties

        #endregion
        
        #region Variables
        [SerializeField] MonoBehaviour _ammoType;
        protected IAmmoModule _ammoModule;
        protected ShootModule _shootModule;

        [SerializeField] protected bool _isExplosifAmmo = false;
        [SerializeField] protected float _explosionRadius = 1f;

        [Header("Charging")] 
        [SerializeField] protected int _currentPercentCharge = 0;
        [SerializeField] protected Vector2 _oneAmmoAddPercentage = new Vector2(5, 5);
        [SerializeField] protected bool _fullCharge = false;
        
        [SerializeField] protected float _damageChargedMultiplicator = 10;
        [SerializeField] protected float _recoilChargedMultiplier = 1.25f;
        [SerializeField] protected float _recoilX = 2f;
        [SerializeField] protected int _numberBulletInCharge = 10;
        [SerializeField] protected float _coolDownCharge = 0.5f;

        public bool _isChargedShooting = false;
        
        //Action
        public Action<int> OnPercentageChargeChange;
        public Action<bool, bool> OnFullCharged;
        
        #endregion
        
        private void Start()
        {
            if(_ammoType != null)
                _ammoModule = (IAmmoModule)_ammoType;

            _shootModule = GetComponent<ShootModule>();
            
            OnPercentageChargeChange?.Invoke(_currentPercentCharge);
            OnFullCharged?.Invoke(false, false);
        }
        
        
        public virtual void TryShootCharging()
        {
            if (_isChargedShooting)
                return;
            
            OnPercentageChargeChange?.Invoke(_currentPercentCharge);
        }

        protected void ResetCharging()
        {
            _gunController.RecoilModule?.SetIsRecoil(false);
            OnFullCharged?.Invoke(false, false);
            
            _gunController?.OnStopCharging?.Invoke();
        }

        public void AddPercentage()
        {
            if (_currentPercentCharge >= 100) return;
            
            _currentPercentCharge += (int)Random.Range(_oneAmmoAddPercentage.x, _oneAmmoAddPercentage.y);

            if (_currentPercentCharge >= 100)
            {
                _currentPercentCharge = 100;
                OnFullCharged?.Invoke(true, false);
            }
            else
            {
                OnFullCharged?.Invoke(false, false);
            }
            
            OnPercentageChargeChange?.Invoke(_currentPercentCharge);
        }
        
        private void Update()
        {
            _fullCharge = _currentPercentCharge >= 100;
        }

        public void SetPercentage(int percent)
        {
            _currentPercentCharge = percent;
            
            OnPercentageChargeChange?.Invoke(_currentPercentCharge);
            OnFullCharged?.Invoke(true, true);
        }
    }
}