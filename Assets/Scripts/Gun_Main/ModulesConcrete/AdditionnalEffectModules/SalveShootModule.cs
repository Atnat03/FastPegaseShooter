using System.Collections;
using UnityEngine;

namespace GunDecorator
{
    public class SalveShootModule : GunModule, ISecondModule
    {
        [SerializeField][Tooltip("renseigner le module de recoil de l'arme si il y en a un")] private RecoilModule _recoilModule;
        [SerializeField][Tooltip("nombre de balles tirées en 1 clic")] private int _numberShootPerSalve = 3;
        [SerializeField][Tooltip("temps en seconde entre chaque tir en 1 salve")] private float _intervalDuration = 0.1f; 
        [SerializeField][Tooltip("Recoil appliqué à la salve")] private float _recoil = 1.5f; 
        
        private IShootModule _shootModule;
        private ISecondModule _next;

        public void SetUpModule(IShootModule shootModule) => _shootModule = shootModule;
        public void SetNext(ISecondModule next) => _next = next;

        public override void SetVariable(GunSetting setting)
        {
            if (setting is S_SalveSetting s)
            {
                _numberShootPerSalve = s.numberShootPerSalve;
                _intervalDuration = s.intervalDuration;
            }
        }
        
        public void DoAdditionnalEffect()
        {
            StartCoroutine(MultipleShoot());
        }

        IEnumerator MultipleShoot()
        {
            _gunController.p_authorizedToShoot = false;
            for (int i = 0; i < _numberShootPerSalve; i++)
            {
                Shooting();
                _recoilModule?.Recoil(_gunController.ModelGun.transform, _shootModule.FireRate, true, _recoil);
                yield return new WaitForSeconds(_intervalDuration);
            }
            _gunController.p_authorizedToShoot = true;
        }

        public void Shooting()
        {
            if (_next != null)
                _next.DoAdditionnalEffect();
            else
                _shootModule.Shooting();
        }
    }
}