using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using MyPrint;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace GunDecorator.AmmoModules
{
    public class RaycastAmmoModule : GunModule, IAmmoModule
    {
        [Header("references")]
        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject BulletPrefab;
        [SerializeField] private Transform _spawnPoint;
        
        [Header("parametres")]
        [SerializeField][Tooltip("disance maximum que les balles peuvent parcourir avant de disparaitre")] private float _maxDistance = 2000;
        [SerializeField][Tooltip("nombre de dommages que chaque balle va infliger a la cible")] private float _damages = 5;
        [SerializeField][Tooltip("vitesse de la balle en Unité/secondes")] private float _BulletSpeed = 50;
        private float _dmgToApply = 1;

        [SerializeField, Tooltip("scriptable object des vfx en fonction de la surface touché")] ImpactBulletSO _impactVFXData;
        
        [SerializeField] private bool _isDistanceReduced = false;
        [SerializeField] private float _factorReduceDamageByDistance = 1;

        private BulletData _bulletData;
        
        private Vector3 bulletDirection;
        private float travelTime;
        
        private Vector3 _finalSpawnPoint;
        
        private Pooler<BulletBehaviour> _ammoPool;
        
        private Dictionary<BulletBehaviour, Coroutine> bulletsLifetime =  new Dictionary<BulletBehaviour, Coroutine>();
        
        public override void SetVariable(GunSetting setting)
        {
            if (setting is RaycastAmmoSetting s)
            {
                _maxDistance = s.maxDistance;
                _damages = s.damages;
                _BulletSpeed = s.bulletSpeed;
                _isDistanceReduced = s.isDistanceReduced;
                _factorReduceDamageByDistance = s.factorReduceDamageByDistance;
            }
        }
        
        void Start()
        {
            _dmgToApply = _damages;
            _ammoPool = new Pooler<BulletBehaviour>(BulletPrefab.GetComponent<BulletBehaviour>(), 100);
        }

        public void SpawnBullet(Vector3 direction, Vector3 offset, bool hadCharged = true)
        {
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
                spreadDirection = cameraRay.direction;
            }

            RaycastHit hit;
            Vector3 targetPoint;
            NetworkObject damagableObject = null;
            string touchTag = "Default";
            _finalSpawnPoint = _spawnPoint.position;
            Vector3 camPos = _camera.transform.position;
            
            if (Physics.Raycast(cameraRay, out hit, _maxDistance, ~LayerMask.GetMask("Owner", "Other"), QueryTriggerInteraction.Ignore))
            {
                targetPoint = hit.point;
                touchTag = hit.collider.gameObject.tag;

                if ((targetPoint - camPos).sqrMagnitude < (_finalSpawnPoint - camPos).sqrMagnitude)
                {
                    _finalSpawnPoint = targetPoint;
                }

                if (hit.collider.TryGetComponent<NetworkObject>(out NetworkObject iDamagable))
                    damagableObject = iDamagable;
            }
            else
            {
                targetPoint = cameraRay.GetPoint(_maxDistance);
            }
            
            bulletDirection = spreadDirection.normalized;
            travelTime = Vector3.Distance(_finalSpawnPoint, targetPoint) / _BulletSpeed;

            bool isExplosive = _bulletData != null && _bulletData.IsExplosive;
            float radius = _bulletData?.ExplosionRadius ?? 0f;
            
            SpawnVisualBulletServerRpc(bulletDirection, travelTime, isExplosive, radius, offset, targetPoint, touchTag, _finalSpawnPoint, damagableObject, _factorReduceDamageByDistance, _isDistanceReduced, hadCharged);
        }
        
        [ServerRpc]
        private void SpawnVisualBulletServerRpc(Vector3 direction, float travel, bool isExplosive, 
            float radius, Vector3 offset, Vector3 targetPoint, string touchObjectTag, Vector3 finalPos, NetworkObject target = null, float ratio = 1, bool isDistanceReduce = false, bool hadCharged = true)
        {
            bool isCritical = _gunController.IsOverload;
            DoSpawnBullet(direction, travel, isExplosive, radius, offset, isCritical, targetPoint, touchObjectTag, finalPos, target,ratio, isDistanceReduce, hadCharged);
            SpawnVisualBulletObserverRpc(direction, travel, isExplosive, radius, offset, isCritical, targetPoint, touchObjectTag, finalPos, target, ratio, isDistanceReduce, hadCharged);
        }

        private void DoSpawnBullet(Vector3 direction, float travel, bool isExplosive,
            float radius, Vector3 offset, bool isCritical, Vector3 targetPoint, string touchObject, Vector3 finalPos,
            NetworkObject target = null, float factorReduceDamageByDistance = 1, bool isDistanceReduce = false, bool hadCharged = true)
        {
            BulletBehaviour newBullet = _ammoPool.Spawn(finalPos + offset, Quaternion.LookRotation(direction));
            Debug.Log($"ammo pool size: {_ammoPool.Size}");
            
            newBullet.OnCollision += DespawnBullet;
            DespawnBullet(newBullet, 5f);//équivalent du destroy
    
            IAmmo bullet = newBullet.GetComponent<IAmmo>();

            SurfaceType surface = ImpactSurface.GetSurfaceType(touchObject);
            GameObject vfx = _impactVFXData.GetVFXFromSurface(surface);
            
            bullet.SetUpVariables(_dmgToApply, _BulletSpeed, vfx, isExplosive, radius, _gunController,
                isCritical, targetPoint, target, _gunController.IsPositivePlayerCharge, 0,factorReduceDamageByDistance, isDistanceReduce, hadCharged);
        }

        [ObserversRpc]
        private void SpawnVisualBulletObserverRpc(Vector3 direction, float travel, bool isExplosive, 
            float radius, Vector3 offset, bool isCritical, Vector3 targetPoint, string touchObject, Vector3 finalPos, NetworkObject target = null, float ratio = 1, bool isDistanceReduce = false, bool hadCharged = true)
        {
            if (IsServerInitialized) return;
            DoSpawnBullet(direction, travel, isExplosive, radius, offset, isCritical, targetPoint, touchObject, finalPos, target,ratio, isDistanceReduce, hadCharged);
        }
        
        void DespawnBullet(BulletBehaviour bullet, float delay)
        {
            bulletsLifetime.Add(bullet,StartCoroutine(DespawnBulletCoroutine( bullet, delay)));
        }
        
        IEnumerator DespawnBulletCoroutine(BulletBehaviour bullet, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (bullet != null && bullet.gameObject != null && bullet.gameObject.activeSelf)
            {
                DespawnBullet(bullet);
            }
        }

        void DespawnBullet(BulletBehaviour bullet)
        {
            if (bulletsLifetime.ContainsKey(bullet)) 
            {
                StopCoroutine(bulletsLifetime[bullet]);
                bulletsLifetime.Remove(bullet);
            }
            
            bullet.OnCollision -= DespawnBullet;
            _ammoPool.ReturnToPool(bullet);
        }
        
        public void SetDamage(float multiplierDmg) => _dmgToApply = _damages * multiplierDmg;
        
        public void ResetBulletData()
        {
            _bulletData = null;
        }
        
        public void SetBulletData(BulletData data)
        {
            _bulletData = data;
        }
    }
}