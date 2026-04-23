using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GunDecorator.ChargedModules
{
    public class IncreaseNoiseChargedModule : ChargedParentModule
    {
        [Header("Salve")] 
        [SerializeField] private AnimationCurve _noiseEvolutionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        [SerializeField] private float _maximumNoiseAngle = 10;
        
        public override void SetVariable(GunSetting setting)
        {
            if (setting is ChargedIncreaseNoiseSetting s)
            {
                _isExplosifAmmo = s.IsExplosifAmmo;
                _explosionRadius = s.explosionRadius;
                _deadZoneStartCharging = s.DeadZoneStartCharging;
                _recoilChargedMultiplier =  s.recoilChargedMultiplier;
                _recoilX = s.RecoilX;
                _timeToCharge = s.timeToCharge;
                _isFullMultiplicator = s.IsFullMultiplicator;
                _numberBulletInCharge = s.NumberBulletInCharged;
                _noiseEvolutionCurve = s.NoiseEvolutionCurve;
                _maximumNoiseAngle = s.maxNoiseAngle;
            }
        }
        
        public override void TryShootCharging()
        {
            if (_charging)
            {
                _ammoModule.SetBulletData(new BulletData
                {
                    IsExplosive = _isExplosifAmmo,
                    IsCritical = _gunController.IsOverload,
                    ExplosionRadius = _explosionRadius
                });

                _gunController.RecoilModule.Recoil(_gunController.ModelGun.transform, 0.25f, false, _recoilChargedMultiplier, _recoilX);
                _gunController.RecoilModule?.SetIsRecoil(true);

                ApplyShoot();
            }
            
            ResetCharging();
        }

        private void ApplyShoot()
        {
            int numberBulletShoot = (int)Mathf.Lerp(0, _numberBulletInCharge, _charginTimer / _timeToCharge);

            float angle = Mathf.Lerp(0, _maximumNoiseAngle, _noiseEvolutionCurve.Evaluate(_charginTimer / _timeToCharge));
            
            for (int i = 0; i < numberBulletShoot; i++)
            {
                Vector3 direction = 
                    new Vector3(
                        Random.Range(-angle, angle),
                        Random.Range(-angle, angle),
                        0);
                
                Vector2 radius = Random.insideUnitCircle * _shootModule.RadiusOffset;
                Vector3 _bulletOffset = new Vector3(
                    radius.x,
                    radius.y, 0
                );
                
                _ammoModule.SpawnBullet(direction, _bulletOffset);
                _gunController.SetAmmo(_gunController.GetCurrentAmmo() - 1, _gunController.IsInfiniteAmmo);
            }

            _gunController.PlaySound("Charged");
        }
    }
}