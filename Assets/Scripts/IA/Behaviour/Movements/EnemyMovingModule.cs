using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyMovingModule : EnemyBehaviourModule
{
    [SerializeField] protected bool _doFreezeWithoutTarget = true;
    [SerializeField] protected EnemyTargetingModule _targetingModule;
    [SerializeField] protected float _speed = 3;
    
    protected List<PathfindingNode> _path = new List<PathfindingNode>();
    
    private bool _isPathUpdateRequested = false;
    private Vector3 _targetPosition;

    public virtual void OnNetworkTick()
    {
        if(!_targetingModule.HasTarget() && _doFreezeWithoutTarget) return;
        
        MoveAlongPath();
    }

    public virtual void OnPlayerMoving(int playerObjectId, Vector3 playerPosition)
    {
        if (!_targetingModule.IsMyTarget(playerObjectId)) return;
        
        _targetPosition = playerPosition;
        
        //Updating Pathfinding
        if (!_isPathUpdateRequested)
        {
            if(!_enemyCore.p_pathRequester) return;
            
            _isPathUpdateRequested = true;
            _enemyCore.p_pathRequester.RegisterPathRequest(new PathRequest{p_AuthorizePathRequest = RecalculatePath});
        }
    }

    protected virtual void RecalculatePath()
    {
        _path = _enemyCore.p_gridReader.GetPath(transform.position, _targetPosition);
        _isPathUpdateRequested = false;
    }
    protected abstract void MoveAlongPath();
}
