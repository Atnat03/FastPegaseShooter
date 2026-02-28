using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GunDecorator
{
    public class SalveShootModule : GunModule, ISecondModule
    {
        public int numberShootPerSalve = 3;
        private IShootModule _shootModule;
        private ISecondModule _next;

        public void SetUpModule(IShootModule shootModule) => _shootModule = shootModule;
        public void SetNext(ISecondModule next) => _next = next;

        public void DoAdditionnalEffect()
        {
            for (int i = 0; i < numberShootPerSalve; i++)
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