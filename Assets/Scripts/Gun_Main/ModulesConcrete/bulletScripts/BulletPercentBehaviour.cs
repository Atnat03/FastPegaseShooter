using System;
using FishNet.Object;
using GunDecorator;
using UnityEngine;

public class BulletPercentBehaviour : MonoBusListener, IAmmo, IPoolable
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

    public Action<BulletPercentBehaviour> OnCollision;

    [SerializeField] private GameObject _positiveExplosionVFX;
    [SerializeField] private GameObject _negativeExplosionVFX;

    [Header("View")]
    [SerializeField] private GameObject[] _models;

    private GameObject _vfx;
    private GunController _gunController;
    private Vector3 _shootPos;

    
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
        p_hadCharged = hadCharged;
        p_isDistanceReduce = isDistanceReduce;
        p_ratioDistanceReduce = factorReduceDamageByDistance;
        
        _vfx = isPositive ? _positiveExplosionVFX : _negativeExplosionVFX;
        
        _models[0].SetActive(isPositive);
        _models[1].SetActive(!isPositive);
        
        _shootPos = transform.position;
    }
    
    private void Update()
    {
        transform.Translate(transform.forward * (p_speed * Time.deltaTime), Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Owner"))
            return;

        if (p_isExplosive)
        {
            HandleExplosion();
            return;
        }

        HandleDirectHit(other);
    }

    private void HandleDirectHit(Collider target)
    {
        if (_gunController == null)
            return;

        if (_gunController.IsServerInitialized)
        {
            float damage = p_damage;

            if (p_isDistanceReduce)
            {
                damage *= (1f / (1f + p_ratioDistanceReduce *
                                 Vector3.Distance(transform.position, _shootPos)));
            }

            _gunController.ApplyDamage(
                target.gameObject,
                (int)damage,
                p_isCritical,
                p_hadCharged);
        }

        // Pas de destruction de la balle ici :
        // elle continue sa trajectoire et peut toucher d'autres ennemis.
    }

    private void HandleExplosion()
    {
        if (_vfx != null)
            Destroy(Instantiate(_vfx, transform.position, Quaternion.identity), 3f);

        if (_gunController.IsServerInitialized)
            Explosed(p_explosionRadius, (int)p_damage);

        OnCollision?.Invoke(this); // l'explosive peut disparaître
    }

    public void Explosed(float radius, int damage)
    {
        foreach (Collider c in Physics.OverlapSphere(transform.position, radius))
        {
            _gunController.ApplyDamage(c.gameObject, damage, p_isCritical, p_hadCharged);
        }
    }

    public void Spawn() { }

    public void ReturnToPool()
    {
        Reset();
    }
    
    private void Reset()
    {
        OnCollision = null;
        _gunController = null;
        _vfx = null;
    }
}