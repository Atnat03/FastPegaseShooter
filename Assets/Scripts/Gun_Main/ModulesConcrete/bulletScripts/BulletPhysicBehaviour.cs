using System;
using System.Collections;
using FishNet.Object;
using GunDecorator;
using MyPrint;
using ScriptableObjectsDefinitions;
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

    [SerializeField] private float _radius = 0.5f;
    
    [SerializeField] private GameObject _positiveExplosionVFX;
    [SerializeField] private GameObject _negativeExplosionVFX;
    
    [Header("Sound")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private SoundsDataSO _dataSound;
    
    [Header("View")]
    [SerializeField] private GameObject[] _models;

    private GameObject _vfx;
    private NetworkObject _targetNetworkObject;
    private bool _hasHit = false;
    private Vector3 _lastPosition;

    public void SetUpVariables(float damage, float speed, GameObject markPrefab, bool isExplosive, 
        float explosionRadius, GunController gun, bool isCritical, Vector3 targetPoint, NetworkObject target, 
        bool isPositive, float durationBeforeExplosion, float ratio = 1, bool isDistanceReduce = false,bool hadCharged = true)
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
        
        _models[0].SetActive(isPositive);
        _models[1].SetActive(!isPositive);
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

        if (!Physics.SphereCast(_lastPosition, _radius, direction.normalized, out RaycastHit hit,
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
        {
            SoundManager.PlaySound(_dataSound, "Explosion", _audioSource);
            
            GameObject v = Instantiate(_vfx);
            v.transform.position = transform.position + transform.up * 0.1f;
            Destroy(v.gameObject, 3f);
        }

        if (_gunController.IsServerInitialized)
            Explosed(p_explosionRadius, (int)p_damage);
    }

    private void HandleDirectHit(RaycastHit hit)
    {
        if (_gunController.IsServerInitialized)
            _gunController.ApplyDamage(hit.collider.gameObject, (int)p_damage, p_isCritical,p_hadCharged);
        else
            _gunController.RequestApplyDamage(hit.collider.gameObject, (int)p_damage, p_isCritical, p_hadCharged);
    }

    IEnumerator DelayBeforeExplosion(float duration)
    {
        yield return new WaitForSeconds(duration);

        Explosed(p_explosionRadius, (int)p_damage);
    }

    public void Explosed(float radius, int damage)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider c in colliders)
        {
            if (_gunController.IsServerInitialized)
                _gunController.ApplyDamage(c.gameObject, (int)p_damage, p_isCritical, p_hadCharged);
            else
                _gunController.RequestApplyDamage(c.gameObject, (int)p_damage, p_isCritical, p_hadCharged);
        }
    }

    public void Spawn()
    {
        _lastPosition = transform.position;
    }

    public void ReturnToPool()
    {
        _hasHit = false;
        _lastPosition = transform.position;
    }
}