using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyMovingModule : EnemyBehaviourModule
{
    [SerializeField] protected bool _doFreezeWithoutTarget = true;
    [SerializeField] protected EnemyTargetingModule _targetingModule;
    [SerializeField] protected float _speed = 3;
    
    protected List<PathfindingNode> _path = new List<PathfindingNode>();

    public virtual void OnNetworkTick()
    {
        if(!_targetingModule.HasTarget() && _doFreezeWithoutTarget) return;
        
        MoveAlongPath();
    }

    public virtual void OnPlayerMoving(int playerObjectId, Vector3 playerPosition, PathfindingGridReader gridReader)
    {
        if (!_targetingModule.IsMyTarget(playerObjectId)) return;
        
        //Updating Pathfinding
        _path = gridReader.GetPath(transform.position, playerPosition);
    }
    protected abstract void MoveAlongPath();
}
