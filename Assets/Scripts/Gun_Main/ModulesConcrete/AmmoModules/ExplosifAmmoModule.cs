using UnityEngine;

namespace GunDecorator.AmmoModules
{
    public class ExplosifAmmoModule : GunModule, IAmmoModule
    {
        public GameObject AmmoPrefab => _ammoPrefab;

        [SerializeField] private GameObject _ammoPrefab;
        
        public void SpawnBullet()
        {
            Debug.Log("Bullet fired");
            Instantiate(AmmoPrefab, transform.position, Quaternion.identity);
        }
    }
}