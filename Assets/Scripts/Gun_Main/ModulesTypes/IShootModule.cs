using UnityEngine;

namespace GunDecorator
{
    public interface IShootModule
    {
        public void TryShoot();
        public void Shooting();

        public void CancelShooting();
        
        public bool IsFullAuto { get; }
        public float FireRate { get; }
        public IAmmoModule AmmoModule { get; }
    }
    
    public interface IReloadModule
    {
        public void Reload();
        public int CurrentAmmo { get; }
        public bool AutoReload { get; }
        
        public bool IsReloading { get; }
        public void SetAmmo(int value);
        public void StopReload();
    }

    public interface IRecoilModule
    {
        public void Recoil(float multiplier = 1);
    }

    public interface IAmmoModule
    {
        public void SpawnBullet(Vector3 direction, Vector3 offset);
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
        public void Explosed(GameObject vfx, float raduis, int damage);
        public void SetUpVariables(float damage, float speed, GameObject markPrefab, bool isExplosive, float explosionRadius, GunController gun);
    }

    public interface ISecondModule
    {
        public void SetUpModule(IShootModule shootModule);
        public void SetNext(ISecondModule next);
        public void DoAdditionnalEffect();
        public void Shooting();
    }

    public interface IHitMarkerModule
    {
        public void HitMark();
        public void HitMarkCritique();
    }
}