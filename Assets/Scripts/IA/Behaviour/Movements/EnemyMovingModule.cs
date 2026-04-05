using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class EnemyMovingModule : EnemyBehaviourModule
{
    //HideInInspector to prevent draw with "base.OnInspectorGUI"
    //SerializeField to get properties in custom inspector 
    [HideInInspector] [SerializeField] protected bool _doFreezeWithoutTarget = true;
    [HideInInspector] [SerializeField] protected EnemyTargetModule _targetModule;
    [HideInInspector] [SerializeField] protected float _speed = 3;
    
    protected List<PathfindingNode> _path = new List<PathfindingNode>();
    
    private bool _isPathUpdateRequested = false;
    private Vector3 _targetPosition;

    public virtual void OnNetworkTick()
    {
        if(!_targetModule.HasTarget() && _doFreezeWithoutTarget) return;
        
        MoveAlongPath();
    }

    public virtual void OnPlayerMoving(int playerObjectId, Vector3 playerPosition)
    {
        if (!_targetModule.IsMyTarget(playerObjectId)) return;
        
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
