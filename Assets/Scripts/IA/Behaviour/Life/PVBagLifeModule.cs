using System;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Life/PVBagLifeModule")]
public class PVBagLifeModule : EnemyLifeModule
{
    //HideInInspector to prevent draw with "base.OnInspectorGUI"
    //SerializeField to get properties in custom inspector
    [HideInInspector][SerializeField] private EnemyLifeModule _enemyLifeModule;
    
    [SerializeField] private float _damageMultWhenDestroyed = 1;
    
    [Server]
    public override bool TakeDamage(int attackerObjectId, int rawDamageAmount, ChargeType charge, bool isCritical = false)
    {
        base.TakeDamage(attackerObjectId, rawDamageAmount, charge, isCritical);
        
        if (IsServerInitialized)
        {
            int damages = GetDamageAmount(rawDamageAmount);
            p_life.Value -= damages;
            
            if(p_life.Value <= 0)
            {
                int damage = Mathf.RoundToInt(damages * _damageMultWhenDestroyed);
                _enemyLifeModule.TakeDamage(attackerObjectId, damage, charge,  isCritical);
            }
        }

        //Damages are always critical when done on PV Bag
        return true;
    }

    [Server]
    public override void Death(int attackerObjectId, ChargeType charge)
    {
        base.Death(attackerObjectId, charge);
        
        
        InstanceFinder.ServerManager.Despawn(gameObject);
    }
}
