using FishNet.Object;
using UnityEngine;

namespace GunDecorator
{
    public interface IShootModule
    {
        public void TryShoot();
        public void Shooting();

        public void CancelShooting();
        
        public float FireRate { get; }
        public bool CanShoot { get; }
        
        public IAmmoModule AmmoModule { get; }
        
        
        public void SetDirectionModifier(Vector3 direction);
        public void SetBulletOffset(Vector3 offset);
        void SetFireRate(float fireRateMultiplier);
    }
    
    public interface IReloadModule
    {
        public void Reload();
        public int CurrentAmmo { get; }
        public bool AutoReload { get; }
        
        public bool IsReloading { get; }
        public void SetAmmo(int value, bool infiniteAmmo);
        public void StopReload();
    }

    public interface IRecoilModule
    {
        public void Recoil(Transform model, float time, bool isFullAuto, float multiplier = 1, float newX = 1);
        public void SetIsRecoil(bool value);
    }

    public interface IAmmoModule
    {
        public void SpawnBullet(Vector3 direction, Vector3 offset, bool hadCharged = true);
        public void SetDamage(float multiplierDmg);
        public void SetBulletData(BulletData data);
        public void ResetBulletData();
    }

    public class BulletData
    {
        public bool IsExplosive { get; set; }
        public bool IsCritical { get; set; }
        public float ExplosionRadius { get; set; }
    }
    
    public interface IAmmoExplosif
    {
        public void Explosed(float raduis, int damage);
        public void SetUpVariables(float damage, float speed, GameObject markPrefab, bool isExplosive, 
            float explosionRadius, GunController gun, bool isCritical, Vector3 targetPoint, NetworkObject target, 
            bool isPositive, bool hadCharged = true);
    }

    public interface ISecondModule
    {
        public void SetUpModule(IShootModule shootModule);
        public void SetNext(ISecondModule next);
        public void DoAdditionnalEffect();
        public void Shooting();
        public void CancelShooting();
    }

    public interface IHitMarkerModule
    {
        public void HitMark();
        public void HitMarkCritique();
    }
}