using System;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class BasicEnemyLife : NetworkBehaviour, IDamagable
{
    [SerializeField] private int _life;
    public readonly SyncVar<int> p_life = new SyncVar<int>();

    private Guid _gridReaderId;
    private int _enemySpawnCost;

    public override void OnStartServer()
    {
        base.OnStartServer();
        p_life.Value = _life;
        p_life.OnChange += OnLifeChanged;
    }
    
    private void OnLifeChanged(int prev, int next, bool asServer)
    {
        if (next <= 0)
        {
            if (asServer)
            {
                Death(); // serveur uniquement
            }
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        TakeDamageServerRpc(damageAmount);
    }

    [ServerRpc(RequireOwnership = false)]
    void TakeDamageServerRpc(int damageAmount)
    {
        p_life.Value -= damageAmount;
    }

    [Server]
    public void Death()
    {
        InstanceFinder.ServerManager.Despawn(gameObject);
        EventBusInitialiser.instance.Bus.InvokeEvent(new EnemyDyingEvent(_gridReaderId, _enemySpawnCost));
    }

    public void SetInfos(Guid _readerId, int cost)
    {
        _gridReaderId = _readerId;
        _enemySpawnCost = cost;
    }
}

public struct EnemyDyingEvent
{
    public Guid p_gridReaderId;
    public int p_enemySpawnCost;

    public EnemyDyingEvent(Guid id, int cost)
    {
        p_gridReaderId = id;
        p_enemySpawnCost = cost;
    }
}

