using UnityEngine;

namespace GunDecorator
{
    public class ExplosionAmmoModule : GunModule, ISecondModule
    {
        public GameObject bulletPrefab;
        private IShootModule _shootModule;
        private ISecondModule _next;

        public void SetUpModule(IShootModule shootModule) => _shootModule = shootModule;
        public void SetNext(ISecondModule next) => _next = next;

        public void DoAdditionnalEffect() => Shooting();

        public void Shooting()
        {
            Debug.Log("Explosive bullet fired!");
            Instantiate(bulletPrefab);

            _next?.Shooting();
        }
    }
}