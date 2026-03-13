using System.Linq;
using UnityEngine;

namespace GunDecorator
{
    public class NoiseModuleModule : GunModule, ISecondModule
    {
        private IShootModule _shootModule;
        private ISecondModule _next;
        
        [SerializeField] private float _maxOffsetX;
        [SerializeField] private float _maxOffsetY;

        public void SetUpModule(IShootModule shootModule)
        {
            _shootModule = shootModule;
        }

        public void SetNext(ISecondModule next) =>  _next = next;

        public void DoAdditionnalEffect()
        {
            _shootModule.SetDirectionModifier(new Vector3(Random.Range(-_maxOffsetX, _maxOffsetX), Random.Range(-_maxOffsetY, _maxOffsetY), 1));
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