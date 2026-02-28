using UnityEngine;

namespace GunDecorator
{
    public class PhysicAmmoModule : GunModule, ISecondModule
    {
        public GameObject bulletPrefab;
        private IShootModule _shootModule;
        private ISecondModule _next;
        
        public void SetUpModule(IShootModule shootModule)
        {
            _shootModule = shootModule;
        }

        public void SetNext(ISecondModule next) => _next = next;

        public void DoAdditionnalEffect()
        {
            Debug.Log("PhysicAmmoModule --- DoAdditionnalEffect");
            Instantiate(bulletPrefab);
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