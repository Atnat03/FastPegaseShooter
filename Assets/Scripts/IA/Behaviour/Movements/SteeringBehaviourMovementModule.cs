using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Movement/SteeringBehaviourMovementModule")]

public class SteeringBehaviourMovementModule : EnemyMovementModule
{
    [Header("Seek Steering")]
    [SerializeField] private float _nodeValidationDistance = 0.5f;
    [SerializeField] private AnimationCurve _seekSteeringCurve;
    [SerializeField] private float _seekSteeringStrenght = 6;
    [Header("Wall Steering")]
    [SerializeField] private float _WallViewDistance = 1;
    [SerializeField] private float _agentWidth = 0.5f;
    [SerializeField] private float _wallSteeringStrenght = 200;
    [SerializeField] private LayerMask _ignoredLayers = 0b10000000011000000;//Owner, Other, Enemy
    [Header("Scatter Steering")]
    [SerializeField] private float _enemyViewDistance = 60;
    [SerializeField] private float _enemySteeringStrenght = 10;
    [SerializeField] private LayerMask _enemyLayers = 0b10000000000000000;//Enemy
    [Header("Path Cleaning")]
    [SerializeField] private int _maxSkippingStep = 3;
    
    private Vector3 _currentVelocity;

    protected override void RecalculatePathConcrete()
    {
        base.RecalculatePathConcrete();
        
        
        ClearPath();
    }

    void ClearPath()
    {
        if(_path.Count <= 2)return;
        
        int skippedStep = 0;
        for (int i = _path.Count - 2; i >= 1; i--)
        {
            if (skippedStep <= _maxSkippingStep && CanRemoveMiddleStep(i - 1, i, i + 1))
            {
                _path.RemoveAt(i);
                skippedStep++;
            }
            else skippedStep = 0;
        }
    }

    bool CanRemoveMiddleStep(int id1, int id2, int id3)
    {
        return Vector3.Cross((_path[id2].position - _path[id1].position), (_path[id3].position - _path[id1].position)).sqrMagnitude <
               0.001f;
    }

    protected override void MoveAlongPath()
    {
        return;
        
        if(_path.Count >= 1)
        {
            _currentVelocity += (GetSeekSteeringForce() + GetWallSteeringForce() + GetEnemyScatterSteeringForce()) * Time.deltaTime;

            _currentVelocity = _currentVelocity.normalized * p_movementModuleSO.p_speed;
            
            transform.position += _currentVelocity * Time.deltaTime;
            
            if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 30, LayerMask.GetMask("Ground")))
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            
            if (Vector3.SqrMagnitude(transform.position - _path[^1].position) <= _nodeValidationDistance * _nodeValidationDistance)
            {
                TryChangeTarget();
            }
        }
    }

    Vector3 GetSeekSteeringForce()
    {
        Vector3 desiredVelocity = (_path[^1].position - transform.position).normalized.RemoveY() * p_movementModuleSO.p_speed;
        Vector3 seekSteeringForce = (desiredVelocity - _currentVelocity) * _seekSteeringStrenght;
        float dirChangeProp = 
            (Vector3.Dot(_currentVelocity.normalized, desiredVelocity.normalized) + 1) *0.5f;
        seekSteeringForce *= _seekSteeringCurve.Evaluate(dirChangeProp);
        
        Debug.DrawRay(Vector3.up * 4 + transform.position, _currentVelocity, Color.green);
        Debug.DrawRay(Vector3.up * 4 + transform.position + _currentVelocity, seekSteeringForce, Color.blue);
        Debug.DrawRay(Vector3.up * 4 + transform.position, desiredVelocity.normalized, Color.red);
        
        return seekSteeringForce.RemoveY();
    }

    Vector3 GetWallSteeringForce()
    {
        if (Physics.SphereCast(transform.position+Vector3.up*0.5f, _agentWidth, _currentVelocity, out RaycastHit hit,
                _WallViewDistance, ~_ignoredLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.Log(hit.collider.gameObject.name);
            Vector3 obstacleAvoidanceSteeringForce = hit.normal * (1 - (hit.distance / _WallViewDistance)) * _wallSteeringStrenght;
            
            Debug.DrawRay(Vector3.up * 4 + transform.position + _currentVelocity, obstacleAvoidanceSteeringForce, Color.cyan);
            return obstacleAvoidanceSteeringForce.RemoveY();
        }
        else return Vector3.zero;
    }
    
    Vector3 GetEnemyScatterSteeringForce()
    {
        Vector3 scatterForce = Vector3.zero;
        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, _enemyViewDistance, _enemyLayers);
        foreach (Collider enemy in enemyColliders)
        {
            Vector3 toEnemy = (transform.position - enemy.transform.position).RemoveY();
            scatterForce += Vector3.Lerp(toEnemy.normalized * 0.1f, toEnemy.normalized, toEnemy.sqrMagnitude / _enemyViewDistance);
        }

        Debug.DrawRay(Vector3.up * 4 + transform.position + _currentVelocity, scatterForce.normalized * _enemySteeringStrenght, Color.purple);
        return scatterForce.normalized * _enemySteeringStrenght;
    }

    void TryChangeTarget()
    {
        _path.RemoveAt(_path.Count-1);
    }

    private void OnDrawGizmos()
    {
        foreach (PathfindingNode node in _path) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(node.position, 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(node.position, _nodeValidationDistance);
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position+Vector3.up*0.5f, _agentWidth);
        Gizmos.DrawWireSphere(transform.position+Vector3.up*0.5f+_currentVelocity.normalized*_WallViewDistance, _agentWidth);
        Gizmos.DrawWireSphere(transform.position+Vector3.up*0.5f+ (_currentVelocity.normalized * _WallViewDistance)*0.5f, _agentWidth);
        Gizmos.DrawRay(transform.position+Vector3.up*0.5f, _currentVelocity.normalized);
    }
}
