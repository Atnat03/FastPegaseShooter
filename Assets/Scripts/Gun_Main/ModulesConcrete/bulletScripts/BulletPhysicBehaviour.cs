using System;
using FishNet.Object;
using GunDecorator;
using MyPrint;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletPhysicBehaviour : MonoBusListener, IAmmo, IPoolable
{
    [HideInInspector] public float p_damage;
    [HideInInspector] public float p_speed;
    [HideInInspector] public bool p_isExplosive;
    [HideInInspector] public float p_explosionRadius;
    [HideInInspector] public bool p_isCritical;
    [HideInInspector] public bool p_hadCharged;
    public Action<BulletPhysicBehaviour> OnCollision;
    private GunController _gunController;
    
    [SerializeField] private GameObject _positiveExplosionVFX;
    [SerializeField] private GameObject _negativeExplosionVFX;
    
    [Header("View")]    
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Material _positiveMaterial;
    [SerializeField] private Material _negativeMaterial;
    
    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private Gradient _positiveLineColor;
    [SerializeField] private Gradient _negativeLineColor;

    private GameObject _vfx;
    private NetworkObject _targetNetworkObject;
    private bool _hasHit = false;
    private Vector3 _lastPosition;

    public void SetUpVariables(float damage, float speed, GameObject markPrefab, bool isExplosive, 
        float explosionRadius, GunController gun, bool isCritical, Vector3 targetPoint, NetworkObject target, 
        bool isPositive, bool hadCharged = true)
    {
        p_damage = damage;
        p_speed = speed;
        p_isExplosive = isExplosive;
        p_explosionRadius = explosionRadius;
        _gunController = gun;
        p_isCritical = isCritical;
        _targetNetworkObject = target;
        p_hadCharged = hadCharged;
        
        _vfx = isPositive ? _positiveExplosionVFX : _negativeExplosionVFX;
        _trailRenderer.colorGradient = isPositive ? _positiveLineColor : _negativeLineColor;
        _meshRenderer.material = isPositive ? _positiveMaterial : _negativeMaterial;
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

        if (!Physics.SphereCast(_lastPosition, 0.15f, direction.normalized, out RaycastHit hit,
                distance, ~LayerMask.GetMask("Owner"), QueryTriggerInteraction.Ignore))
            return;

        if (_hasHit) return;
        _hasHit = true;

        if (p_isExplosive)
            HandleExplosion();
        else
            HandleDirectHit(hit);

        OnCollision.Invoke(this); // il y avait un destroy ici
    }

    private void HandleExplosion()
    {
        if (_vfx != null)
            Destroy(Instantiate(_vfx, transform.position, Quaternion.identity), 3f);

        if (_gunController.IsServerInitialized)
            Explosed(p_explosionRadius, (int)p_damage);
    }

    private void HandleDirectHit(RaycastHit hit)
    {
        if (_gunController.IsServerInitialized)
            _gunController.ApplyDamage(hit.transform.GetComponent<NetworkObject>(), (int)p_damage, p_isCritical,p_hadCharged);
        else
            _gunController.RequestApplyDamage(hit.transform.GetComponent<NetworkObject>(), (int)p_damage, p_isCritical, p_hadCharged);
    }

    public void Explosed(float radius, int damage)
    {
        if (_vfx != null)
            Instantiate(_vfx, transform.position, Quaternion.identity);

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

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
        _lastPosition = transform.position;
    }

    public void OnReturnToPool()
    {
        _hasHit = false;
    }
}