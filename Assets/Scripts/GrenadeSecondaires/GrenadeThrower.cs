using System;
using FishNet.Object;
using MyPrint;
using UnityEngine;

public enum Element {Fire, Electric, Ice}

[RequireComponent(typeof(GrenadeThrowerView))]
public class GrenadeThrower : NetworkBehaviour
{
    #region Properties
    
    public Element Element => _element;

    #endregion
    
    #region Variables

    [SerializeField] private Element _element;
    [SerializeField] private ArmBridgeAnimation _bridgeAnimation;
    
    [Header("Throw")]
    [SerializeField] private ElementaryGrenade _elementaryGrenadePrefab;
    [SerializeField] private Transform _spawnPoint;
    
    [Header("Settings")]
    [SerializeField] private float _cooldown = 2f;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _throwForce = 10f;
    [SerializeField] private Vector3 _directionThrow = new Vector3(0f, 0.5f, 1f);
    [SerializeField] private int _numberBounces = 2;

    [Header("Explosion")] 
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private bool _showGizmoOnSpawnPoint = true;
    
    private bool _canThrow = true;
    private float _elapsedTimeCooldown = 0f;
    
    //Action
    public Action OnStartThrow;
    public Action<float> OnCooldownUpdate;
    public Action<ElementaryGrenade> OnThrow;
    #endregion

    #region Functions
    
    private void Update()
    {
        if (_elapsedTimeCooldown > 0)
        {
            _elapsedTimeCooldown -= Time.deltaTime;
            _canThrow = false;
            
            OnCooldownUpdate?.Invoke(_elapsedTimeCooldown / _cooldown);
            
            if (_elapsedTimeCooldown <= 0f)
            {
                _canThrow = true;
            }
        }
    }

    public void TryThrowGrenade()
    {
        if (_canThrow)
        {
            if (_bridgeAnimation != null)
            {
                _bridgeAnimation.StartThrow(_element);
            }
            else
            {
                ThrowGrenadeServerRpc();
            }
            
            OnStartThrow?.Invoke();
            
            _elapsedTimeCooldown = _cooldown;
        }
    }

    [ServerRpc]
    public void ThrowGrenadeServerRpc()
    {
        ThrowGrenadeObserversRpc();
    }

    [ObserversRpc]
    private void ThrowGrenadeObserversRpc()
    {
        Cons.Print("Throw grenade", ColorConsole.Green);
        
        ElementaryGrenade grenade = Instantiate(_elementaryGrenadePrefab, _spawnPoint.position, _spawnPoint.rotation);
        grenade.Initialize(_element, _damage, _explosionRadius, _numberBounces, NetworkObject.ObjectId);

        Vector3 direction = _spawnPoint.forward;
        grenade.GetComponent<Rigidbody>().AddForce(direction * _throwForce, ForceMode.Impulse);
        
        OnThrow?.Invoke(grenade);
    }

    private void OnDrawGizmosSelected()
    {
        if (_showGizmoOnSpawnPoint)
        {
            Gizmos.color = Color.darkGreen;
            Gizmos.DrawWireSphere(_spawnPoint.position, _explosionRadius);
        }
    }

    #endregion
}
