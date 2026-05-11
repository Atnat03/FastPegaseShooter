using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using MyPrint;
using UnityEngine;

namespace GunDecorator.AmmoModules
{
    public class PhysicAmmoModule : GunModule, IAmmoModule
    {
        public GameObject AmmoPrefab => _ammoPrefab;

        [Header("References")]
        [SerializeField, Tooltip("Prefab de la balle qui sera instanciée lors du tir.")]
        private GameObject _ammoPrefab;

        [SerializeField, Tooltip("Point de spawn de la balle (généralement à l'extrémité du canon).")]
        private Transform _spawnPoint;

        [SerializeField, Tooltip("Caméra du joueur utilisée pour déterminer la direction du tir.")]
        private Camera _camera;
        
        [Header("Bullet Settings")]
        [SerializeField, Tooltip("Masse physique appliquée au Rigidbody de la balle.")]
        private float _bulletMass = 1;

        [SerializeField, Tooltip("Dégâts de base infligés par la balle.")]
        private float _damages = 2;

        [SerializeField, Tooltip("Force initiale appliquée à la balle lors du tir.")]
        private float _bulletThrowForce = 100;

        [SerializeField, Tooltip("Vitesse logique de la balle utilisée par certains systèmes (ex: calculs d'impact ou trajectoire).")]
        private float _BulletSpeed = 50;
        
        private float _dmgToApply = 0;
        
        private BulletData _bulletData;
        
        private Vector3 _finalSpawnPoint;
        
        private Pooler<BulletPhysicBehaviour> _ammoPool;

        public override void SetVariable(GunSetting setting)
        {
            if (setting is PhysicAmmoSetting s)
            {
                _bulletMass = s.mass;
                _damages =  s.damages;
                _bulletThrowForce = s.bulletThrowForce;
                _BulletSpeed = s.bulletSpeed;
            }
        }
        
        void Start()
        {
            _dmgToApply = _damages;
            _ammoPool = new Pooler<BulletPhysicBehaviour>(_ammoPrefab.GetComponent<BulletPhysicBehaviour>(), 10);
        }
        
        public void SpawnBullet(Vector3 direction, Vector3 offset, bool hadCharged)
        {
            bool isExplosive = _bulletData != null && _bulletData.IsExplosive;
            float radius = _bulletData?.ExplosionRadius ?? 0f;
            bool isCritical = _bulletData?.IsCritical ?? _gunController.IsOverload;
            
            Ray cameraRay;
            Vector3 spreadDirection;
            
            if (direction != Vector3.zero)
            {
                spreadDirection = _camera.transform.rotation 
                                  * Quaternion.Euler(direction.y, direction.x, 0) 
                                  * Vector3.forward;
                cameraRay = new Ray(_camera.transform.position, spreadDirection);
            }
            else
            {
                cameraRay = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            }
            
            RaycastHit hit;
            _finalSpawnPoint = _spawnPoint.position;
            Vector3 camPos = _camera.transform.position;
            Vector3 targetPoint;
            
            if (Physics.Raycast(cameraRay, out hit, 2000, ~LayerMask.GetMask("Owner", "Other"), QueryTriggerInteraction.Ignore))
            {
                targetPoint = hit.point;

                if ((targetPoint - camPos).sqrMagnitude < (_finalSpawnPoint - camPos).sqrMagnitude)
                {
                    _finalSpawnPoint = targetPoint - (_spawnPoint.forward * 0.5f);
                }
            }
            
            SpawnVisualBulletServerRpc(direction, isExplosive, radius, offset, isCritical, _finalSpawnPoint, hadCharged);
        }
        
        [ServerRpc]
        private void SpawnVisualBulletServerRpc(Vector3 direction, bool isExplosive, float radius, Vector3 offset, bool isCritical, Vector3 spawnPoint, bool hadCharged)
        {
            SpawnVisualBulletObserverRpc(direction, isExplosive, radius, offset, isCritical, spawnPoint, hadCharged);
        }
        
        [ObserversRpc]
        private void SpawnVisualBulletObserverRpc(Vector3 direction, bool isExplosive, float radius, Vector3 offset, bool isCritical, Vector3 spawnPoint, bool hadCharged)
        {
            Vector3 baseDirection = _spawnPoint.forward;
            Vector3 spreadDirection = direction == Vector3.zero ? baseDirection : Quaternion.Euler(direction.y, direction.x, 0) * baseDirection;
    
            BulletPhysicBehaviour newBullet = _ammoPool.Spawn(spawnPoint + offset, Quaternion.LookRotation(spreadDirection)); // ← utiliser spawnPoint // équivalent de instanciate
            newBullet.OnCollision += DespawnBullet;
            if (newBullet.TryGetComponent(out Rigidbody rb))
            {
                rb.mass = _bulletMass;
                rb.AddForce(spreadDirection.normalized * _bulletThrowForce, ForceMode.Impulse);
            }
    
            Vector3 targetPos = spawnPoint + _spawnPoint.forward * 2000f; // ← utiliser spawnPoint

            IAmmo bullet = newBullet.GetComponent<IAmmo>();
            bullet.SetUpVariables(_dmgToApply, _BulletSpeed, null, isExplosive, radius, _gunController, 
                isCritical, targetPos, null, _gunController.IsPositivePlayerCharge, hadCharged);

            DespawnBullet(newBullet, 5f);//équivalent du destroy
        }

        void DespawnBullet(BulletPhysicBehaviour bullet, float delay)
        {
            StartCoroutine(DespawnBulletCoroutine( bullet, delay));
        }
        void DespawnBullet(BulletPhysicBehaviour bullet)
        {
            bullet.OnCollision -= DespawnBullet;
            _ammoPool.ReturnToPool(bullet);
        }

        IEnumerator DespawnBulletCoroutine(BulletPhysicBehaviour bullet, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (bullet != null && bullet.gameObject != null && bullet.gameObject.activeSelf)
            {
                bullet.OnCollision -= DespawnBullet;
                _ammoPool.ReturnToPool(bullet);
            }
        }

        public void SetDamage(float multiplierDmg) => _dmgToApply = _damages * multiplierDmg;

        public void SetBulletData(BulletData data)
        {
            _bulletData = data;
        }

        public void ResetBulletData()
        {
            _bulletData = null;
        }
    }
}