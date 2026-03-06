using System.Collections;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace GunDecorator.AmmoModules
{
    public class DefaultRaycastAmmoModuleRealTimeObstacleDetection : GunModule , IAmmoModule
    {
        #region variables
        
        [Header("references")]
        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject BulletPrefab;
        [SerializeField] private Transform _spawnPoint;

        [Header("parametres")]
        [SerializeField] private float _maxDistance;
        [SerializeField] private float _damages;
        [SerializeField] private float _BulletSpeed = 50;
        
        [Header("Debug")]
        public GameObject p_markPrefab;
        
        //privates
        private Transform _camTransform;
        
        #endregion

        void Start()
        {
            _camTransform = _camera.transform;
        }


        private Vector3 bulletDirection;
        private float travelTime;
        public void SpawnBullet()
        {
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            
            Vector3 targetPoint;
            
            NetworkObject damagableObject = null;

            if (Physics.Raycast(ray, out hit, _maxDistance, ~LayerMask.GetMask("Owner", "Other")))
            {
                targetPoint = hit.point;
                if (hit.collider.TryGetComponent<NetworkObject>(out NetworkObject iDamagable))
                {
                    damagableObject = iDamagable;
                }
            }
            else
            {
                targetPoint = ray.GetPoint(_maxDistance);
            }

            bulletDirection = (targetPoint - _spawnPoint.position).normalized;
            travelTime = Vector3.Distance(_spawnPoint.position, targetPoint) / _BulletSpeed;
            
            if (damagableObject != null)
                ApplyDamageServerRpc(damagableObject);

            SpawnVisualBulletServerRpc(bulletDirection, travelTime);
        }
        
        [ServerRpc]
        private void ApplyDamageServerRpc(NetworkObject target)
        {
            if (target.TryGetComponent<IDamagable>(out IDamagable damagable))
            {
                damagable.TakeDamage((int)_damages);
            }
        }

        [ServerRpc]
        private void SpawnVisualBulletServerRpc(Vector3 direction, float travel)
        {
            SpawnVisualBulletObserverRpc(direction, travel);
        }

        [ObserversRpc]
        private void SpawnVisualBulletObserverRpc(Vector3 direction, float travel)
        {
            GameObject newBullet = Instantiate(BulletPrefab, _spawnPoint.position, Quaternion.LookRotation(direction));
    
            //InstanceFinder.ServerManager.Spawn(newBullet);
    
            Destroy(newBullet, travel + .5f);
            BulletBehaviour bulletBehaviour = newBullet.GetComponent<BulletBehaviour>();
    
            bulletBehaviour.p_damage = _damages;
            bulletBehaviour.p_speed = _BulletSpeed;
            bulletBehaviour.p_markPrefab = p_markPrefab;
        }

        public void SetDamage(float multiplierDmg)
        {
            _damages *= multiplierDmg;
        }
    }
}
