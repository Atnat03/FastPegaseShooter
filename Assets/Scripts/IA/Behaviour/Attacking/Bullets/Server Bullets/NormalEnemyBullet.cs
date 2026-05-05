using UnityEngine;

public class NormalEnemyBullet : EnemyBullet
{
    public NormalEnemyBullet(EnemyShootingEvent ESE, Vector3 direction, float spawnTime, int bulletId, LayerMask layerMask)
        : base(ESE, direction, spawnTime, bulletId, layerMask)
    {}

    protected override Vector3 GetNewPosition(float serverTime)
    {
        float t = serverTime - _spawnTime;
        Vector3 position = _startPos + (_direction * _speed * t);
        
        return position;
    }

    protected override void ManageCollision(RaycastHit hit)
    {
        if(hit.collider.gameObject.TryGetComponent(out PlayerVisuelBridge PVB))
        {
            EventBus.InvokeEvent(new PlayerTakeDamageEvent
            {
                p_playerN = PVB.NetworkObject,
                p_value = p_bulletDamage
            });
            
            p_attackModule.p_onHitPlayer?.Invoke(PVB.NetworkObject.ObjectId, p_bulletDamage);
        }
    }
}
