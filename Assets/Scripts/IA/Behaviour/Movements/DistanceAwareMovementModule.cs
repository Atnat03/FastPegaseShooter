using CustomConsole.Runtime.Logger;
using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Movement/DistanceAwareMovementModule")]
public class DistanceAwareMovementModule : EnemyMovementModule
{
    [SerializeField] private float _idealDistance;
    
    protected override void MoveAlongPath()
    {
        //cuts Execution if the enemy is close enough from the player
        if(_path.Count >= 1 &&
           _targetModule.GetTargetSqrDistance(transform.position) >= _idealDistance*_idealDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, _path[^1].position, _speed * Time.deltaTime);
            
            
            if ((_path[^1].position - transform.position).sqrMagnitude <= 0.01f)
            {
                _path.RemoveAt(_path.Count - 1);
            }
        }
    }
}
