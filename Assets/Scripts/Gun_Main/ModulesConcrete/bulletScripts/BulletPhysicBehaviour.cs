using FishNet.Object;
using GunDecorator;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletPhysicBehaviour : MonoBehaviour, IAmmoExplosif
{
    [HideInInspector] public float p_damage;
    [HideInInspector] public float p_speed;
    [HideInInspector] public bool p_isExplosive;
    [HideInInspector] public float p_explosionRadius;
    [SerializeField] private GameObject _explosionVFX;
    
    private bool _hasHit = false;

    public void SetUpVariables(float damage, float speed, GameObject markPrefab, bool isExplosive, float explosionRadius)
    {
        p_damage = damage;
        p_speed = speed;
        p_isExplosive = isExplosive;
        p_explosionRadius = explosionRadius;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasHit) return;
        _hasHit = true;

        if (p_isExplosive)
            Explosed(_explosionVFX, p_explosionRadius, (int)p_damage);

        Destroy(gameObject);
    }

    public void Explosed(GameObject vfx, float radius, int damage)
    {
        if (vfx != null)
            Instantiate(vfx, transform.position, Quaternion.identity);
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider c in colliders)
        {
            if (c.TryGetComponent<IDamagable>(out IDamagable damagable))
                damagable.TakeDamage(damage);
        }
    }
}