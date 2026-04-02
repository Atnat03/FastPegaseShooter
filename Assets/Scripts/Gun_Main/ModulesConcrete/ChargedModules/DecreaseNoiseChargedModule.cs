using UnityEngine;

namespace GunDecorator.ChargedModules
{
    public class DecreaseNoiseChargedModule : ChargedParentModule
    {
        [Header("Salve")] 
        [SerializeField] private AnimationCurve _noiseEvolutionCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
        [SerializeField] private float _startMaxNoiseAngle = 10;
        
        public override void SetVariable(GunSetting setting)
        {
            if (setting is ChargedDecreaseNoiseSetting s)
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
                _startMaxNoiseAngle = s.startMaxNoiseAngle;
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

                int numberBulletShoot = (int)Mathf.Lerp(_numberBulletInCharge, 0, _charginTimer / _timeToCharge);

                if (IsFullCharged)
                    ApplyShoot();
                else
                    ApplyMultipleShoot(numberBulletShoot);
            }
            
            ResetCharging();
        }

        private void ApplyShoot()
        {
            _ammoModule.SpawnBullet(Vector3.zero, Vector3.zero);
            _gunController.SetAmmo(_gunController.GetCurrentAmmo() - 1);

            _gunController.PlaySound("Charged");
        }

        private void ApplyMultipleShoot(int numberBulletShoot)
        {
            float angle = Mathf.Lerp(0, _startMaxNoiseAngle, _noiseEvolutionCurve.Evaluate(_charginTimer / _timeToCharge));
            
            for (int i = 0; i < numberBulletShoot; i++)
            {
                Vector3 direction = 
                    new Vector3(
                        Random.Range(-angle, angle),
                        Random.Range(-angle, angle),
                        0);
                
                _ammoModule.SpawnBullet(direction, Vector3.zero);
                _gunController.SetAmmo(_gunController.GetCurrentAmmo() - 1);
            }

            _gunController.PlaySound("Charged");
        }
    }
}