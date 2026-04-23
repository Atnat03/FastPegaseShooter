using System;
using System.Collections.Generic;
using FishNet.Object;
using MyPrint;
using ScriptableObjectsDefinitions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GunDecorator
{
    public class ShootModule : GunModule, IShootModule
    {
        public float FireRate => _realFireRate;
        public IAmmoModule AmmoModule => _ammoModule;
        public float RadiusOffset => _radiusOffset;
        public bool CanShoot => _canShoot;

        [SerializeField][Tooltip("liste de l'ensemble des modificateurs appliqués au tir (l'ordre eut changer le comportement du tir)")] protected MonoBehaviour[] _secondModule;
        List<ISecondModule> _additionalEffectModule;
        
        [SerializeField][Tooltip("type de la balle tirée par l'ensemble du module")] MonoBehaviour _ammoType;
        protected IAmmoModule _ammoModule;

        [SerializeField][Tooltip("si '_isFullAuto' est actif, détermine l'interval en seconde entre deux tirs")]private float _fireRate;
        private float _realFireRate;
        
        [SerializeField][Tooltip("le nombre de balles tirées a chaque tir")] private float _numberBulletSpread = 1;
        [SerializeField, Range(0, 60)][Tooltip("l'angle de propagation maximum que les balles peuvent prendre par rapport a l'orientation du canon")] private float _spreadAngle = 1;
        [SerializeField, Range(0, 0.5f)][Tooltip("l'angle de propagation maximum que les balles peuvent prendre par rapport a l'orientation du canon")] private float _radiusOffset = 0f;
        
        [SerializeField] Transform DEBUG_spawnPoint;
        
        private BulletData _currentBulletConfig;
        private Vector3 _directionModifier = Vector3.zero;
        private Vector3 _bulletOffset = Vector3.zero;
        
        float _elapsedFireTime = 0;
        private bool _canShoot = true;

        public override void SetVariable(GunSetting setting)
        {
            if (setting is TemplateShootSetting s)
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

            _realFireRate = _fireRate;

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
                for (int i = 0; i < _numberBulletSpread; i++)
                {
                    if(_directionModifier == Vector3.zero)
                    {
                        _directionModifier = new Vector3(
                            Random.Range(-_spreadAngle, _spreadAngle),
                            Random.Range(-_spreadAngle, _spreadAngle),
                            0);
                    }
                    
                    Vector2 radius = Random.insideUnitCircle * _radiusOffset;
                    _bulletOffset = new Vector3(
                        radius.x,
                        radius.y, 0
                        );
                    
                    _ammoModule.SpawnBullet(_directionModifier, _bulletOffset);
                    
                    _directionModifier = Vector3.zero;
                }
                
                _ammoModule.ResetBulletData();

                _gunController.PlaySound("Shoot");
            }
        }

        public void CancelShooting()
        {
            _additionalEffectModule[0].CancelShooting();
        }
        
        public void SetDirectionModifier(Vector3 direction) => _directionModifier = direction;
        
        public void SetBulletOffset(Vector3 offset) =>_bulletOffset = offset;
        public void SetFireRate(float fireRateMultiplier)
        {
            if (Mathf.Approximately(fireRateMultiplier, -1))
            {
                _realFireRate = _fireRate;
            }
            else
            {
                _realFireRate = _fireRate / fireRateMultiplier;
            }
        }
            
        private void OnDrawGizmosSelected()
        {
            if(DEBUG_spawnPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(DEBUG_spawnPoint.position, _radiusOffset);
            }
        }
    }
}