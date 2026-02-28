using UnityEngine;

namespace GunDecorator
{
    public class MultipleShootModule : GunModule, ISecondModule
    {
        [SerializeField] private string _shapeName;
        private IShootModule _shootModule;
        private ISecondModule _next;

        public void SetUpModule(IShootModule shootModule)
        {
            _shootModule = shootModule;
        }

        public void SetNext(ISecondModule next) =>  _next = next;

        public void DoAdditionnalEffect()
        {
            Debug.Log("MultipleShootModule --- " + _shapeName);
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