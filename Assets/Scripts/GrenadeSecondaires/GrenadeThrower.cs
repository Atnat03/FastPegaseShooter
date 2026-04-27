using System;
using FishNet.Object;
using MyPrint;
using UnityEngine;

public enum MagneticCharge {Positive, Negative}

[RequireComponent(typeof(GrenadeThrowerView))]
public class GrenadeThrower : NetworkBusListener
{
    #region Properties
    
    public MagneticCharge MagneticCharge => magneticCharge;

    #endregion
    
    #region Variables

    [SerializeField] private MagneticCharge magneticCharge;
    [SerializeField] private ArmBridgeAnimation _bridgeAnimation;
    [SerializeField] private PlayerEnergy _playerEnergy;
    
    [Header("Throw")]
    [SerializeField] private ElementaryGrenade _elementaryGrenadePrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private GunSwitching _currentGun;
    
    [Header("Settings")]
    [SerializeField] private float _cooldown = 2f;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _throwForce = 10f;
    [SerializeField] private int _numberBounces = 2;

    [Header("Explosion")] 
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private bool _showGizmoOnSpawnPoint = true;
    ParticleSystem _particleSystem;
    
    private bool _canThrow = true;
    private float _elapsedTimeCooldown = 0f;
    
    //Action
    public Action OnStartThrow;
    public Action<float> OnCooldownUpdate;
    public Action<ElementaryGrenade> OnThrow;
    #endregion

    #region Functions
    
    public void Initialize(int startIndex)
    {
        magneticCharge = (MagneticCharge)startIndex;
    }

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
        if (_canThrow && _playerEnergy.CanThrow(_playerEnergy.p_costThrowGrenade))
        {
            if (_bridgeAnimation != null)
            {
                _bridgeAnimation.StartThrow(magneticCharge);
                _currentGun.IGunMain.TryCancelShooting();
                _currentGun.ISurchargeMain.StopReload();
            }
            else
            {
                ThrowGrenadeServerRpc();
            }
            
            OnStartThrow?.Invoke();
            
            _elapsedTimeCooldown = _cooldown;
            
            InvokeEvent(new ModifyEnergyEvent
            {
                p_player = Owner,
                p_value = -(_playerEnergy.p_costThrowGrenade * _playerEnergy.EnergyOneBar)
            });
        }
    }

    [ServerRpc]
    public void ThrowGrenadeServerRpc()
    {
        ElementaryGrenade grenade = Instantiate(_elementaryGrenadePrefab, _spawnPoint.position, _spawnPoint.rotation);
        
        ServerManager.Spawn(grenade.gameObject, Owner);

        grenade.Initialize(magneticCharge, _damage, _explosionRadius, _numberBounces, NetworkObject.ObjectId, 
            magneticCharge == MagneticCharge.Positive, Owner);
        
        NotifyGrenadeThrown(grenade);
    }

    [ObserversRpc]
    private void NotifyGrenadeThrown(ElementaryGrenade grenade)
    {
        Cons.Print("Grenade thrown", ColorConsole.Green);
        
        Vector3 direction = _spawnPoint.forward;
        grenade.GetComponent<Rigidbody>().AddForce(direction * _throwForce, ForceMode.Impulse);
        
        _currentGun.ISurchargeMain.StopReload();

        OnThrow?.Invoke(grenade);
    }
    
    [ServerRpc]
    public void ChangeMagneticChargeServerRpc()
    {
        if (!IsServerInitialized) return;

        magneticCharge = magneticCharge == MagneticCharge.Positive ? MagneticCharge.Negative : MagneticCharge.Positive;
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