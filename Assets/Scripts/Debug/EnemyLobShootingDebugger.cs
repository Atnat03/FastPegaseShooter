using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLobShootingDebugger : MonoBehaviour
{
    [SerializeField] private List<Transform> targetTransform = new List<Transform>();
    [SerializeField] private LobShootingAttackModule LSAModule;
    [SerializeField] private bool _useComponentTarget;

    const float steps = 60f;
    private const float simTime = 2f;
    

    private void OnDrawGizmos()
    {
        if (!LSAModule) return;
        Gizmos.color =  Color.yellow;
        if (_useComponentTarget)
        {
            Vector3 targetPosition = LSAModule.GetTargetPosition();
            Vector3? shootVelocity = LSAModule.GetShootingVelocity(targetPosition);

            if (!shootVelocity.HasValue)
                return;
            
            Vector3 startPos = transform.position + LSAModule.p_shootingOffset;
            Vector3 velocity = shootVelocity.Value;

            // Direction horizontale vers la cible
            Vector3 flatToTarget = (targetPosition - startPos).RemoveY();
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
            Gizmos.DrawSphere(targetPosition, 0.1f);   
            
        }
        else
        {
            foreach (Transform tr in targetTransform)
            {
                Vector3? shootVelocity = LSAModule.GetShootingVelocity(tr.position);

                if (!shootVelocity.HasValue)
                    return;
                
                Vector3 startPos = transform.position;
                Vector3 velocity = shootVelocity.Value;

                // Direction horizontale vers la cible
                Vector3 flatToTarget = (tr.position - startPos).RemoveY();
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
                Gizmos.DrawSphere(tr.position, 0.1f);   
            }
        }
    }
}
