using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Life/WeakPointLifeModule")]
public class WeakPointLifeModule : EnemyLifeModule
{
    //HideInInspector to prevent draw with "base.OnInspectorGUI"
    //SerializeField to get properties in custom inspector
    [HideInInspector][SerializeField] private EnemyLifeModule _enemyLifeModule;
    
    [SerializeField] private float _damageMult = 2;
    
    [Server]
    public override bool TakeDamage(int attackerObjectId, int rawDamageAmount, ChargeType charge, bool isCritical = false)
    {
        base.TakeDamage(attackerObjectId, rawDamageAmount, charge, isCritical);
        if (IsServerInitialized)
        {
            int damage = Mathf.RoundToInt(GetDamageAmount(rawDamageAmount) * _damageMult);
            _enemyLifeModule.TakeDamage(attackerObjectId, damage, charge,  true);
        }

        //Damages are always critical when done on weak point
        return true;
    }
}
