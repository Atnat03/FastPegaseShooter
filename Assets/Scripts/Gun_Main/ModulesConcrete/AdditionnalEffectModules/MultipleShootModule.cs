using UnityEngine;

namespace GunDecorator
{
    public class MultipleShootModule : GunModule, ISecondModule
    {
        private IShootModule _shootModule;
        private ISecondModule _next;

        [SerializeField] private Transform[] _shootingStartPoint;

        public void SetUpModule(IShootModule shootModule)=>_shootModule = shootModule;
        public void SetNext(ISecondModule next) =>  _next = next;

        
        public void DoAdditionnalEffect()
        {
            foreach (Transform t in _shootingStartPoint)
            {
                _shootModule.SetDirectionModifier(t.forward);
                _shootModule.SetBulletOffset(t.localPosition);
                Shooting();
            }
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