using System.Collections;
using UnityEngine;

namespace GunDecorator.ChargedModules
{
    public class SalveChargedModule : ChargedParentModule
    {
        [Header("Salve")] 
        [SerializeField] private float _intervaleCharge = 0.05f;
        [SerializeField, Range(0, 30)] private float _noiseCharged = 5;
        
        public override void SetVariable(GunSetting setting)
        {
            if (setting is ChargedSalveSetting s)
            {
                _damageChargedMultiplicator = s._damageChargedMultiplicator;
                _isExplosifAmmo = s.IsExplosifAmmo;
                _explosionRadius = s.explosionRadius;
                _recoilChargedMultiplier =  s.recoilChargedMultiplier;
                _recoilX = s.RecoilX;
                _numberBulletInCharge = s.NumberBulletInCharged;
                _intervaleCharge = s.intervaleCharge;
            }
        }
        
        public override void TryShootCharging()
        {
            base.TryShootCharging();
            
            if (!_fullCharge) return;
            
            _ammoModule.SetBulletData(new BulletData
            {
                IsExplosive = _isExplosifAmmo,
                IsCritical = _gunController.IsOverload,
                ExplosionRadius = _explosionRadius
            });
                
            StartCoroutine(ShootSalve(_numberBulletInCharge));
            
            ResetCharging();
        }

        IEnumerator ShootSalve(int numberBullet)
        {
            _gunController.PlaySound("Charged");

            _ammoModule.SetDamage(_damageChargedMultiplicator);
            
            for (int i = 0; i < numberBullet; i++)
            {
                Vector3 spread = new Vector3(
                    Random.Range(-_noiseCharged, _noiseCharged),
                    Random.Range(-_noiseCharged, _noiseCharged),
                    0);
                
                _ammoModule.SpawnBullet(spread, Vector3.zero);
                
                _gunController.RecoilModule?.Recoil(_gunController.ModelGun.transform, 0.1f, false, _recoilChargedMultiplier, _recoilX);
                _gunController.RecoilModule?.SetIsRecoil(true);
                
                yield return new WaitForSeconds(_intervaleCharge);
            }
            
            _gunController?.OnStopCharging?.Invoke();
            _ammoModule.ResetBulletData();
            _ammoModule.SetDamage(1);
        }
    }
}