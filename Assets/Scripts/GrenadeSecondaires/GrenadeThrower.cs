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
    [SerializeField] private Camera _camera;
    
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
    
    float _finalThrowForce = 0f;
    
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

            ConsumeEnergyServerRpc(_playerEnergy.p_costThrowGrenade * _playerEnergy.EnergyOneBar);
        }
    }
    
    [ServerRpc]
    private void ConsumeEnergyServerRpc(float amount)
    {
        InvokeEvent(new ConsumeEnergyEvent()
        {
            p_player = Owner,
            p_value = -amount
        });
    }

    [ServerRpc]
    public void ThrowGrenadeServerRpc()
    {
        Vector3 finalSpawnPoint = GetSpawnPoint();
        
        ElementaryGrenade grenade = Instantiate(_elementaryGrenadePrefab, finalSpawnPoint, _spawnPoint.rotation);
        
        ServerManager.Spawn(grenade.gameObject, Owner);

        grenade.Initialize(magneticCharge, _damage, _explosionRadius, _numberBounces, NetworkObject.ObjectId, 
            magneticCharge == MagneticCharge.Positive, Owner);
        
        NotifyGrenadeThrown(grenade);
    }

    private Vector3 GetSpawnPoint()
    {
        Ray cameraRay = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 camPos = _camera.transform.position;
        Vector3 targetPoint;
            
        Vector3 finalSpawnPoint = _spawnPoint.position;
        _finalThrowForce = _throwForce;
        
        if (Physics.Raycast(cameraRay, out hit, 2000, ~LayerMask.GetMask("Owner", "Other"), QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;

            if ((targetPoint - camPos).sqrMagnitude < (finalSpawnPoint - camPos).sqrMagnitude)
            {
                finalSpawnPoint = targetPoint - (_spawnPoint.forward * 0.5f);
                _finalThrowForce = _throwForce / 5;
            }
        }

        return finalSpawnPoint;
    }

    [ObserversRpc]
    private void NotifyGrenadeThrown(ElementaryGrenade grenade)
    {
        Vector3 direction = _camera.transform.forward;
        grenade.GetComponent<Rigidbody>().AddForce(direction * _finalThrowForce, ForceMode.Impulse);
        
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