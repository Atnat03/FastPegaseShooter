using UnityEngine;

public class PuddleEnemyBullet : EnemyBullet
{
    public PuddleEnemyBullet(EnemyBulletManager EBM, EnemyShootingEvent ESE, Vector3 direction, float spawnTime,
        int bulletId)
        : base(EBM, ESE, direction, spawnTime, bulletId)
    {}

    protected override Vector3 GetNewPosition(float serverTime)
    {
        //Puddle doesn't move over time
        return _startPos;
    }

    protected override bool DoCollide(Vector3 startPos, Vector3 endPos, out RaycastHit hit)
    {
        //return base.DoCollide(startPos, endPos, out hit);
        
        Vector3 delta = endPos - startPos;
        float length = delta.magnitude;
        Vector3 dir = delta / length;

        Collider[] colliders = Physics.OverlapBox(
            _currentPosition,
            new Vector3(_bulletSize * 0.5f, 0.25f, _bulletSize * 0.5f),
            Quaternion.identity, _enemyBulletManager.p_puddleBullerLayerMask);

        if (colliders.Length > 0)
        {
            Physics.Raycast(_currentPosition, colliders[0].transform.position - _currentPosition, out hit, length);
            if(hit.distance <= _bulletSize*0.5f)
                return true;
        }

        hit = new RaycastHit();
        return false;
        
    }

    protected override void ManageCollision(RaycastHit hit)
    {
        Debug.Log("PLAYER HIT PUDDLE");
    }
    public override bool ShouldBeDestroyed(float serverTime)
    {
        //if the bullet was alive for too long
        //or collided with something
        return (serverTime - _spawnTime > _maxLifeTime);
    }
}
