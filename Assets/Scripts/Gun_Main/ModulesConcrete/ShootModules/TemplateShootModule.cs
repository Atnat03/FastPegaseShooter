using System;
using System.Collections.Generic;
using FishNet.Object;
using MyPrint;
using ScriptableObjectsDefinitions;
using UnityEngine;

namespace GunDecorator
{
    public class TemplateShootModule : GunModule, IShootModule
    {
        public float FireRate => _fireRate;
        public IAmmoModule AmmoModule => _ammoModule;
        
        public bool CanShoot => _canShoot;

        [SerializeField][Tooltip("liste de l'ensemble des modificateurs appliqués au tir (l'ordre eut changer le comportement du tir)")] protected MonoBehaviour[] _secondModule;
        List<ISecondModule> _additionalEffectModule;
        
        [SerializeField][Tooltip("type de la balle tirée par l'ensemble du module")] MonoBehaviour _ammoType;
        protected IAmmoModule _ammoModule;

        [SerializeField][Tooltip("si '_isFullAuto' est actif, détermine l'interval en seconde entre deux tirs")]private float _fireRate;
        
        private BulletData _currentBulletConfig;
        private Vector3 _directionModifier = Vector3.zero;
        private Vector3 _bulletOffset = Vector3.zero;
        
        float _elapsedFireTime = 0;
        private bool _canShoot = true;

        public override void SetVariable(GunSetting setting)
        {
            if (setting is TemplateShootSetting shootSetting)
            {
                _fireRate = shootSetting.fireRate;
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

        private void Update()
        {
            if (_elapsedFireTime > 0)
            {
                _elapsedFireTime -= Time.deltaTime;
                
                _canShoot = false;
            }
            else
            {
                _canShoot = true;
            }
        }

        public void TryShoot()
        {
            _elapsedFireTime = FireRate;
            
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
                _ammoModule.SpawnBullet(_directionModifier, _bulletOffset);
                
                _ammoModule.ResetBulletData();
            }

            _gunController.PlaySound("Shoot");
        }
        
        public void CancelShooting()
        { }
        
        public void SetDirectionModifier(Vector3 direction) =>_directionModifier = direction;
        
        public void SetBulletOffset(Vector3 offset) =>_bulletOffset = offset;
        
    }
}