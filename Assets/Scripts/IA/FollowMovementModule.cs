using System;
using UnityEngine;

public class FollowMovementModule : MonoBehaviour
{
    [SerializeField] private EnemyMovementModule _movementModule;

    private void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            _movementModule.transform.position,
            Time.deltaTime * _movementModule.p_movementModuleSO.p_speed);
    }
}
