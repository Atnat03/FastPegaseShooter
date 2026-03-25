using System;
using UnityEngine;

public abstract class EnemyMovingModule : EnemyBehaviourModule
{
    [SerializeField] protected EnemyTargetingModule _targetingModule;

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public virtual void OnNetworkTick()
    {
        _targetingModule.OnNetworkTick();
        if(!_targetingModule.HasTarget()) return;
        
        MoveAlongPath();
    }
    
    public abstract void OnPlayerMoving(int playerObjectId, Vector3 playerPosition, PathfindingGridReader gridReader);
    protected abstract void MoveAlongPath();
}
