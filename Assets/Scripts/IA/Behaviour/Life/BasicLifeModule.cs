
using FishNet.Object;
using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Life/BasicLifeModule")]
public class BasicLifeModule : EnemyLifeModule
{

    [Server]
    public override bool TakeDamage(int attackerObjectId, int rawDamageAmount, ChargeType charge, bool isCritical = false)
    {
        base.TakeDamage(attackerObjectId, rawDamageAmount, charge,  isCritical);
        if (IsServerInitialized)
        {
            int damages = GetDamageAmount(rawDamageAmount);
            p_life.Value -= damages;
        }

        //No specific logic modifying critical behaviour
        return isCritical;
    }
    
    [Server]
    public override void Death(int attackerObjectId, ChargeType charge)
    {
        base.Death(attackerObjectId, charge);
        
        _enemyCore.KillEnemy(attackerObjectId, charge);
    }
}

