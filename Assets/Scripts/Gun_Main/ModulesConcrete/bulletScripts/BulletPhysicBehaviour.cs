using FishNet;
using FishNet.Object;
using GunDecorator;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletPhysicBehaviour : MonoBusListener, IAmmoExplosif
{
    [HideInInspector] public float p_damage;
    [HideInInspector] public float p_speed;
    [HideInInspector] public bool p_isExplosive;
    [HideInInspector] public float p_explosionRadius;
    [HideInInspector] public bool p_isCritical;
    [SerializeField] private GameObject _explosionVFX;
    private GunController _gunController;
    
    private bool _hasHit = false;
    RaycastHit hit;
    
    private Vector3 _lastPosition;

    public void SetUpVariables(float damage, float speed, GameObject markPrefab, bool isExplosive, 
        float explosionRadius, GunController gun, bool isCritical, Vector3 targetPoint, NetworkObject target)
    {
        p_damage = damage;
        p_speed = speed;
        p_isExplosive = isExplosive;
        p_explosionRadius = explosionRadius;
        _gunController = gun;
        p_isCritical = isCritical;
    }

    void Start()
    {
        _lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        DetectCollision();
        _lastPosition = transform.position;
    }

    private void DetectCollision()
    {
        Vector3 direction = transform.position - _lastPosition;
        float distance = direction.magnitude;

        if (distance <= 0f) return;

        if (Physics.SphereCast(_lastPosition, 0.15f, direction.normalized, out hit,
                distance, ~LayerMask.GetMask("Owner"), QueryTriggerInteraction.Ignore))
        {
            if (_hasHit) return;
            _hasHit = true;

            if (InstanceFinder.IsServerStarted)
            {
                if (p_isExplosive)
                    Explosed(_explosionVFX, p_explosionRadius, (int)p_damage);
                else
                {
                    if (hit.transform.TryGetComponent<IDamagable>(out IDamagable damagable))
                    {
                        bool crit = damagable.TakeDamage(_gunController.NetworkObject.ObjectId,  (int)p_damage, p_isCritical);
                        _gunController.TriggerHitMark(crit || p_isCritical);
                        InvokeEvent(new AddEnergyEvent
                        {
                            p_player = _gunController.Owner,
                            p_value = p_damage
                        });
                        
                        if (hit.collider.TryGetComponent<EnemyCore>(out var enemyCore))
                        {
                            enemyCore.AddCharge(_gunController.IsPositivePlayerCharge);
                        }
                    }
                }
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
                bool crit = damagable.TakeDamage(_gunController.NetworkObject.ObjectId,(int)p_damage, p_isCritical);
                _gunController.TriggerHitMark(crit || p_isCritical);
                InvokeEvent(new AddEnergyEvent
                {
                    p_player = _gunController.Owner,
                    p_value = p_damage
                });
                
                if (hit.collider.TryGetComponent<EnemyCore>(out var enemyCore))
                {
                    enemyCore.AddCharge(_gunController.IsPositivePlayerCharge);
                }
            }
        }
    }
}