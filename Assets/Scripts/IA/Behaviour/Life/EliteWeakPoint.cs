using System;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class EliteWeakPoint : NetworkBehaviour, IDamagable
{
    [SerializeField] private BasicEnemyLife baseEnemyLife;
    [SerializeField] private int _life;
    public readonly SyncVar<int> p_life = new SyncVar<int>();
    [SerializeField] private float _energyGainWhenTouch = 1;
    [SerializeField] private float _eliteDamageMultWhenDestroyed;
    
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
                Death(next-prev); // serveur uniquement
            }
        }
    }
    
    [Server]
    public bool TakeDamage(int rawDamageAmount, bool isCritical = false)
    {
        if (IsServerInitialized)
        {
            int damages = GetDamageAmount(rawDamageAmount);
            p_life.Value -= damages;
            baseEnemyLife.TriggerHitMarkObserversRpc(true, damages);
        }

        //Damages are always critical when done on weak point
        return true;
    }
    protected virtual int GetDamageAmount(int rawDamage)
    {
        return Mathf.RoundToInt(rawDamage * baseEnemyLife.p_damageMultiplier);
    }

    [Server]
    public void Death(int takenDamages)
    {
        baseEnemyLife.TakeDamage(Mathf.RoundToInt(takenDamages*_eliteDamageMultWhenDestroyed));
        
        InstanceFinder.ServerManager.Despawn(gameObject);
        WeakPointDestroyedObserverRPC();
    }

    [ObserversRpc]
    void WeakPointDestroyedObserverRPC()
    {
        //callBack when weakpoint destroyed
        gameObject.SetActive(false);
    }
}
