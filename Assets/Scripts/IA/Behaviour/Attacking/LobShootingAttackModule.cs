using UnityEngine;

public class LobShootingAttackModule : EnemyAttackModule
{
    public LobShootingAttackModuleSO _lobShootingAttackModuleSo;

    public override void OnNetworkTick(float tickDelta)
    {
        base.OnNetworkTick(tickDelta);
        if (_waitedTimeSinceAttack >= _attackModuleSO.p_attackDelay)
        {
            Vector3? shootingInitialVelocity = GetShootingVelocity(_targetModule.GetTargetPosition());
            if(!shootingInitialVelocity.HasValue) return;

            Vector3 shootingPos = transform.position + _lobShootingAttackModuleSo.p_shootingOffset;
            
            if(!CanAttack(shootingPos, shootingInitialVelocity.Value)) return;
            _waitedTimeSinceAttack = 0;
            
            InvokeEvent(new EnemyShootingEvent(
                shootingPos,
                shootingInitialVelocity.Value,
                _lobShootingAttackModuleSo.p_bulletSpeed,
                _attackModuleSO.p_damage,
                _lobShootingAttackModuleSo.p_bulletSize,
                _attackModuleSO.p_bulletType,
                _lobShootingAttackModuleSo.p_maxBulletLifeTime,
                this,
                _attackModuleSO.p_projectileUseGravity
                ));
        }
    }

    protected override bool CanAttack(Vector3 shootingPos, Vector3 projectileDir)
    {
        if (GetTargetSqrDistance() > _attackModuleSO.p_maxPlayerDistance * _attackModuleSO.p_maxPlayerDistance)
        {
            return false;
        }

        return true;
    }

    float? GetLaunchingSpeed(Vector3 targetPosition)
    {
        float hDist = ((transform.position + _lobShootingAttackModuleSo.p_shootingOffset) - targetPosition).RemoveY().magnitude;
        float vDist = targetPosition.y - (transform.position.y + _lobShootingAttackModuleSo.p_shootingOffset.y);

        float cosTheta = Mathf.Cos(_lobShootingAttackModuleSo.p_shootingAngle * Mathf.Deg2Rad);
        float tanTheta = Mathf.Tan(_lobShootingAttackModuleSo.p_shootingAngle * Mathf.Deg2Rad);
        
        float denominator = hDist * tanTheta - vDist;
        
        //no solution, target unreachable
        if (denominator <= 0) return null;
        
        float shootingSpeed =
            Mathf.Sqrt((LobShootingAttackModuleSO._g * hDist * hDist) / (2 * cosTheta * cosTheta * denominator));

        return shootingSpeed;
    }

    public Vector3? GetShootingVelocity(Vector3 targetPos)
    {
        //Vector3 targetPos = _targetModule.GetTargetPosition();
        float radAngle = _lobShootingAttackModuleSo.p_shootingAngle * Mathf.Deg2Rad;
        float? launchResult = GetLaunchingSpeed(targetPos);
        
        if(launchResult == null)return null;
        float launchSpeed = launchResult.Value;
        
        Vector3 launchVelocity =
            (targetPos - (transform.position+_lobShootingAttackModuleSo.p_shootingOffset)).RemoveY().normalized * (launchSpeed * Mathf.Cos(radAngle));
        launchVelocity.y = launchSpeed * Mathf.Sin(radAngle);

        return launchVelocity;
    }

    protected override void Reset()
    {
        base.Reset();
        _attackModuleSO.p_projectileUseGravity = true;
        _attackModuleSO.p_bulletType = BulletTypes.Viscous;
    }

    public Vector3 GetTargetPosition() => _targetModule.GetTargetPosition();
}
