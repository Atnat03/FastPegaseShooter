using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ElementaryGrenade : NetworkBehaviour
{
    [SerializeField] private MeshRenderer _model;
    
    private readonly SyncVar<Element> _element = new SyncVar<Element>();
    private readonly SyncVar<float> _radius = new SyncVar<float>();
    private readonly SyncVar<int> _damage = new SyncVar<int>();
    private readonly SyncVar<int> _networkIdAttacker = new SyncVar<int>();
    
    private float _maxNumberTouch;
    private float _currentNumberTouch;
    private const float SPAWN_GRACE = 0.2f;
    private float _spawnTime;
    
    private ParticleSystem _particlesExplosionPrefab;
    
    private Vector3 _lastPosition;
    private bool _hasHit;

    public void Initialize(Element element, int damage, float radius, int numberWallTouch, int netID)
    {
        _element.Value = element;
        _radius.Value = radius;
        _maxNumberTouch = numberWallTouch;
        _currentNumberTouch = 0;
        _damage.Value = damage;
        _networkIdAttacker.Value = netID;
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

    private void OnElementChanged(Element prev, Element next, bool asServer)
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_model != null)
        {
            Color GetColor(Element e)
            {
                return e switch
                {
                    Element.Fire => Color.red,
                    Element.Electric => Color.yellow,
                    Element.Ice => Color.blue,
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

        if (Physics.SphereCast(_lastPosition, 0.1f, direction.normalized, out RaycastHit hit,
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