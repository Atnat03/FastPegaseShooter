using FishNet.Object;
using UnityEngine;

namespace GunDecorator.AmmoModules
{
    public class PhysicAmmoModule : GunModule, IAmmoModule
    {
        public GameObject AmmoPrefab => _ammoPrefab;

        [SerializeField] private GameObject _ammoPrefab;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Camera _camera;
        
        [SerializeField] private float _bulletMass = 1;
        [SerializeField] private float _damages;
        [SerializeField] private float _bulletThrowForce = 2000;
        [SerializeField] private float _BulletSpeed = 50;
        
        private BulletData _bulletData;
        
        public void SpawnBullet(Vector3 direction, Vector3 offset)
        {
            bool isExplosive = _bulletData != null && _bulletData.IsExplosive;
            float radius = _bulletData?.ExplosionRadius ?? 0f;

            SpawnVisualBulletServerRpc(direction, isExplosive, radius, offset);
        }
        
        [ServerRpc]
        private void SpawnVisualBulletServerRpc(Vector3 direction, bool isExplosive, float radius, Vector3 offset)
        {
            SpawnVisualBulletObserverRpc(direction, isExplosive, radius, offset);
        }

        [ObserversRpc]
        private void SpawnVisualBulletObserverRpc(Vector3 direction, bool isExplosive, float radius, Vector3 offset)
        {
            Vector3 baseDirection = _spawnPoint.forward;
            Vector3 spreadDirection = direction == Vector3.zero ? baseDirection : Quaternion.Euler(direction.y, direction.x, 0) * baseDirection;

            GameObject newBullet = Instantiate(AmmoPrefab, _spawnPoint.position + offset, Quaternion.LookRotation(spreadDirection));

            if (newBullet.TryGetComponent(out Rigidbody rb))
            {
                rb.mass = _bulletMass;
                rb.AddForce(spreadDirection.normalized * _bulletThrowForce, ForceMode.Impulse);
            }

            IAmmoExplosif bullet = newBullet.GetComponent<IAmmoExplosif>();
            bullet.SetUpVariables(_damages, _BulletSpeed, null, isExplosive, radius, _gunController);

            Destroy(newBullet, 5f);
        }

        public void SetDamage(float multiplierDmg)
        {
            _damages *= multiplierDmg;
        }

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