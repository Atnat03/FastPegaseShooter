using System;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using UnityEngine;

public class ElementaryGrenade : NetworkBusListener
{
    [SerializeField] private MeshRenderer _model;
    
    private readonly SyncVar<MagneticCharge> _element = new SyncVar<MagneticCharge>();
    private readonly SyncVar<float> _radius = new SyncVar<float>();
    private readonly SyncVar<int> _damage = new SyncVar<int>();
    private readonly SyncVar<int> _networkIdAttacker = new SyncVar<int>();
    private readonly SyncVar<bool> _isPositive = new SyncVar<bool>();
    private readonly SyncVar<NetworkConnection> _thrower = new SyncVar<NetworkConnection>();
    
    private float _maxNumberTouch;
    private float _currentNumberTouch;
    private const float SPAWN_GRACE = 0.2f;
    private float _spawnTime;
    
    
    private ParticleSystem _particlesExplosionPrefab;
    
    private Vector3 _lastPosition;
    private bool _hasHit;

    public void Initialize(MagneticCharge magneticCharge, int damage, float radius, int numberWallTouch, int netID, bool isPositive, NetworkConnection thrower)
    {
        _element.Value = magneticCharge;
        _radius.Value = radius;
        _maxNumberTouch = numberWallTouch;
        _currentNumberTouch = 0;
        _damage.Value = damage;
        _networkIdAttacker.Value = netID;
        _isPositive.Value = isPositive;
        _thrower.Value = thrower;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        UpdateVisuals();
        
        _element.OnChange += OnElementChanged;
    }

    public override void OnStopClient()
    {
        _element.OnChange -= OnElementChanged;
        base.OnStopClient();
    }

    private void OnElementChanged(MagneticCharge prev, MagneticCharge next, bool asServer)
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_model != null)
        {
            Color GetColor(MagneticCharge e)
            {
                return e switch
                {
                    MagneticCharge.Positive => Color.red,
                    MagneticCharge.Negative => Color.blue,
                    _ => Color.white
                };
            }
            
            _model.material.color = GetColor(_element.Value);
        }
    }
    
    void Start()
    {
        _lastPosition = transform.position;
        _spawnTime = Time.time;
    }

    void FixedUpdate()
    {
        if (Time.time - _spawnTime < SPAWN_GRACE) 
        {
            _lastPosition = transform.position;
            return;
        }
    
        if (IsServerInitialized)
        {
            DetectCollision();
        }
        
        _lastPosition = transform.position;
    }

    private void DetectCollision()
    {
        Vector3 direction = transform.position - _lastPosition;
        float distance = direction.magnitude;

        if (distance <= 0f) return;

        if (Physics.SphereCast(transform.position, 0.1f, direction.normalized, out RaycastHit hit,
                distance, ~LayerMask.GetMask("Owner", "Other"), QueryTriggerInteraction.Ignore))
        {
            if (_hasHit) return;
            _hasHit = true;

            Cons.Print($"Grenade hit at {hit.point}", ColorConsole.Grey);

            ApplyExplosionDamage(hit.point);
            
            ExplodeObserversRpc(hit.point, hit.normal);

            ServerManager.Despawn(gameObject);
        }
    }

    private void ApplyExplosionDamage(Vector3 explosionCenter)
    {
        Collider[] colliders = Physics.OverlapSphere(explosionCenter, _radius.Value);
        
        foreach (Collider c in colliders)
        {
            if (c.TryGetComponent(out IDamagable damagable))
            {
                Cons.Print($"Grenade damaged {c.name}", ColorConsole.Red);
                damagable.TakeDamage(_networkIdAttacker.Value, _damage.Value);
                
                InvokeEvent(new AddEnergyEvent
                {
                    p_player = _thrower.Value,
                    p_value = _damage.Value
                });
                        
                if (c.TryGetComponent<EnemyCore>(out var enemyCore))
                {
                    enemyCore.AddCharge(_isPositive.Value, _damage.Value);
                }
            }
        }
    }

    [ObserversRpc]
    private void ExplodeObserversRpc(Vector3 position, Vector3 normal)
    {
        if (_particlesExplosionPrefab != null)
        {
            ParticleSystem explosion = Instantiate(_particlesExplosionPrefab, position, Quaternion.LookRotation(normal));
            Destroy(explosion.gameObject, 3f);
        }
    }

    public void SetEffect(ParticleSystem particle)
    {
        _particlesExplosionPrefab = particle;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _radius.Value);
    }
}