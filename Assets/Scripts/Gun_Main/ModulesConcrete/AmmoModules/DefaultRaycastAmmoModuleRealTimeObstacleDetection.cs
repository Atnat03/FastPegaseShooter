using FishNet.Object;
using UnityEngine;

namespace GunDecorator.AmmoModules
{
    public class DefaultRaycastAmmoModuleRealTimeObstacleDetection : GunModule, IAmmoModule
    {
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

        private Transform _camTransform;
        private BulletData _bulletData;
        
        private Vector3 bulletDirection;
        private float travelTime;
        
        void Start()
        {
            _camTransform = _camera.transform;
        }

        public void SpawnBullet(Vector3 direction, Vector3 offset)
        {
            Ray cameraRay = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            RaycastHit hit;
            Vector3 targetPoint;
            NetworkObject damagableObject = null;

            if (Physics.Raycast(cameraRay, out hit, _maxDistance, ~LayerMask.GetMask("Owner", "Other")))
            {
                targetPoint = hit.point;
                if (hit.collider.TryGetComponent<NetworkObject>(out NetworkObject iDamagable))
                    damagableObject = iDamagable;
            }
            else
            {
                targetPoint = cameraRay.GetPoint(_maxDistance);
            }

            Vector3 baseDirection = (targetPoint - _spawnPoint.position).normalized;
            Vector3 spreadDirection = Quaternion.Euler(direction.y, direction.x, 0) * baseDirection;

            bulletDirection = spreadDirection.normalized;
            travelTime = Vector3.Distance(_spawnPoint.position, targetPoint) / _BulletSpeed;

            bool isExplosive = _bulletData != null && _bulletData.IsExplosive;
            float radius = _bulletData?.ExplosionRadius ?? 0f;
            
            if (damagableObject != null && !isExplosive)
                ApplyDamageServerRpc(damagableObject);

            SpawnVisualBulletServerRpc(bulletDirection, travelTime, isExplosive, radius, offset);
        }

        [ServerRpc]
        private void ApplyDamageServerRpc(NetworkObject target)
        {
            if (target.TryGetComponent<IDamagable>(out IDamagable damagable))
                damagable.TakeDamage((int)_damages);
        }

        [ServerRpc]
        private void SpawnVisualBulletServerRpc(Vector3 direction, float travel, bool isExplosive, float radius, Vector3 offset)
        {
            SpawnVisualBulletObserverRpc(direction, travel, isExplosive, radius, offset);
        }

        [ObserversRpc]
        private void SpawnVisualBulletObserverRpc(Vector3 direction, float travel, bool isExplosive, float radius, Vector3 offset)
        {
            GameObject newBullet = Instantiate(BulletPrefab, _spawnPoint.position + offset, Quaternion.LookRotation(direction));
            Destroy(newBullet, travel + .5f);

            IAmmoExplosif bullet = newBullet.GetComponent<IAmmoExplosif>();
            bullet.SetUpVariables(_damages, _BulletSpeed, p_markPrefab, isExplosive, radius);
        }
        
        public void SetDamage(float multiplierDmg) => _damages *= multiplierDmg;
        
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