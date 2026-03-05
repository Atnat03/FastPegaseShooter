using UnityEngine;

namespace GunDecorator
{
    public interface IShootModule
    {
        public void TryShoot();
        public void Shooting();
    }
    
    public interface IReloadModule
    {
        public void Reload();
        public int CurrentAmmo { get; }
        public void SetAmmo(int value);
    }

    public interface IRecoilModule
    {
        public void Recoil();
    }

    public interface IAmmoModule
    {
        public void SpawnBullet();
        public void SetDamage(float multiplierDmg);
    }

    public interface ISecondModule
    {
        public void SetUpModule(IShootModule shootModule);
        public void SetNext(ISecondModule next);
        public void DoAdditionnalEffect();
        public void Shooting();
    }
}