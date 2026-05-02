using FishNet;
using FishNet.Object;
using GunDecorator;
using MyPrint;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletPhysicBehaviour : MonoBusListener, IAmmoExplosif
{
    [HideInInspector] public float p_damage;
    [HideInInspector] public float p_speed;
    [HideInInspector] public bool p_isExplosive;
    [HideInInspector] public float p_explosionRadius;
    [HideInInspector] public bool p_isCritical;
    private GunController _gunController;
    
    [SerializeField] private GameObject _positiveExplosionVFX;
    [SerializeField] private GameObject _negativeExplosionVFX;
    
    [Header("View")]    
    [SerializeField]private MeshRenderer _meshRenderer;
    [SerializeField]private Material _positiveMaterial;
    [SerializeField] private Material _negativeMaterial;
    
    [SerializeField]private TrailRenderer _trailRenderer;
    [SerializeField]private Gradient _positiveLineColor;
    [SerializeField]private Gradient _negativeLineColor;

    private GameObject _vfx;
    
    private bool _hasHit = false;
    RaycastHit hit;
    
    private Vector3 _lastPosition;

    public void SetUpVariables(float damage, float speed, GameObject markPrefab, bool isExplosive, 
        float explosionRadius, GunController gun, bool isCritical, Vector3 targetPoint, NetworkObject target, bool isPositive)
    {
        p_damage = damage;
        p_speed = speed;
        p_isExplosive = isExplosive;
        p_explosionRadius = explosionRadius;
        _gunController = gun;
        p_isCritical = isCritical;
        
        _vfx = isPositive ? _positiveExplosionVFX : _negativeExplosionVFX;
        
        _trailRenderer.colorGradient = isPositive ? _positiveLineColor : _negativeLineColor;
        _meshRenderer.material = isPositive ? _positiveMaterial : _negativeMaterial;
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
            
            if (_vfx != null && p_isExplosive)
            {
                Instantiate(_vfx, transform.position, Quaternion.identity);
            }

            if (InstanceFinder.IsServerStarted)
            {
                if (p_isExplosive)
                {
                    Explosed(p_explosionRadius, (int)p_damage);
                    return;
                }
                else
                {
                    if (hit.transform.TryGetComponent<IDamagable>(out IDamagable damagable))
                    {
                        bool crit = damagable.TakeDamage(_gunController.OwnerId,  (int)p_damage, p_isCritical);
                        _gunController.TriggerHitMark(crit || p_isCritical);
                        InvokeEvent(new ModifyEnergyEvent
                        {
                            p_player = _gunController.Owner,
                            p_value = p_damage
                        });
                        
                        InvokeEvent(new OnPlayerDoDamage
                        {
                            p_ownerId = _gunController.OwnerId,
                            p_value = p_damage,
                            p_critical = p_isCritical
                        });
                        
                        _gunController.AddPercentageCharge();
                        
                        if (hit.collider.TryGetComponent<EnemyCore>(out var enemyCore))
                        {
                            enemyCore.AddCharge(_gunController.IsPositivePlayerCharge, p_damage);
                        }
                    }
                }
            }

            Destroy(gameObject);
        }
    }

    public void Explosed(float radius, int damage)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        bool isHit = false;
        bool crit = false;
    
        foreach (Collider c in colliders)
        {
            if (c.TryGetComponent<IDamagable>(out IDamagable damagable))
            {
                crit = damagable.TakeDamage(_gunController.OwnerId,(int)p_damage, p_isCritical);
                InvokeEvent(new ModifyEnergyEvent
                {
                    p_player = _gunController.Owner,
                    p_value = p_damage
                });
                
                InvokeEvent(new OnPlayerDoDamage
                {
                    p_ownerId = _gunController.OwnerId,
                    p_value = p_damage,
                    p_critical = p_isCritical
                });
                
                if (c.TryGetComponent<EnemyCore>(out var enemyCore))
                {
                    enemyCore.AddCharge(_gunController.IsPositivePlayerCharge, p_damage);
                }
                
                _gunController.AddPercentageCharge();
            
                isHit = true;
            }
        }
    
        if(isHit)
            _gunController.TriggerHitMark(crit || p_isCritical);
    }
}