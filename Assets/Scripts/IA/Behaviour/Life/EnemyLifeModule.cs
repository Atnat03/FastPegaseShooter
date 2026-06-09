using System;
using Controller;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

//[AddComponentMenu("EnemyBehaviour/Life")]
[DisallowMultipleComponent]
public abstract class EnemyLifeModule : EnemyBehaviourModule, IDamagable
{
    public readonly SyncVar<int> p_life = new SyncVar<int>();
    
    [HideInInspector,SerializeField] private LifeModuleSO _lifeModuleSO;
    
    /// <summary>
    /// bool => Is Critical Damages <br/>
    /// int => Taken damages amount
    /// </summary>
    public Action<bool, int, int, int> OnLifeUpdate;
    public Action OnDeathViewer;
    
    public Action<int, int> p_onHitPlayer;
    
    
    [HideInInspector] private float p_damageMultiplier = 1;

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
        p_life.Value = _lifeModuleSO.p_life;
        
        ListenToEvent((EndOverloadEvent EOE) => p_damageMultiplier = 1);
    }
    

    protected virtual int GetDamageAmount(int rawDamage) => Mathf.RoundToInt(rawDamage * p_damageMultiplier);

    [ObserversRpc]
    protected void OnLifeUpdateObserverRPC(bool isCritical, int dmg)
    {
        OnLifeUpdate?.Invoke(isCritical, dmg, p_life.Value, _lifeModuleSO.p_life);
    }
    
    [ObserversRpc]
    protected void OnDeathObserverRPC()
    {
        OnDeathViewer?.Invoke();
    }
}
