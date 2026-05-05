using CustomConsole.Runtime.Logger;
using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Movement/DistanceAwareMovementModule")]
public class DistanceAwareMovementModule : EnemyMovementModule
{
    [SerializeField] private float _idealDistance;
    private Vector3 _lastPos;
    private float _t;

    [Header("-----Testing-----")]
    [SerializeField] private int _maxTraverseNodeBeforePathUpdate = 3;
    private int _walkedNodeSinceRecalculated;

    protected override void RecalculatePathConcrete()
    {
        base.RecalculatePathConcrete();
        _lastPos = transform.position;
        _t = 0;
        _walkedNodeSinceRecalculated = 0;
    }


    protected override void MoveAlongPath()
    {
        //cuts Execution if the enemy is close enough from the player
        if(_path.Count > 1 &&
           _targetModule.GetTargetSqrDistance(transform.position) >= _idealDistance*_idealDistance)
        {
            transform.position = Vector3.Lerp(_lastPos, _path[^2].position, _t);
            _t += Time.deltaTime * _speed;
            if (_t >= 1)
            {
                _t = 0;
                _path.RemoveAt(_path.Count - 1);
                _walkedNodeSinceRecalculated++;
                if (_path.Count > 0) _lastPos = transform.position;
            }
        }

        if (_walkedNodeSinceRecalculated >= _maxTraverseNodeBeforePathUpdate)
        {
            PathUpdateRequest();
        }
    }
}
