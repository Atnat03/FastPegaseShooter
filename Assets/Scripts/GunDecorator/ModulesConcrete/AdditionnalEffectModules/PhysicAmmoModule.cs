using UnityEngine;

namespace GunDecorator
{
    public class PhysicAmmoModule : GunModule, ISecondModule
    {
        public GameObject bulletPrefab;
        private IShootModule _shootModule;

        public void SetUpModule(IShootModule shootModule)
        {
            _shootModule = shootModule;
        }

        public void DoAdditionnalEffect()
        {
            Debug.Log("PhysicAmmoModule --- DoAdditionnalEffect");
            Instantiate(bulletPrefab);
        }
    }
}