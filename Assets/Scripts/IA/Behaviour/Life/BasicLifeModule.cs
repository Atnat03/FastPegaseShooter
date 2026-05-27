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
    public override bool TakeDamage(int attackerObjectId, int rawDamageAmount, EnemyCore.ChargeType charge, bool isCritical = false)
    {
        if(!CanReceiveDamage(charge)) return false;
        
        base.TakeDamage(attackerObjectId, rawDamageAmount, charge,  isCritical);
        
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
        //InvokeEvent(new EnemyDyingEvent(_enemyCore.p_gridReaderId, _enemyCore.p_enemySpawnCost, _enemyCore));
    }
}

