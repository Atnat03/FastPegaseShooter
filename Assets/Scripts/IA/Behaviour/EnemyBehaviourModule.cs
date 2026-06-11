public abstract class EnemyBehaviourModule : NetworkBusListener
{
    protected EnemyCore _enemyCore;

    public virtual void InitialiseBehaviourModule(EnemyCore enemyCore)
    {
        _enemyCore = enemyCore;
    }

    public virtual void OnNetworkTick(float tickDelta)
    {
        //cuts logic when spawning or dying
        if(_enemyCore.p_isSpawning || _enemyCore.p_isDying) return;
    }
}
