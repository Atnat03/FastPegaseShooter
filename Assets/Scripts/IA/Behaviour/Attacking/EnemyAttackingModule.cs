using CustomConsole.Runtime.Logger;
using FishNet;
using UnityEngine;

public abstract class EnemyAttackingModule : EnemyBehaviourModule
{
    [SerializeField] protected EnemyTargetingModule _targetingModule;
    
    [SerializeField] protected int _damage = 10;
    [SerializeField] protected float _attackDelay = 2f;
    protected float _waitedTimeSinceAttack;

    protected abstract bool CanAttack();

    public virtual void OnNetworkTick()
    {
        _waitedTimeSinceAttack += (float)InstanceFinder.TimeManager.TickDelta;
        _targetingModule.OnNetworkTick();
    }

    protected float GetTargetSqrDistance()
    {
        return _targetingModule.GetTargetSqrDistance(transform.position);
    }
}
