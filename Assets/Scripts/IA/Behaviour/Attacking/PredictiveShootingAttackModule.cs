using CustomConsole.Runtime.Logger;
using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Attack/PredictiveShootAttackModule")]
public class PredictiveShootingAttackModule : EnemyAttackModule
{
    [SerializeField] private float _bulletSize = 0.2f;
    [SerializeField] private float _bulletSpeed = 1;
    [SerializeField] private float _maxBulletLifeTime = 10f;

    [SerializeField] private int _bulletAmount = 1;
    [SerializeField] private float _shootingSpreadAngle = 0;
    

    public override void OnNetworkTick()
    {
        base.OnNetworkTick();
        if (_waitedTimeSinceAttack >= _attackDelay && _targetModule.HasTarget())
        {
            Vector3 shootDir = Vector3.zero;
            Vector3 shootingPos = transform.position + Vector3.up * 0.5f;
            if (TryGetShootingDirection(
                    _targetModule.p_fpsController.transform.position,
                    shootingPos,
                    _targetModule.p_fpsController.Rb.linearVelocity,
                    _bulletSpeed, out Vector3 shootingDirection))
            {
                //CustomLogger.HighlightLog("Using predictive shoot");
                shootDir = shootingDirection;
            }
            else
            {
                CustomLogger.CCErrorLog("Defaulted back to normal shoot");
                Vector3 delta = _targetModule.GetTargetPosition() - transform.position;
                float length = delta.magnitude;
                shootDir = delta / length;
            }
            
            if(!CanAttack(shootingPos, shootDir)) return;
            _waitedTimeSinceAttack = 0;
            
            InvokeEvent(
                new EnemyShootingEvent(
                    shootingPos, 
                    shootDir, 
                    _bulletSpeed, 
                    _damage, 
                    _bulletSize, 
                    _maxBulletLifeTime, 
                    this, 
                    _bulletAmount, 
                    _shootingSpreadAngle));
        }
    }

    public bool TryGetShootingDirection(Vector3 playerPosition, Vector3 shootingPosition, Vector3 playerVelocity, float bulletSpeed, out Vector3 bulletDirection)
    {
        Vector3 toTarget =  playerPosition - shootingPosition;
        
        float a = Vector3.Dot(playerVelocity, playerVelocity) - (bulletSpeed * bulletSpeed);
        float b = 2f * Vector3.Dot(toTarget, playerVelocity);
        float c = Vector3.Dot(toTarget, toTarget);
        if (MathUtilities.SolveQuadratic(a, b, c, out float root1, out float root2) == 0)
        {
            //called if no solution was found to predict the player position, can happen if the player is moving to quickly away from the shooting pos
            bulletDirection = Vector3.zero;
            return false;
        }
        if (root1 < 0 && root2 < 0)
        {
            bulletDirection = Vector3.zero;
            return false;
        }
        //using the positive root and ignoring the negative one
        float t;
        if (root1 < 0) t = root2;
        else if (root2 < 0) t = root1;
        else  t = Mathf.Min(root1, root2);

        float impactTime = t;
        Vector3 impactPosition = playerPosition+playerVelocity*impactTime;
        
        bulletDirection = (impactPosition - shootingPosition).normalized;
        return true;
    }

    protected override bool CanAttack(Vector3 shootingPos, Vector3 projectileDir)
    {
        if (GetTargetSqrDistance() > _maxPlayerDistance * _maxPlayerDistance)
        {
            return false;
        }

        return true;//HasLineOfSight(shootingPos, projectileDir);
    }

    bool HasLineOfSight(Vector3 shootingPos, Vector3 projectileDir)
    {
        Debug.DrawLine(shootingPos,shootingPos + projectileDir * _maxPlayerDistance, Color.red, _attackDelay);
        
        if (Physics.Raycast(shootingPos, projectileDir, out RaycastHit hit, _maxPlayerDistance, LayerMask.GetMask("Owner", "Other"), QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }
}
