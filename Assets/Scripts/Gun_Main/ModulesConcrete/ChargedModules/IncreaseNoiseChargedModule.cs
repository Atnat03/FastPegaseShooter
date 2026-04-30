using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GunDecorator.ChargedModules
{
    public class IncreaseNoiseChargedModule : ChargedParentModule
    {
        [Header("Salve")] 
        [SerializeField] private float _noiseAngle = 5f;
        [SerializeField] private float _maximumNoiseAngle = 10;
        
        public override void SetVariable(GunSetting setting)
        {
            if (setting is ChargedIncreaseNoiseSetting s)
            {
                _isExplosifAmmo = s.IsExplosifAmmo;
                _explosionRadius = s.explosionRadius;
                _recoilChargedMultiplier =  s.recoilChargedMultiplier;
                _recoilX = s.RecoilX;
                _numberBulletInCharge = s.NumberBulletInCharged;
                _noiseAngle = s.noiseAngle;
                _maximumNoiseAngle = s.maxNoiseAngle;
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

            _gunController.RecoilModule.Recoil(_gunController.ModelGun.transform, 0.25f, false, _recoilChargedMultiplier, _recoilX);
            _gunController.RecoilModule?.SetIsRecoil(true);

            ApplyShoot();
            
            ResetCharging();
        }

        private void ApplyShoot()
        {
            for (int i = 0; i < _numberBulletInCharge; i++)
            {
                Vector3 direction = 
                    new Vector3(
                        Random.Range(-_noiseAngle, _noiseAngle),
                        Random.Range(-_noiseAngle, _noiseAngle),
                        0);
                
                Vector2 radius = Random.insideUnitCircle * _shootModule.RadiusOffset;
                Vector3 _bulletOffset = new Vector3(
                    radius.x,
                    radius.y, 0
                );
                
                _ammoModule.SpawnBullet(direction, _bulletOffset, false);
                _gunController.SetAmmo(_gunController.GetCurrentAmmo() - 1, _gunController.IsInfiniteAmmo);
            }

            _gunController.PlaySound("Charged");
            _gunController?.OnStopCharging?.Invoke();
        }
    }
}