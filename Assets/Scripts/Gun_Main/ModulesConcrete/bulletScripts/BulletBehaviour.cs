using System;
using FishNet.Object;
using GunDecorator;
using MyPrint;
using UnityEngine;

public class BulletBehaviour : MonoBusListener, IAmmo, IPoolable
{
    [HideInInspector] public float p_damage;
    [HideInInspector] public float p_speed;
    [HideInInspector] public GameObject p_markPrefab;
    [HideInInspector] public bool p_isExplosive;
    [HideInInspector] public float p_explosionRadius;
    [HideInInspector] public bool p_isCritical;
    [HideInInspector] public bool p_hadCharged;
    public Action<BulletBehaviour> OnCollision;

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
        bool isPositive, bool hadCharged = true)
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
        p_hadCharged = hadCharged;

        _vfx = isPositive ? _positiveExplosionVFX : _negativeExplosionVFX;
        
        _trailenderer.colorGradient = isPositive ? _positiveLineColor : _negativeLineColor;
        _meshRenderer.material = isPositive ? _positiveMaterial : _negativeMaterial;
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
                ~LayerMask.GetMask("Owner"),
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

        OnCollision.Invoke(this); // il y avait un destroy ici
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
        if (_gunController.IsServerInitialized) _gunController.ApplyDamage(_targetNetworkObject, (int)p_damage, p_isCritical, p_hadCharged);
        else _gunController.RequestApplyDamage(_targetNetworkObject, (int)p_damage, p_isCritical, p_hadCharged);

        CreateHitMark(hit);
    }
    
    private void CreateHitMark(RaycastHit hit) => Destroy(Instantiate(p_markPrefab, _targetPoint + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal)), 1f);

    public void Explosed(float radius, int damage)
    {
        if(_vfx != null)
            Instantiate(_vfx, transform.position, Quaternion.identity);

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        bool crit = false;
        bool hit = false;
        
        foreach (Collider c in colliders)
        {
            if (!c.TryGetComponent<IDamagable>(out _)) continue;
    
            if (!c.TryGetComponent<NetworkObject>(out var netObj)) continue;

            if (_gunController.IsServerInitialized)
                _gunController.ApplyDamage(netObj, (int)p_damage, p_isCritical, p_hadCharged);
            else
                _gunController.RequestApplyDamage(netObj, (int)p_damage, p_isCritical, p_hadCharged);
        }
    }

    public void Spawn()
    {
        
    }

    public void ReturnToPool()
    {
        
    }
}