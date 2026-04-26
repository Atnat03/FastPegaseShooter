using System.Collections;
using UnityEngine;

namespace GunDecorator.ChargedModules
{
    public class SalveChargedModule : ChargedParentModule
    {
        [Header("Salve")] 
        [SerializeField] private float _intervaleCharge = 0.05f;
        
        public override void SetVariable(GunSetting setting)
        {
            if (setting is ChargedSalveSetting s)
            {
                _isExplosifAmmo = s.IsExplosifAmmo;
                _explosionRadius = s.explosionRadius;
                _deadZoneStartCharging = s.DeadZoneStartCharging;
                _recoilChargedMultiplier =  s.recoilChargedMultiplier;
                _recoilX = s.RecoilX;
                _timeToCharge = s.timeToCharge;
                _isFullMultiplicator = s.IsFullMultiplicator;
                _numberBulletInCharge = s.NumberBulletInCharged;
                _intervaleCharge = s.intervaleCharge;
            }
        }
        
        public override void TryShootCharging()
        {
            base.TryShootCharging();
            
            if (_charging)
            {
                int numberBulletShoot = (int)Mathf.Lerp(0, _numberBulletInCharge, _charginTimer / _timeToCharge);

                _ammoModule.SetBulletData(new BulletData
                {
                    IsExplosive = _isExplosifAmmo,
                    IsCritical = _gunController.IsOverload,
                    ExplosionRadius = _explosionRadius
                });
                
                StartCoroutine(ShootSalve(numberBulletShoot));
            }
            
            ResetCharging();
        }

        IEnumerator ShootSalve(int numberBullet)
        {
            _gunController.PlaySound("Charged");
            
            for (int i = 0; i < numberBullet; i++)
            {
                _ammoModule.SpawnBullet(Vector3.zero, Vector3.zero);
                _gunController.SetAmmo(_gunController.GetCurrentAmmo() - 1, _gunController.IsInfiniteAmmo);
                
                _gunController.RecoilModule?.Recoil(_gunController.ModelGun.transform, 0.1f, false, _recoilChargedMultiplier, _recoilX);
                _gunController.RecoilModule?.SetIsRecoil(true);
                
                yield return new WaitForSeconds(_intervaleCharge);
            }
            
            _gunController?.OnStopCharging?.Invoke();
            _ammoModule.ResetBulletData();
        }
    }
}