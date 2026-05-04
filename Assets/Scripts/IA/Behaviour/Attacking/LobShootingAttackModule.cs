using UnityEngine;

public class LobShootingAttackModule : EnemyAttackModule
{
    [SerializeField] private float _bulletSize = 0.2f;
    [SerializeField] private float _bulletSpeed = 1;
    [SerializeField] private float _maxBulletLifeTime = 10f;

    [Header("Shooting")]
    [SerializeField] private float _shootingAngle = 30;

    static readonly float _g = Physics.gravity.y;
    
    

    public override void OnNetworkTick()
    {
        base.OnNetworkTick();
        if (_waitedTimeSinceAttack >= _attackDelay)
        {
            Vector3 delta = _targetModule.GetTargetPosition() - transform.position;
            float length = delta.magnitude;
            Vector3 dir = delta / length;

            Vector3 shootingPos = transform.position + Vector3.up * 0.5f;
            
            if(!CanAttack(shootingPos, dir)) return;
            _waitedTimeSinceAttack = 0;
            
            InvokeEvent(new EnemyShootingEvent(
                shootingPos, 
                dir, 
                _bulletSpeed, 
                _damage, 
                _bulletSize, 
                _maxBulletLifeTime, 
                this));
        }
    }

    protected override bool CanAttack(Vector3 shootingPos, Vector3 projectileDir)
    {
        if (GetTargetSqrDistance() > _maxPlayerDistance * _maxPlayerDistance)
        {
            return false;
        }

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

    float? GetLaunchingSpeed(Vector3 targetPosition)
    {
        float hDist = (transform.position - targetPosition).RemoveY().magnitude;
        float vDist = targetPosition.y - transform.position.y;

        float cosTheta = Mathf.Cos(_shootingAngle * Mathf.Deg2Rad);
        float tanTheta = Mathf.Tan(_shootingAngle * Mathf.Deg2Rad);
        
        float denominator = hDist * tanTheta - vDist;
        
        //no solution, target unreachable
        if (denominator >= 0) return null;
        
        float shootingSpeed =
            Mathf.Sqrt((_g * hDist * hDist) / (2 * cosTheta * cosTheta * (denominator)));

        return shootingSpeed;
    }

    public Vector3? GetShootingVelocity(Vector3 targetPos)
    {
        //Vector3 targetPos = _targetModule.GetTargetPosition();
        float radAngle = _shootingAngle * Mathf.Deg2Rad;
        float? launchResult = GetLaunchingSpeed(targetPos);
        
        if(launchResult == null)return null;
        float launchSpeed = launchResult.Value;
        
        Vector3 launchVelocity =
            (targetPos - transform.position).RemoveY().normalized * (launchSpeed * Mathf.Cos(radAngle));
        launchVelocity.y = launchSpeed * Mathf.Sin(radAngle);

        return launchVelocity;
    }
}
