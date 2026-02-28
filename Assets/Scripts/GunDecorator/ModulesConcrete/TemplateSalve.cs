using UnityEngine;

namespace GunDecorator
{
    public class TemplateSalve : MonoBehaviour, ISecondModule
    {
        public float numberShootPerSalve;
        private IShootModule _shootModule;

        public void SetUpModule(IShootModule shootModule)
        {
            _shootModule = shootModule;
        }

        public void DoAdditionnalEffect()
        {
            for (int i = 1; i < numberShootPerSalve; i++)
            {
                _shootModule.Shoot();
            }
        }
    }
}