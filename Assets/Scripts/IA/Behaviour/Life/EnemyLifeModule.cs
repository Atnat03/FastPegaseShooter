using System;
using Controller;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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
    
    /// <summary>
    /// bool => Is Critical Damages <br/>
    /// int => Taken damages amount
    /// </summary>
    public Action<bool, int, int, int> OnLifeUpdate;
    public Action OnDeathViewer;
    public Action<int> OnDeath;
    
    public Action<int, int> p_onHitPlayer;
    
    
    [HideInInspector] private float p_damageMultiplier = 1;

    public virtual bool TakeDamage(int attackerObjectId, int rawDamageAmount, bool isCritical = false)
    {
        if (IsServerInitialized)
        {
            p_onHitPlayer?.Invoke(attackerObjectId, GetDamageAmount(rawDamageAmount));
            OnLifeUpdateObserverRPC(isCritical, GetDamageAmount(rawDamageAmount));

            if (p_life.Value - GetDamageAmount(rawDamageAmount) <= 0)
            {
                OnDeath?.Invoke(attackerObjectId);
            }
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
        OnLifeUpdate?.Invoke(isCritical, dmg, p_life.Value, _life);
    }
    
    [ObserversRpc]
    protected void OnDeathObserverRPC()
    {
        OnDeathViewer?.Invoke();
    }
}
