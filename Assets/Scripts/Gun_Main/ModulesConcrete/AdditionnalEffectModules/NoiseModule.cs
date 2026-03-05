using System.Linq;
using UnityEngine;

namespace GunDecorator
{
    public class NoiseModuleModule : GunModule, ISecondModule
    {
        private IShootModule _shootModule;
        private ISecondModule _next;

        public void SetUpModule(IShootModule shootModule)
        {
            _shootModule = shootModule;
        }

        public void SetNext(ISecondModule next) =>  _next = next;

        public void DoAdditionnalEffect()
        {
            Debug.Log("plein de noise omg");
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