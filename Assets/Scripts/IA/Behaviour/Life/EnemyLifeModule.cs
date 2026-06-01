using System;
using Controller;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Managers;
using MyPrint;
using UnityEngine;

//[AddComponentMenu("EnemyBehaviour/Life")]
[DisallowMultipleComponent]
public abstract class EnemyLifeModule : EnemyBehaviourModule, IDamagable
{
    //HideInInspector to prevent draw with "base.OnInspectorGUI"
    //SerializeField to get properties in custom inspector
    [HideInInspector][SerializeField] private int _life = 10;
    public readonly SyncVar<int> p_life = new SyncVar<int>();
    
    [Header("debug")] [SerializeField] private PlayerZoneManager playerZoneManager;
    
    /// <summary>
    /// bool => Is Critical Damages <br/>
    /// int => Taken damages amount
    /// </summary>
    public Action<bool, int, int, int> OnLifeUpdate;
    public Action OnDeathViewer;
    
    public Action<int, int> p_onHitPlayer;
    
    
    [HideInInspector] private float p_damageMultiplier = 1;

    private void Start()// pour du debug, a tej en build finale
    {
        playerZoneManager = FindAnyObjectByType<PlayerZoneManager>();
    }

    public virtual bool TakeDamage(int attackerObjectId, int rawDamageAmount, ChargeType charge, bool isCritical = false)
    {
        if (IsServerInitialized)
        {
            int damages = GetDamageAmount(rawDamageAmount);
            p_onHitPlayer?.Invoke(attackerObjectId, damages);
            
            if (p_life.Value - damages <= 0)
            {
                Death(attackerObjectId, charge);
                return isCritical;
            }
            
            OnLifeUpdateObserverRPC(isCritical, damages);

        }
        return isCritical;
    }
    
    public virtual void Death(int attackerObjectId, ChargeType charge)
    {
        if(!IsServerInitialized) return;
        OnDeathObserverRPC();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        p_life.Value = _life;
        
        ListenToEvent((EndOverloadEvent EOE) => p_damageMultiplier = 1);
    }
    

    protected virtual int GetDamageAmount(int rawDamage) => Mathf.RoundToInt(rawDamage * p_damageMultiplier);

    [ObserversRpc]
    protected void OnLifeUpdateObserverRPC(bool isCritical, int dmg)
    {
        OnLifeUpdate?.Invoke(isCritical, dmg, p_life.Value, _life);
    }
    
    [ObserversRpc]
    protected void OnDeathObserverRPC()
    {
        OnDeathViewer?.Invoke();
    }
}
