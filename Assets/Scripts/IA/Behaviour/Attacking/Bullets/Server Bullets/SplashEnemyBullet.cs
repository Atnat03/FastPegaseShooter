using UnityEngine;

public class SplashEnemyBullet : EnemyBullet
{
    public SplashEnemyBullet(EnemyShootingEvent ESE, Vector3 direction, float spawnTime, int bulletId, LayerMask layerMask)
        : base(ESE, direction, spawnTime, bulletId, layerMask)
    {}

    protected override Vector3 GetNewPosition(float serverTime)
    {
        float t = (serverTime - _spawnTime) * _speed;
        return _startPos + 
               _direction * t + 
               0.5f * Physics.gravity * t * t;
    }

    protected override void ManageCollision(RaycastHit hit)
    {
        LobShootingAttackModule LSAModule = p_attackModule as LobShootingAttackModule;
        
        EventBus.InvokeEvent(
            new EnemyShootingEvent(
            hit.point,
            Vector3.up,
            0,
            p_bulletDamage,
            LSAModule.p_splashSize,
            BulletTypes.GooPuddle,
            LSAModule.p_splashDuration,
            p_attackModule,
            false));
    }
}
