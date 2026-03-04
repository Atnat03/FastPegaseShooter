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

    [Server]
    public void TakeDamage(int damageAmount)
    {
        p_life.Value -= damageAmount;
    }

    [Server]
    public void Death()
    {
        InstanceFinder.ServerManager.Despawn(gameObject);
    }
}