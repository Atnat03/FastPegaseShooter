using System.Collections;
using UnityEngine;

namespace GunDecorator.ChargedModules
{
    public class SalveChargedModule : ChargedParentModule
    {
        [Header("Salve")]
        [SerializeField] private int _numberSalve = 1;
        [SerializeField] private float _intervaleBetweenSalve = 0.5f;
        [SerializeField] private float _intervaleCharge = 0.05f;
        [SerializeField] private Vector2 _noiseCharged = new Vector2(0, 0);
        
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
                _noiseCharged = s.noiseCharged;
                _numberSalve = s.numberSalve;
                _intervaleBetweenSalve = s.intervaleBetweenSalve;
            }
        }
        
        public override void TryShootCharging()
        {
            base.TryShootCharging();

            _isChargedShooting = true;
            
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

            for (int y = 0; y < _numberSalve; y++)
            {
                for (int i = 0; i < numberBullet; i++)
                {
                    Vector3 spread = new Vector3(
                        Random.Range(-_noiseCharged.x, _noiseCharged.x),
                        Random.Range(-_noiseCharged.y, _noiseCharged.y),
                        0);
                    
                    _ammoModule.SpawnBullet(spread, Vector3.zero, false);
                    
                    _gunController.RecoilModule?.Recoil(_gunController.ModelGun.transform, 0.1f, false, _recoilChargedMultiplier, _recoilX);
                    _gunController.RecoilModule?.SetIsRecoil(true);
                    
                    yield return new WaitForSeconds(_intervaleCharge);
                }
                
                yield return new WaitForSeconds(_intervaleBetweenSalve);
            }
            
            _gunController?.OnStopCharging?.Invoke();
            _ammoModule.ResetBulletData();
            _ammoModule.SetDamage(1);

            _isChargedShooting = false;
        }
    }
}