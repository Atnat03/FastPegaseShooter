using FishNet.Object;
using GunDecorator;
using MyPrint;
using UnityEngine;

public class BulletBehaviour : MonoBusListener, IAmmoExplosif
{
    [HideInInspector] public float p_damage;
    [HideInInspector] public float p_speed;
    [HideInInspector] public GameObject p_markPrefab;
    [HideInInspector] public bool p_isExplosive;
    [HideInInspector] public float p_explosionRadius;
    [HideInInspector] public bool p_isCritical;

    [SerializeField] private GameObject _positiveExplosionVFX;
    [SerializeField] private GameObject _negativeExplosionVFX;

    [Header("View")]    
    [SerializeField]private MeshRenderer _meshRenderer;
    [SerializeField]private Material _positiveMaterial;
    [SerializeField] private Material _negativeMaterial;
    
    [SerializeField]private TrailRenderer _trailenderer;
    [SerializeField]private Gradient _positiveLineColor;
    [SerializeField]private Gradient _negativeLineColor;
    
    private GameObject _vfx;
    
    private GunController _gunController;
    private Vector3 _targetPoint;
    private NetworkObject _targetNetworkObject;

    private bool _hasHit = false;
    private bool _hasMark = false;

    private void FixedUpdate()
    {
        if (_hasHit) return;

        DetectCollision();
        Move();
    }

    public void SetUpVariables(
        float damage,
        float speed,
        GameObject markPrefab,
        bool isExplosive,
        float explosionRadius,
        GunController gun,
        bool isCritical,
        Vector3 targetPoint,
        NetworkObject target,
        bool isPositive)
    {
        p_damage = damage;
        p_speed = speed;
        p_markPrefab = markPrefab;
        p_isExplosive = isExplosive;
        p_explosionRadius = explosionRadius;
        _gunController = gun;
        p_isCritical = isCritical;
        _targetPoint = targetPoint;
        _targetNetworkObject = target;

        _vfx = isPositive ? _positiveExplosionVFX : _negativeExplosionVFX;
        
        _trailenderer.colorGradient = isPositive ? _positiveLineColor : _negativeLineColor;
        _meshRenderer.material = isPositive ? _positiveMaterial : _negativeMaterial;

        Destroy(gameObject, 3f);
    }

    private void Move()
    {
        transform.Translate(transform.forward * (p_speed * Time.deltaTime), Space.World);
    }

    private void DetectCollision()
    {
        float distance = p_speed * Time.fixedDeltaTime;

        if (!Physics.Raycast(
                transform.position,
                transform.forward,
                out RaycastHit hit,
                distance,
                ~LayerMask.NameToLayer("Owner"),
                QueryTriggerInteraction.Ignore))
            return;

        _hasHit = true;

        if (p_isExplosive)
        {
            HandleExplosion();
        }
        else
        {
            HandleDirectHit(hit);
        }

        Destroy(gameObject);
    }

    private void HandleExplosion()
    {
        if(_vfx != null)
            Destroy(Instantiate(_vfx, transform.position, Quaternion.identity), 3f);

        if (_gunController.IsServerInitialized)
            Explosed(p_explosionRadius, (int)p_damage);
    }

    private void HandleDirectHit(RaycastHit hit)
    {
        if (_gunController.IsServerInitialized)
        {
            ApplyDamage();
        }

        CreateHitMark(hit);
    }

    private void ApplyDamage()
    {
        if (_targetNetworkObject == null) return;

        if (_targetNetworkObject.TryGetComponent<IDamagable>(out var d))
        {
            bool crit = d.TakeDamage(
                _gunController.NetworkObject.ObjectId,
                (int)p_damage,
                p_isCritical);

            _gunController.TriggerHitMark(crit || p_isCritical);

            InvokeEvent(new ModifyEnergyEvent
            {
                p_player = _gunController.Owner,
                p_value = p_damage
            });

            if (_targetNetworkObject.TryGetComponent<EnemyCore>(out var enemyCore))
            {
                enemyCore.AddCharge(_gunController.IsPositivePlayerCharge, p_damage);
            }
        }
    }

    private void CreateHitMark(RaycastHit hit)
    {
        GameObject hitMark = Instantiate(
            p_markPrefab,
            _targetPoint + hit.normal * 0.01f,
            Quaternion.LookRotation(hit.normal));

        Destroy(hitMark, 1f);
    }

    public void Explosed(float radius, int damage)
    {
        if(_vfx != null)
            Instantiate(_vfx, transform.position, Quaternion.identity);

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        bool crit = false;
        bool hit = false;
        
        foreach (Collider c in colliders)
        {
            if (!c.TryGetComponent<IDamagable>(out IDamagable damagable))
                continue;

            crit = damagable.TakeDamage(
                _gunController.NetworkObject.ObjectId,
                damage,
                p_isCritical);
            InvokeEvent(new ModifyEnergyEvent
            {
                p_player = _gunController.Owner,
                p_value = p_damage
            });

            hit = true;

            if (_targetNetworkObject != null &&
                _targetNetworkObject.TryGetComponent<EnemyCore>(out var enemyCore))
            {
                enemyCore.AddCharge(_gunController.IsPositivePlayerCharge, p_damage);
            }
        }
        
        if(hit)
            _gunController.TriggerHitMark(crit || p_isCritical);
    }
}