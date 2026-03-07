using UnityEngine;

namespace GunDecorator
{
    public class ExplosifAmmoModule : GunModule, ISecondModule
    {
        private IShootModule _shootModule;
        private ISecondModule _next;

        [Header("Explosion")] 
        [SerializeField] private float _radiusExplosion;

        public void SetUpModule(IShootModule shootModule)
        {
            _shootModule = shootModule;
        }

        public void SetNext(ISecondModule next) =>  _next = next;

        public void DoAdditionnalEffect()
        {
            _shootModule.AmmoModule.SetBulletData(new BulletData
            {
                IsExplosive = true,
                ExplosionRadius = _radiusExplosion,
            });

            Shooting();
        }

        public void Shooting()
        {
            if (_next != null)
                _next.Shooting();
            else
                _shootModule.Shooting();
        }
    }
}