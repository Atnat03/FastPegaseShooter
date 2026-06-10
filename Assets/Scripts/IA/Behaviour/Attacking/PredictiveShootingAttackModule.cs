using CustomConsole.Runtime.Logger;
using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Attack/PredictiveShootAttackModule")]
public class PredictiveShootingAttackModule : EnemyAttackModule
{
    [SerializeField] private PredictiveShootingAttackModuleSO _predictiveShootingAttackModuleSO;
    

    public override void OnNetworkTick(float tickDelta)
    {
        base.OnNetworkTick(tickDelta);
        if (_waitedTimeSinceAttack >= _attackModuleSO.p_attackDelay && _targetModule.HasTarget())
        {
            Vector3 shootDir = Vector3.zero;
            Vector3 shootingPos = transform.position + Vector3.up * 0.5f;
            if (TryGetShootingDirection(
                    _targetModule.p_playerVisualBridge.transform.position,
                    shootingPos,
                    _targetModule.p_playerVisualBridge.FPSController.Rb.linearVelocity,
                    _predictiveShootingAttackModuleSO.p_bulletSpeed, out Vector3 shootingDirection))
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
                    _predictiveShootingAttackModuleSO.p_bulletSpeed, 
                    _attackModuleSO.p_damage, 
                    _predictiveShootingAttackModuleSO.p_bulletSize, 
                    _attackModuleSO.p_bulletType,
                    _predictiveShootingAttackModuleSO.p_maxBulletLifeTime, 
                    this, 
                    _attackModuleSO.p_projectileUseGravity,
                    _predictiveShootingAttackModuleSO.p_bulletAmount, 
                    _predictiveShootingAttackModuleSO.p_shootingSpreadAngle));
            p_onAttack?.Invoke();
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
        if (GetTargetSqrDistance() > _attackModuleSO.p_maxPlayerDistance * _attackModuleSO.p_maxPlayerDistance)
        {
            return false;
        }

        return true;//HasLineOfSight(shootingPos, projectileDir);
    }

    bool HasLineOfSight(Vector3 shootingPos, Vector3 projectileDir)
    {
        Debug.DrawLine(shootingPos,shootingPos + projectileDir * _attackModuleSO.p_maxPlayerDistance, Color.red, _attackModuleSO.p_attackDelay);
        
        if (Physics.Raycast(shootingPos, projectileDir, out RaycastHit hit, _attackModuleSO.p_maxPlayerDistance, LayerMask.GetMask("Owner", "Other"), QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }
}
