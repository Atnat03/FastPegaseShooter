using System;
using Controller;
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
    
    [Header("debug")] [SerializeField] private SwapGunManager swapGunManager;
    
    /// <summary>
    /// bool => Is Critical Damages <br/>
    /// int => Taken damages amount
    /// </summary>
    public Action<bool, int, int, int> OnLifeUpdate;
    public Action OnDeathViewer;
    public Action<int> OnDeath;
    
    public Action<int, int> p_onHitPlayer;
    
    
    [HideInInspector] private float p_damageMultiplier = 1;

    private void Start()// pour du debug, a tej en build finale
    {
        swapGunManager = FindAnyObjectByType<SwapGunManager>();
    }

    public virtual bool TakeDamage(int attackerObjectId, int rawDamageAmount, EnemyCore.ChargeType charge, bool isCritical = false)
    {
        if(!CanReceiveDamage(charge)) return false;
        
        if (IsServerInitialized)
        {
            if (_enemyCore._hasShied.Value != 0)
                return false;
            
            p_onHitPlayer?.Invoke(attackerObjectId, GetDamageAmount(rawDamageAmount));
            OnLifeUpdateObserverRPC(isCritical, GetDamageAmount(rawDamageAmount));

            if (p_life.Value - GetDamageAmount(rawDamageAmount) <= 0)
            {
                OnDeath?.Invoke(attackerObjectId);
            }
        }
        
        //debug clement
        
        float player1PVs = -1;
        float player2PVs = -1;
        if (PlayerHealthManager.Instance != null)
        {
            player1PVs = PlayerHealthManager.Instance.RegisteredPlayers.Count > 0
                ? PlayerHealthManager.Instance.RegisteredPlayers[0].CurrentHealth
                : 0;
            player2PVs = PlayerHealthManager.Instance.RegisteredPlayers.Count > 1
                ? PlayerHealthManager.Instance.RegisteredPlayers[1].CurrentHealth
                : 0;
        }
        InvokeEvent(new OnDataLog
        {
            entityName = gameObject.name,
            weapon = "heal",
            targetName = gameObject.name,
            damages = rawDamageAmount,
            player1PVs = player1PVs,
            player2PVs = player2PVs,
            ArenaID = swapGunManager.p_playerZones.ContainsKey(OwnerId) ? swapGunManager.p_playerZones[OwnerId] : -1
        });
        
        //fin du debug
        return isCritical;
    }

    protected bool CanReceiveDamage(EnemyCore.ChargeType charge)
    {
        if(_enemyCore.p_affinityType == EnemyCore.ChargeType.None) return true;
        
        return _enemyCore.p_affinityType != charge;
    }
    
    public virtual void Death(int takenDamages)
    {
        if(!IsServerInitialized) return;
        OnDeathObserverRPC();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        p_life.Value = _life;
        p_life.OnChange += OnLifeChanged;
        
        //ListenToEvent((SwapingGunEvent SGE) => p_damageMultiplier = SGE.dataSurcharge.damageMultiplier);
        ListenToEvent((EndOverloadEvent EOE) => p_damageMultiplier = 1);
    }

    public override void OnStopServer()
    {
        p_life.OnChange -= OnLifeChanged;
        base.OnStopServer();
    }

    [Server]
    protected virtual void OnLifeChanged(int prev, int next, bool asServer)
    {
        if (next <= 0)
        {
            if (asServer)
            {
                Death(prev-next); // serveur uniquement
            }
        }
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
