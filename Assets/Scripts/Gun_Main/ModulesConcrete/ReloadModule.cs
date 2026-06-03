using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GunDecorator
{
    [DisallowMultipleComponent]
    public class ReloadModule : GunModule, IReloadModule
    {
        public int CurrentAmmo => _currentAmmo;
        public bool AutoReload => _autoReload;
        
        public bool IsReloading => _isReloading;

        [SerializeField, Tooltip("Si coché => l'arme recharge automatiquement quand on arrive à 0 balles dans le chargeur")]
        private bool _autoReload = true;
        [SerializeField, Tooltip("Nombre max de munition dans un chargeur")] private int _magazineSize = 30;
        [SerializeField, Tooltip("Temps de rechargement")] private float reloadDuration = 3f;
        private int _currentAmmo = 0;
        private bool _isReloading = false;
        

        
        public Coroutine p_reloadCoroutine = null;
        
        public override void SetVariable(GunSetting setting)
        {
            if (setting is ReloadSetting s)
            {
                _autoReload = s.isAutoReload;
                _magazineSize = s.magazineSize;
                reloadDuration = s.reloadDuration;
            }
        }

        public override void Initialize(GunController gun)
        {
            base.Initialize(gun);
            SetAmmo(_magazineSize, false);
        }

        private void Update()
        {
            _currentAmmo = Mathf.Clamp(_currentAmmo, 0, _magazineSize);
            
            if (_autoReload)
            {
                if (_currentAmmo <= 0)
                {
                    Reload();
                }
            }
        }

        public void StopReload()
        {
            if(p_reloadCoroutine != null)
            {
                StopCoroutine(p_reloadCoroutine);
                p_reloadCoroutine = null;
                _isReloading = false;
                _gunController?._animator.ResetTrigger("Reload");
                
                if(_gunController?._animatorArm)
                    _gunController?._animatorArm.ResetTrigger("Reload");
                
                _gunController?.OnEndReload?.Invoke();
            }
        }

        public void Reload()
        {
            if(p_reloadCoroutine == null)
                p_reloadCoroutine = StartCoroutine(ReloadCoroutine());
        }

        IEnumerator ReloadCoroutine()
        {
            _gunController?.OnStartReload?.Invoke(reloadDuration);
            
            _gunController?._animator.SetTrigger("Reload");
            
            if(_gunController?._animatorArm)
                _gunController?._animatorArm.SetTrigger("Reload");
            
            _gunController?.PlaySound("Reload");
            
            _isReloading = true;

            yield return new WaitForSeconds(reloadDuration);
            
            _isReloading = false;
            SetAmmo(_magazineSize, false);
            
            p_reloadCoroutine = null;
            
            if (_gunController.IsFullAuto)
            {
                _gunController.ResetNoise();
                _gunController.ApplyShoot();
            }
        }

        public void SetAmmo(int value, bool infiniteAmmo)
        {
            if(!infiniteAmmo)
                _currentAmmo = Mathf.Min(value, _magazineSize);
            
            _gunController.OnShootAmmo?.Invoke(_currentAmmo, _magazineSize);
        }
    }
}