using UnityEngine;

public class DistanceAwareMovingModule : EnemyMovingModule
{
    [SerializeField] private float _idealDistance;
    private Vector3 _lastPos;
    private float _t;

    public override void OnPlayerMoving(int playerObjectId, Vector3 playerPosition, PathfindingGridReader gridReader)
    {
        base.OnPlayerMoving(playerObjectId, playerPosition, gridReader);
        
        //only called when playerObjectId is the target
        _lastPos = transform.position;
        _t = 0;
    }


    protected override void MoveAlongPath()
    {
        //cuts Execution if the enemy is close enough from the player
        if(_path.Count > 1 &&
           _targetingModule.GetTargetSqrDistance(transform.position) >= _idealDistance*_idealDistance)
        {
            transform.position = Vector3.Lerp(_lastPos, _path[^2].position, _t);
            _t += Time.deltaTime * _speed;
            if (_t >= 1)
            {
                _t = 0;
                _path.RemoveAt(_path.Count - 1);
                if (_path.Count > 0) _lastPos = _path[^1].position;
            }
        }
    }
}
