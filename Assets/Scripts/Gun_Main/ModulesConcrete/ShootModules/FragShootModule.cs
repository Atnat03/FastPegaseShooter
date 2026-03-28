using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;

namespace GunDecorator
{
    public class FragShootModule : GunModule, IShootModule
    {
        public float FireRate => _fireRate;
        public IAmmoModule AmmoModule => _ammoModule;
        public bool IsExplosed => _isExplosedAmmo;

        [SerializeField][Tooltip("liste de l'ensemble des modificateurs appliqués au tir (l'ordre eut changer le comportement du tir)")] protected MonoBehaviour[] _secondModule;
        List<ISecondModule> _additionalEffectModule;
        
        [SerializeField][Tooltip("type de la balle tirée par l'ensemble du module")] MonoBehaviour _ammoType;
        protected IAmmoModule _ammoModule;

        [SerializeField][Tooltip("si '_isFullAuto' est actif, détermine l'interval en seconde entre deux tirs")]private float _fireRate;
        
        [SerializeField][Tooltip("le nombre de balles tirées a chaque tir")] private float _numberBulletSpread;
        [SerializeField, Range(0, 60)][Tooltip("l'angle de propagation maximum que les balles peuvent prendre par rapport a l'orientation du canon")] private float _spreadAngle;
        private bool _isExplosedAmmo = false;
        
        public override void SetVariable(GunSetting setting)
        {
            if (setting is FragShootSetting s)
            {
                _fireRate = s.fireRate;
                _numberBulletSpread = s.numberBulletSpread;
                _spreadAngle = s.SpreadAngle;
            }
        }
        
        private void Start()
        {
            _additionalEffectModule = new List<ISecondModule>();
            
            foreach (MonoBehaviour module in _secondModule)
            {
                ISecondModule secondModule = (ISecondModule)module;

                if(secondModule != null)
                {
                    secondModule.SetUpModule(this);
                    _additionalEffectModule.Add(secondModule);
                }
            }

            if(_ammoType != null)
                _ammoModule = (IAmmoModule)_ammoType;
        }

        public void TryShoot()
        {
            if (_additionalEffectModule.Count == 0)
            {
                Shooting();
                return;
            }

            for (int i = 0; i < _additionalEffectModule.Count - 1; i++)
                _additionalEffectModule[i].SetNext(_additionalEffectModule[i + 1]);

            _additionalEffectModule[^1].SetNext(null);

            _additionalEffectModule[0].DoAdditionnalEffect();
        }

        public void Shooting()
        {
            if (_ammoModule != null)
            {
                for (int i = 0; i < _numberBulletSpread; i++)
                {
                    Vector3 direction = 
                            new Vector3(
                                Random.Range(-_spreadAngle, _spreadAngle),
                                Random.Range(-_spreadAngle, _spreadAngle),
                                0);
                    
                    _ammoModule.SpawnBullet(direction, Vector3.zero);
                }
                
                _ammoModule.ResetBulletData();

                _gunController.PlaySound("Shoot");
            }
        }

        public void CancelShooting()
        { }
        
        public void SetDirectionModifier(Vector3 direction)
        {
            throw new System.NotImplementedException();
        }

        public void SetBulletOffset(Vector3 offset)
        {
            throw new System.NotImplementedException();
        }
    }
}