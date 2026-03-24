using System;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class EliteWeakPoint : EnemyLifeModule
{
    [SerializeField] private EnemyLifeModule _enemyLifeModule;
    [SerializeField] private float _eliteDamageMultWhenDestroyed = 1;
    
    [Server]
    public override bool TakeDamage(int rawDamageAmount, bool isCritical = false)
    {
        base.TakeDamage(rawDamageAmount, isCritical);
        if (IsServerInitialized)
        {
            int damages = GetDamageAmount(rawDamageAmount);
            p_life.Value -= damages;
        }

        //Damages are always critical when done on weak point
        return true;
    }

    [Server]
    public override void Death(int takenDamages)
    {
        base.Death(takenDamages);
        
        int damages = Mathf.RoundToInt(takenDamages * _eliteDamageMultWhenDestroyed);
        CustomLogger.HighlightLog($"Weak point hit damages : {damages}");
        _enemyLifeModule.TakeDamage(damages, true);
        
        InstanceFinder.ServerManager.Despawn(gameObject);
    }
}
