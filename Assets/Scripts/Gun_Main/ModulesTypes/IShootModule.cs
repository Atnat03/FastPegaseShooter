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
    }
    
    public interface INoiseModule
    {
        public void ApplyNoise();
    }

    public interface IAmmoModule
    {
        public GameObject AmmoPrefab { get; }
        public void SpawnBullet();
    }

    public interface ISecondModule
    {
        public void SetUpModule(IShootModule shootModule);
        public void SetNext(ISecondModule next);
        public void DoAdditionnalEffect();
        public void Shooting();
    }
}