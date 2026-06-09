using UnityEngine;

public class FollowMovementModuleViewer : MonoBehaviour
{
    [SerializeField] private EnemyMovementModule _movementModule;
    [SerializeField] private float _rotationSpeed = 5.0f;
    
    private void Update()
    {
        if(_movementModule)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                _movementModule.transform.position,
                Time.deltaTime * _movementModule.p_movementModuleSO.p_speed);

            if(_movementModule.GetNextTargetPosition().HasValue)
                transform.rotation = Quaternion.Slerp(
                transform.rotation,
                
                Quaternion.LookRotation(
                    (transform.position.RemoveY() - 
                     _movementModule.GetNextTargetPosition().Value.RemoveY())
                        .normalized),
                
                Time.deltaTime * _rotationSpeed);

        }
    }
}
