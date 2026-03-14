using FishNet.Object;
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

        void Start()
        {
            _dmgToApply = _damages;
        }
        
        public void SpawnBullet(Vector3 direction, Vector3 offset)
        {
            bool isExplosive = _bulletData != null && _bulletData.IsExplosive;
            float radius = _bulletData?.ExplosionRadius ?? 0f;
            bool isCritical = _bulletData?.IsCritical ?? _gunController.IsOverload;

            SpawnVisualBulletServerRpc(direction, isExplosive, radius, offset, isCritical);
        }
        
        [ServerRpc]
        private void SpawnVisualBulletServerRpc(Vector3 direction, bool isExplosive, float radius, Vector3 offset, bool isCritical)
        {
            SpawnVisualBulletObserverRpc(direction, isExplosive, radius, offset, isCritical);
        }

        [ObserversRpc]
        private void SpawnVisualBulletObserverRpc(Vector3 direction, bool isExplosive, float radius, Vector3 offset, bool isCritical)
        {
            Vector3 baseDirection = _spawnPoint.forward;
            Vector3 spreadDirection = direction == Vector3.zero ? baseDirection : Quaternion.Euler(direction.y, direction.x, 0) * baseDirection;

            GameObject newBullet = Instantiate(AmmoPrefab, _spawnPoint.position + offset, Quaternion.LookRotation(spreadDirection));

            if (newBullet.TryGetComponent(out Rigidbody rb))
            {
                rb.mass = _bulletMass;
                rb.AddForce(spreadDirection.normalized * _bulletThrowForce, ForceMode.Impulse);
            }
            
            Vector3 targetPos = _spawnPoint.position + _spawnPoint.forward * 2000f;

            IAmmoExplosif bullet = newBullet.GetComponent<IAmmoExplosif>();
            bullet.SetUpVariables(_dmgToApply, _BulletSpeed, null, isExplosive, radius, _gunController, isCritical, targetPos);

            Destroy(newBullet, 5f);
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