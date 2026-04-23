using UnityEngine;

namespace GunDecorator
{
    public class ExplosifAmmoModule : GunModule, ISecondModule
    {
        private IShootModule _shootModule;
        private ISecondModule _next;

        [Header("Explosion")] 
        [SerializeField, Tooltip("Taille de la zone d'explosion")] private float _radiusExplosion;

        public override void SetVariable(GunSetting setting)
        {
            if (setting is S_ExplosifSetting s)
            {
                _radiusExplosion = s.explosionRadius;
            }
        }
        
        
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
                IsCritical = _gunController.IsOverload
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

        public void CancelShooting()
        { }
    }
}