using System;
using FishNet.Object;
using GunDecorator;
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
    [HideInInspector] public bool p_isDistanceReduce;
    [HideInInspector] public float p_ratioDistanceReduce;
    
    public Action<BulletBehaviour> OnCollision;

    [SerializeField] private GameObject _positiveExplosionVFX;
    [SerializeField] private GameObject _negativeExplosionVFX;

    [Header("View")]    
    [SerializeField] private GameObject[] _models;
    
    private GameObject _vfx;
    
    private GunController _gunController;
    private Vector3 _targetPoint;
    private NetworkObject _targetNetworkObject;
    private Vector3 _shootPos;

    private bool _hasHit = false;

    private void FixedUpdate()
    {
        if (_hasHit) return;

        Vector3 startPosition = transform.position;

        Move();

        DetectCollision(startPosition, transform.position);
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
        bool isPositive, float duration = 0, 
        float factorReduceDamageByDistance = 1,
        bool isDistanceReduce = false, bool hadCharged = true)
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
        p_isDistanceReduce = isDistanceReduce;
        p_ratioDistanceReduce = factorReduceDamageByDistance;
        
        _vfx = isPositive ? _positiveExplosionVFX : _negativeExplosionVFX;
        
        _models[0].SetActive(isPositive);
        _models[1].SetActive(!isPositive);
        
        _shootPos = transform.position;
    }

    private void Move()
    {
        transform.Translate(transform.forward * (p_speed * Time.deltaTime), Space.World);
    }

    private void DetectCollision(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0f)
            return;

        if (!Physics.SphereCast(start, 0.15f, direction.normalized, out RaycastHit hit, distance, ~LayerMask.GetMask("Owner"), QueryTriggerInteraction.Ignore))
            return;

        if (_hasHit) return;
        _hasHit = true;

        if (p_isExplosive)
            HandleExplosion();
        else
            HandleDirectHit(hit);

        OnCollision?.Invoke(this);
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
        if (_gunController == null)
        {
            Debug.LogError("GunController is NULL");
            return;
        }

        if (_gunController.IsServerInitialized)
        {
            float damage = p_damage;

            if (p_isDistanceReduce)
                damage *= (1 / (1 + p_ratioDistanceReduce * Vector3.Distance(hit.point, _shootPos)));
            
            _gunController.ApplyDamage(hit.collider.gameObject, (int)damage, p_isCritical, p_hadCharged);
            
            /*if (hit.collider.TryGetComponent<G>(out var core))
            {
                _gunController.ApplyDamage(core.NetworkObject, (int)damage, p_isCritical, p_hadCharged);
            }
            else
            {
                Debug.LogWarning("Touched object has no NetworkObject");
            }*/
        }

        CreateHitMark(hit);
    }
    
    private void CreateHitMark(RaycastHit hit) => Destroy(Instantiate(p_markPrefab, _targetPoint + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal)), 1f);

    public void Explosed(float radius, int damage)
    {
        if(_vfx != null) Instantiate(_vfx, transform.position, Quaternion.identity);

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        
        foreach (Collider c in Physics.OverlapSphere(transform.position, radius))
        {
            /*if (!c.TryGetComponent<IDamagable>(out _)) continue;
            if (!c.TryGetComponent<NetworkObject>(out var netObj)) continue;*/

            _gunController.ApplyDamage(c.gameObject, damage, p_isCritical, p_hadCharged);
        }
    }

    private void Reset()
    {
        _hasHit = false;
        OnCollision = null;
        //if (_trailRenderer != null) _trailRenderer.Clear();
        _targetPoint = Vector3.zero;
        _targetNetworkObject = null;
        _gunController = null;
        _vfx = null;
    }

    public void Spawn() { }

    public void ReturnToPool()
    {
        Reset();
    }
}