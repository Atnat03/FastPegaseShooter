using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

//[AddComponentMenu("EnemyBehaviour/Movement")]
public abstract class EnemyMovementModule : EnemyBehaviourModule
{
    //HideInInspector to prevent draw with "base.OnInspectorGUI"
    //SerializeField to get properties in custom inspector 
    [HideInInspector] [SerializeField] protected bool _doFreezeWithoutTarget = true;
    [HideInInspector] [SerializeField] protected EnemyTargetModule _targetModule;
    [HideInInspector] [SerializeField] protected float _speed = 3;
    [HideInInspector] [SerializeField] protected int _traceWeight = 9;
    [HideInInspector] [SerializeField] protected int _traceSpread = 3;
    
    protected List<PathfindingNode> _path = new List<PathfindingNode>();
    private int _pathReservationId = -1;
    
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
        PathUpdateRequest();
    }

    protected void PathUpdateRequest()
    {
        if (!_isPathUpdateRequested)
        {
            if(!_enemyCore.p_pathRequester) return;
            
            _isPathUpdateRequested = _enemyCore.p_pathRequester.TryRegisterPathRequest(
                new PathRequest(
                    _path.Count > 0 ?Vector3.SqrMagnitude(_path[0].position - _targetPosition) : float.MaxValue,
                    RecalculatePathConcrete));
        }
    }
    
    public void ClearPathReservation()
    {
        if (_pathReservationId < 0) return;
        _enemyCore.p_gridReader.ClearPathReservation(_pathReservationId);
        _pathReservationId = -1;
    }

    protected virtual void RecalculatePathConcrete()
    {
        ClearPathReservation();
        
        _enemyCore.p_gridReader.GetAndRegisterPath(
            transform.position, _targetPosition, _traceWeight, _traceSpread,
            out _path, out _pathReservationId);
        _isPathUpdateRequested = false;
    }
    protected abstract void MoveAlongPath();
}
