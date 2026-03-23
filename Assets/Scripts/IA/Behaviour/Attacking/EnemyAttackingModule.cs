using CustomConsole.Runtime.Logger;
using FishNet;
using UnityEngine;

public abstract class EnemyAttackingModule : EnemyBehaviourModule
{
    [SerializeField] protected EnemyTargetingModule _targetingModule;
    
    [SerializeField] protected int _damage = 10;
    [SerializeField] protected float _attackDelay = 2f;
    protected float _waitedTimeSinceAttack;

    public override void OnStartServer()
    {
        base.OnStartServer();
        InstanceFinder.TimeManager.OnTick += OnNetworkTick;
    }
    
    public override void OnStopServer()
    {
        InstanceFinder.TimeManager.OnTick -= OnNetworkTick;
        
        base.OnStopServer();
    }

    protected abstract bool CanAttack();

    protected virtual void OnNetworkTick()
    {
        CustomLogger.HighlightLog("on networkTime attack");
        _waitedTimeSinceAttack += (float)InstanceFinder.TimeManager.TickDelta;
    }

    protected float GetTargetSqrDistance()
    {
        float dist = (_targetingModule.GetTargetPosition() - transform.position).sqrMagnitude;
        CustomLogger.HighlightLog($"target distance : {dist}");
        return dist;
    }
}
