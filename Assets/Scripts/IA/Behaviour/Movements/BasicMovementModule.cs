using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Movement/BasicMovementModule")]
public class BasicMovementModule : EnemyMovementModule
{
    private Vector3 _lastPos;
    private float _t;

    protected override void RecalculatePathConcrete()
    {
        base.RecalculatePathConcrete();
        _lastPos = transform.position;
        _t = 0;
    }


    protected override void MoveAlongPath()
    {
        if(_path.Count > 1)
        {
            if (!_isWalking)
            {
                _isWalking = true;
                p_onChangeMovement(_isWalking);
            }
            
            transform.position = Vector3.Lerp(_lastPos, _path[^2].position, _t);
            _t += Time.deltaTime * p_movementModuleSO.p_speed;
            if (_t >= 1)
            {
                _t = 0;
                _path.RemoveAt(_path.Count - 1);
                if (_path.Count > 0)
                {
                    _lastPos = _path[^1].position;
                }
            }
        }
        else if (_isWalking)
        {
            _isWalking = false;
            p_onChangeMovement(_isWalking);
        }
    }
}
