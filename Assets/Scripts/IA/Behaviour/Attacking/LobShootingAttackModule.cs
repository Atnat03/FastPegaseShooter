using UnityEngine;

public class LobShootingAttackModule : EnemyAttackModule
{
    [Header("Bullets")]
    [SerializeField] private float _bulletSize = 0.2f;
    [SerializeField] private float _bulletSpeed = 1;
    [SerializeField] private float _maxBulletLifeTime = 10f;

    [Header("Shooting")]
    [SerializeField, Range(5,85)] private float _shootingAngle = 30;
    public Vector3 p_shootingOffset = Vector3.up * 0.5f;
    public float p_splashSize = 3;
    public float p_splashDuration = 30;
    public float p_splashDamageDelay = 1;

    static readonly float _g = -Physics.gravity.y;
    
    

    public override void OnNetworkTick(float tickDelta)
    {
        base.OnNetworkTick(tickDelta);
        if (_waitedTimeSinceAttack >= _attackDelay)
        {
            Vector3? shootingInitialVelocity = GetShootingVelocity(_targetModule.GetTargetPosition());
            if(!shootingInitialVelocity.HasValue) return;

            Vector3 shootingPos = transform.position + p_shootingOffset;
            
            if(!CanAttack(shootingPos, shootingInitialVelocity.Value)) return;
            _waitedTimeSinceAttack = 0;
            
            InvokeEvent(new EnemyShootingEvent(
                shootingPos,
                shootingInitialVelocity.Value,
                _bulletSpeed,
                _damage,
                _bulletSize,
                _bulletType,
                _maxBulletLifeTime,
                this,
                _projectileUseGravity
                ));
        }
    }

    protected override bool CanAttack(Vector3 shootingPos, Vector3 projectileDir)
    {
        if (GetTargetSqrDistance() > _maxPlayerDistance * _maxPlayerDistance)
        {
            return false;
        }

        return true;
    }

    float? GetLaunchingSpeed(Vector3 targetPosition)
    {
        float hDist = ((transform.position + p_shootingOffset) - targetPosition).RemoveY().magnitude;
        float vDist = targetPosition.y - (transform.position.y + p_shootingOffset.y);

        float cosTheta = Mathf.Cos(_shootingAngle * Mathf.Deg2Rad);
        float tanTheta = Mathf.Tan(_shootingAngle * Mathf.Deg2Rad);
        
        float denominator = hDist * tanTheta - vDist;
        
        //no solution, target unreachable
        if (denominator <= 0) return null;
        
        float shootingSpeed =
            Mathf.Sqrt((_g * hDist * hDist) / (2 * cosTheta * cosTheta * denominator));

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
            (targetPos - (transform.position+p_shootingOffset)).RemoveY().normalized * (launchSpeed * Mathf.Cos(radAngle));
        launchVelocity.y = launchSpeed * Mathf.Sin(radAngle);

        return launchVelocity;
    }

    protected override void Reset()
    {
        base.Reset();
        _projectileUseGravity = true;
        _bulletType = BulletTypes.Splash;
    }

    public Vector3 GetTargetPosition() => _targetModule.GetTargetPosition();
}
