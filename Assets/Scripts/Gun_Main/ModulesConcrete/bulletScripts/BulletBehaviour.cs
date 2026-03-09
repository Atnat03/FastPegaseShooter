using FishNet.Object;
using GunDecorator;
using NUnit.Framework.Constraints;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour, IAmmoExplosif
{
    [HideInInspector] public float p_damage;
    [HideInInspector] public float p_speed;
    [HideInInspector] public GameObject p_markPrefab;
    [HideInInspector] public bool p_isExplosive;
    [HideInInspector] public float p_explosionRadius;
    [SerializeField] private GameObject _explosionVFX;
    private GunController _gunController;
    
    private bool _hasHit = false;

    private void FixedUpdate()
    {
        DetectCollision();
        Move();
    }

    public void SetUpVariables(float damage, float speed, GameObject markPrefab, bool isExplosive, float explosionRadius, GunController gun)
    {
        p_damage = damage;
        p_speed = speed;
        p_markPrefab = markPrefab;
        p_isExplosive = isExplosive;
        p_explosionRadius = explosionRadius;
        _gunController = gun;
    }

    private void Move()
    {
        transform.Translate(transform.forward * (p_speed * Time.deltaTime), Space.World);
    }

    private void DetectCollision()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit,
                p_speed * Time.fixedDeltaTime, ~LayerMask.NameToLayer("Owner"), QueryTriggerInteraction.Ignore))
        {
            if (p_isExplosive)
                Explosed(_explosionVFX, p_explosionRadius, (int)p_damage);
            else
            {
                Destroy(Instantiate(p_markPrefab, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal)), 3f);
            }
            
            Destroy(gameObject);
        }
    }

    public void Explosed(GameObject vfx, float radius, int damage)
    {
        if (vfx != null)
            Instantiate(vfx, transform.position, Quaternion.identity);
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider c in colliders)
        {
            if (c.TryGetComponent<IDamagable>(out IDamagable damagable))
            {
                damagable.TakeDamage(damage, _gunController.IsOverload);
                _gunController.TriggerHitMark(_gunController.IsOverload);
            }
        }
    }
}