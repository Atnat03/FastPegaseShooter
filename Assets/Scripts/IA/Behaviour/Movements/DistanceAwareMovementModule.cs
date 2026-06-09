using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Movement/DistanceAwareMovementModule")]
public class DistanceAwareMovementModule : EnemyMovementModule
{
    [SerializeField] private DistanceAwareMovementModuleSO _distanceAwareModuleSO;
    protected override void MoveAlongPath()
    {
        //cuts Execution if the enemy is close enough from the player
        if(_path.Count > 0 &&
           _targetModule.GetTargetSqrDistance(transform.position) >= _distanceAwareModuleSO.p_idealDistance*_distanceAwareModuleSO.p_idealDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, _path[^1].position, p_movementModuleSO.p_speed * Time.deltaTime);
            
            
            if ((_path[^1].position - transform.position).sqrMagnitude <= 0.01f)
            {
                _path.RemoveAt(_path.Count - 1);
            }
        }
    }
}
