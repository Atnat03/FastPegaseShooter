using FishNet.Managing;
using FishNet.Object;
using UnityEngine;


public abstract class EnemyBehaviourModule : NetworkBusListener
{
    protected EnemyCore _enemyCore;

    public virtual void InitialiseBehaviourModule(EnemyCore enemyCore)
    {
        _enemyCore = enemyCore;
    }
    
    public virtual void OnNetworkTick(float tickDelta) {}
}
