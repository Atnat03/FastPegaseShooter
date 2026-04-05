using System;
using System.Collections.Generic;
using Controller;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Life/BasicLifeModule")]
public class BasicLifeModule : EnemyLifeModule
{

    [Server]
    public override bool TakeDamage(int attackerObjectId, int rawDamageAmount, bool isCritical = false)
    {
        base.TakeDamage(attackerObjectId, rawDamageAmount, isCritical);
        
        if (IsServerInitialized)
        {
            int damages = GetDamageAmount(rawDamageAmount);
            p_life.Value -= damages;
            if (isCritical)
            {
                //
            }
        }

        //No specific logic modifying critical behaviour
        return isCritical;
    }
    [Server]
    public override void Death(int takenDamages)
    {
        base.Death(takenDamages);
        InstanceFinder.ServerManager.Despawn(gameObject);
        InvokeEvent(new EnemyDyingEvent(_enemyCore.p_gridReaderId, _enemyCore.p_enemySpawnCost, _enemyCore));
    }
}

public struct EnemyDyingEvent
{
    public Guid p_gridReaderId;
    public int p_enemySpawnCost;
    public EnemyCore p_enemyCore;

    public EnemyDyingEvent(Guid id, int cost, EnemyCore core)
    {
        p_gridReaderId = id;
        p_enemySpawnCost = cost;
        p_enemyCore = core;
    }
}

