using System;
using System.Collections.Generic;
using CustomConsole.Runtime.Logger;
using UnityEngine;
using UnityEngine.Serialization;

//[AddComponentMenu("EnemyBehaviour/Movement")]
public abstract class EnemyMovementModule : EnemyBehaviourModule
{
    //HideInInspector to prevent draw with "base.OnInspectorGUI"
    //SerializeField to get properties in custom inspector 
    [HideInInspector] [SerializeField] protected EnemyTargetModule _targetModule;
    [HideInInspector] public MovementModuleSO p_movementModuleSO;
    
    protected List<PathfindingNode> _path = new List<PathfindingNode>();
    private int _pathReservationId = -1;
    
    private bool _isPathUpdateRequested = false;

    public override void InitialiseBehaviourModule(EnemyCore enemyCore)
    {
        base.InitialiseBehaviourModule(enemyCore);
        _targetModule.p_onTargetPositionUpdate += PathUpdateRequest;
    }

    public override void OnNetworkTick(float tickDelta)
    {
        base.OnNetworkTick(tickDelta);
        
        if(!_targetModule.HasTarget() && p_movementModuleSO.p_doFreezeWithoutTarget) return;
        
        MoveAlongPath();
    }

    protected void PathUpdateRequest()
    {
        if (!_isPathUpdateRequested && _targetModule.HasTarget())
        {
            if(!_enemyCore.p_pathRequester) return;
            
            _isPathUpdateRequested = _enemyCore.p_pathRequester.TryRegisterPathRequest(
                new PathRequest(
                    _path.Count > 0 ?Vector3.SqrMagnitude(_path[0].position - _targetModule.GetTargetPosition()) : float.MaxValue,
                    RecalculatePathConcrete));
        }
    }

    protected virtual void RecalculatePathConcrete()
    {
        
        _enemyCore.p_gridReader.GetPath(
            transform.position, _targetModule.GetTargetPosition(), out _path);
        _isPathUpdateRequested = false;

        if (_path == null || _path.Count < 0)
        {
            _path = new List<PathfindingNode>();
            return;
        }
        
        _path.RemoveAt(_path.Count-1);
    }
    protected abstract void MoveAlongPath();
}
