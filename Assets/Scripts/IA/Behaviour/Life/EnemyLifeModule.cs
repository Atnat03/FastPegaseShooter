using System;
using Controller;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public abstract class EnemyLifeModule : EnemyBehaviourModule, IDamagable
{
    [SerializeField] protected float _energyGainWhenTouch = 1;
    [SerializeField] private int _life;
    public readonly SyncVar<int> p_life = new SyncVar<int>();
    
    /// <summary>
    /// bool => Is Critical Damages <br/>
    /// int => Taken damages amount
    /// </summary>
    public Action<bool, int> OnLifeUpdate;
    public Action OnDeath;
    
    public Action<int, int> p_onHitPlayer;
    
    
    [HideInInspector] private float p_damageMultiplier = 1;

    public virtual bool TakeDamage(int attackerObjectId, int rawDamageAmount, bool isCritical = false)
    {
        if (IsServerInitialized)
        {
            p_onHitPlayer?.Invoke(attackerObjectId, GetDamageAmount(rawDamageAmount));
            OnLifeUpdateObserverRPC(isCritical, GetDamageAmount(rawDamageAmount));
        }
        return isCritical;
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
        
        ListenToEvent((SwapingGunEvent SGE) => p_damageMultiplier = SGE.dataSurcharge.damageMultiplier);
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
        OnLifeUpdate?.Invoke(isCritical, dmg);
    }
    [ObserversRpc]
    protected void OnDeathObserverRPC()
    {
        OnDeath?.Invoke();
    }
}
