using System;
using FishNet;
using UnityEngine;
using UnityEngine.Serialization;

//[AddComponentMenu("EnemyBehaviour/Attack")]
public abstract class EnemyAttackModule : EnemyBehaviourModule
{
    public Action<int, int> p_onHitPlayer;
    
    //HideInInspector to prevent draw with "base.OnInspectorGUI"
    //SerializeField to get properties in custom inspector 
    [HideInInspector, SerializeField] protected EnemyTargetModule _targetModule;
    [HideInInspector, SerializeField] protected AttackModuleSO _attackModuleSO;
    protected float _waitedTimeSinceAttack;

    protected abstract bool CanAttack(Vector3 shootingPos, Vector3 projectileDir);

    public override void OnNetworkTick(float tickDelta)
    {
        base.OnNetworkTick(tickDelta);
        
        _waitedTimeSinceAttack += (float)InstanceFinder.TimeManager.TickDelta;
    }

    protected float GetTargetSqrDistance()
    {
        return _targetModule.GetTargetSqrDistance(transform.position);
    }
}
