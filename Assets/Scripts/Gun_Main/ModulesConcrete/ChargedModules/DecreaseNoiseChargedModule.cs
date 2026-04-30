using UnityEngine;

namespace GunDecorator.ChargedModules
{
    public class DecreaseNoiseChargedModule : ChargedParentModule
    {
        [Header("Salve")] 
        [SerializeField] private float _startMaxNoiseAngle = 10;
        
        public override void SetVariable(GunSetting setting)
        {
            if (setting is ChargedDecreaseNoiseSetting s)
            {
                _damageChargedMultiplicator = s._damageChargedMultiplicator;
                _isExplosifAmmo = s.IsExplosifAmmo;
                _explosionRadius = s.explosionRadius;
                _recoilChargedMultiplier =  s.recoilChargedMultiplier;
                _recoilX = s.RecoilX;
                _numberBulletInCharge = s.NumberBulletInCharged;
                _startMaxNoiseAngle = s.startMaxNoiseAngle;
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
            _ammoModule.SpawnBullet(Vector3.zero, Vector3.zero);

            _gunController.PlaySound("Charged");
        }
    }
}