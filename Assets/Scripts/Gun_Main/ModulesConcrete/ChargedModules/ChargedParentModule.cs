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
            
            OnFullCharged?.Invoke(false, false);
        }
        
        
        public virtual void TryShootCharging()
        { }

        protected void ResetCharging()
        {
            _gunController.RecoilModule?.SetIsRecoil(false);
            OnFullCharged?.Invoke(false, false);
            
            _gunController?.OnStopCharging?.Invoke();
        }
    }
}