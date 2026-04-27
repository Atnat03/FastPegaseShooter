using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Movement/SteeringBehaviourMovementModule")]

public class SteeringBehaviourMovementModule : EnemyMovementModule
{
    [SerializeField] private float _nodeValidationDistance = 0.1f;
    [SerializeField, Range(0, 1)] private float _steering = 0.5f; 
    
    private Vector3 _currentVelocity;

    protected override void RecalculatePath()
    {
        base.RecalculatePath();
        
        ClearPath();
    }

    void ClearPath()
    {
        if(_path.Count <= 2)return;

        for (int i = _path.Count - 2; i >= 0; i--)
        {
            if (CanRemoveMiddleStep(i - 1, i, i + 1))
            {
                _path.RemoveAt(i);
            }
        }
    }

    bool CanRemoveMiddleStep(int id1, int id2, int id3)
    {
        return Vector3.Cross((_path[id2].position - _path[id1].position), (_path[id3].position - _path[id1].position)).sqrMagnitude <
               0.001f;
    }

    protected override void MoveAlongPath()
    {
        if(_path.Count > 1)
        {
            Vector3 desiredVelocity = (_path[^2].position - transform.position).normalized.RemoveY() * _speed;
            Vector3 steeringForce = desiredVelocity - _currentVelocity;
            _currentVelocity += steeringForce.RemoveY()*Time.deltaTime;

            _currentVelocity = _currentVelocity.normalized * _speed;
            
            transform.position += _currentVelocity * Time.deltaTime;
            
            
            if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 30, LayerMask.GetMask("Ground")))
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            
            if (Vector3.SqrMagnitude(transform.position - _path[^2].position) <= _nodeValidationDistance * _nodeValidationDistance)
            {
                TryChangeTarget();
            }
        }
    }

    void TryChangeTarget()
    {
        _path.RemoveAt(_path.Count-1);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (PathfindingNode node in _path) {
            Gizmos.DrawSphere(node.position, 1f);
        }
    }
}
