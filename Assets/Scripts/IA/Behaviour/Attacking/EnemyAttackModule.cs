using System;
using FishNet;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class EnemyAttackModule : EnemyBehaviourModule
{
    public Action<int, int> p_onHitPlayer;
    
    //HideInInspector to prevent draw with "base.OnInspectorGUI"
    //SerializeField to get properties in custom inspector 
    [HideInInspector][SerializeField] protected EnemyTargetModule _targetModule;
    [HideInInspector][SerializeField] protected int _damage = 10;
    [HideInInspector][SerializeField] protected float _attackDelay = 2f;
    [HideInInspector][SerializeField] protected float _maxPlayerDistance = 10f;
    protected float _waitedTimeSinceAttack;

    protected abstract bool CanAttack();

    public virtual void OnNetworkTick()
    {
        _waitedTimeSinceAttack += (float)InstanceFinder.TimeManager.TickDelta;
    }

    protected float GetTargetSqrDistance()
    {
        return _targetModule.GetTargetSqrDistance(transform.position);
    }
}
