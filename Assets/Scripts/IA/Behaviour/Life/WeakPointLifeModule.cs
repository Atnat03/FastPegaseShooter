using System;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class WeakPointLifeModule : EnemyLifeModule
{
    [SerializeField] private EnemyLifeModule _enemyLifeModule;
    [SerializeField] private float _eliteDamageMultWhenDestroyed = 1;
    
    [Server]
    public override bool TakeDamage(int attackerObjectId, int rawDamageAmount, bool isCritical = false)
    {
        base.TakeDamage(attackerObjectId, rawDamageAmount, isCritical);
        if (IsServerInitialized)
        {
            int damages = GetDamageAmount(rawDamageAmount);
            p_life.Value -= damages;
            
            if(p_life.Value <= 0)
            {
                int damage = Mathf.RoundToInt(GetDamageAmount(rawDamageAmount) * _eliteDamageMultWhenDestroyed);
                CustomLogger.HighlightLog($"Weak point hit damages : {damage}");
                _enemyLifeModule.TakeDamage(attackerObjectId, rawDamageAmount, isCritical);
            }
        }

        //Damages are always critical when done on weak point
        return true;
    }

    [Server]
    public override void Death(int takenDamages)
    {
        base.Death(takenDamages);
        
        
        InstanceFinder.ServerManager.Despawn(gameObject);
    }
}
