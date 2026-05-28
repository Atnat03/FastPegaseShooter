using System;
using Controller;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class EnergyCuve : NetworkBusListener, IDamagable
{
    private readonly SyncVar<int> _lifeAmount = new SyncVar<int>();

    [SerializeField] private int _life;
    [SerializeField] private float _energyToGive;
    
    public Action<bool, int> OnLifeUpdate;
    public Action OnDeath;
    
    private float damageMultiplier = 1;

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        _lifeAmount.Value = _life;
        _lifeAmount.OnChange += OnLifeChanged;
        
        //ListenToEvent((SwapingGunEvent SGE) => damageMultiplier = SGE.dataSurcharge.damageMultiplier);
        ListenToEvent((EndOverloadEvent EOE) => damageMultiplier = 1);
    }

    private void OnLifeChanged(int prev, int next, bool asServer)
    {
        if (next <= 0)
        {
            if (asServer)
            {
                Death(prev-next); // serveur uniquement
            }
        }
    }

    public bool TakeDamage(int attackerObjectId, int rawDamageAmount, ChargeType charge, bool isCritical = false)
    {
        //Debug.Log($"raw {rawDamageAmount}, mult {damageMultiplier}, life {_lifeAmount.Value}");
        if (IsServerInitialized)
        {
            _lifeAmount.Value -= Mathf.RoundToInt(rawDamageAmount * damageMultiplier);
            OnLifeUpdateObserverRPC(false, attackerObjectId);
        }
        
        //should never be a critical damage
        return false;
    }

    public void Death(int takenDamages)
    {
        if(!IsServerInitialized) return;
        InvokeEvent<ModifyEnergyEvent>(new ModifyEnergyEvent { p_value = _energyToGive});
        OnDeathObserverRPC();
    }
    
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
