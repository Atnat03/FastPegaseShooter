using System;
using UnityEngine;

public class EnemyLobShootingDebugger : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private LobShootingAttackModule LSAModule;

    const float steps = 30f;
    private const float simTime = 2f;
    
    private void OnDrawGizmos()
    {
        Vector3? shootVelocity = LSAModule.GetShootingVelocity(targetTransform.position);

        if (!shootVelocity.HasValue)
            return;

        Gizmos.color = Color.yellow;

        Vector3 startPos = transform.position;
        Vector3 velocity = shootVelocity.Value;

        // Direction horizontale vers la cible
        Vector3 flatToTarget = (targetTransform.position - startPos).RemoveY();
        float targetDistance = flatToTarget.magnitude;
        Vector3 targetDir = flatToTarget.normalized;

        Vector3 previousPoint = startPos;

        for (int i = 1; i < steps; i++)
        {
            float t = i * (simTime / steps);

            Vector3 currentPoint =
                startPos +
                velocity * t +
                0.5f * Physics.gravity * t * t;

            // Distance parcourue horizontalement dans la direction de la cible
            Vector3 flatOffset = (currentPoint - startPos).RemoveY();
            float projectedDistance = Vector3.Dot(flatOffset, targetDir);

            // On coupe dès qu'on a dépassé la cible
            if (projectedDistance > targetDistance)
                break;

            Gizmos.DrawLine(previousPoint, currentPoint);

            previousPoint = currentPoint;
        }

        Gizmos.color = Color.orange;
        Gizmos.DrawSphere(targetTransform.position, 0.2f);
    }
}
