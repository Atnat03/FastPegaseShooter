using System;
using UnityEngine;

public class EnemyLobShootingDebugger : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private LobShootingAttackModule LSAModule;

    const float steps = 30f;
    private const float simTime = 5f;
    private void OnDrawGizmos()
    {
        Vector3? shootVelocity = LSAModule.GetShootingVelocity(targetTransform.position);

        if (!shootVelocity.HasValue)
            return;
        
        Gizmos.color = Color.yellow;

        Vector3 startPos = transform.position;
        Vector3 velocity = shootVelocity.Value;
        
        Vector3 previousPoint = startPos;
        
        for (int i = 1; i < steps; i++)
        {
            float t = (i / steps) * simTime;
            Vector3 currentPoint = 
                startPos +
                velocity * t +
                0.5f * Physics.gravity * t * t;
            
            Gizmos.DrawLine(startPos, currentPoint);
            
            previousPoint = currentPoint;
        }
        Gizmos.color = Color.orange;
        Gizmos.DrawSphere(targetTransform.position, 0.2f);
    }
}
