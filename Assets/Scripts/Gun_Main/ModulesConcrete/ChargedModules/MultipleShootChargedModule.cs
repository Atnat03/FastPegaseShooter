using MyPrint;
using UnityEngine;

namespace GunDecorator.ChargedModules
{
    public class MultipleShootChargedModule : ChargedParentModule
    {
        [Header("Salve")] 
        [SerializeField] private Vector3[] _posOffset;
        
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
                _posOffset = s._posOffset;
            }
        }
        
        public override void TryShootCharging()
        {
            base.TryShootCharging();
            
                Cons.Print("Apply shoot : " + _posOffset.Length);
            for (int i = 0; i < _posOffset.Length; i++)
            {
                _ammoModule.SetBulletData(new BulletData
                {
                    IsExplosive = _isExplosifAmmo,
                    IsCritical = _gunController.IsOverload,
                    ExplosionRadius = _explosionRadius
                });

                ApplyShoot(i);
                
                _gunController.RecoilModule.Recoil(_gunController.CurrentModelGun.transform, 0.25f, false, _recoilChargedMultiplier, _recoilX);
                _gunController.RecoilModule?.SetIsRecoil(true);
            }
            
            _gunController.PlaySound("Charged");
        }

        private void ApplyShoot(int index)
        {
            _ammoModule.SpawnBullet(Vector3.zero, _posOffset[index], false);
        }
    }
}