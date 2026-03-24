using System;
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
    
    [HideInInspector] public float p_damageMultiplier = 1;

    public virtual bool TakeDamage(int rawDamageAmount, bool isCritical = false)
    {
        if (IsServerInitialized)
        {
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
