using UnityEngine;

namespace GunDecorator
{
    public class MultipleShootModule : GunModule, ISecondModule
    {
        [SerializeField] private string _shapeName;
        private IShootModule _shootModule;

        public void SetUpModule(IShootModule shootModule)
        {
            _shootModule = shootModule;
        }

        public void DoAdditionnalEffect()
        {
            Debug.Log("MultipleShootModule --- " + _shapeName);
        }
    }
}