using UnityEngine;

public abstract class EnemyBullet
{
    //Positions
    protected Vector3 _startPos;
    protected Vector3 _lastPosition;
    protected Vector3 _currentPosition;
    
    //trajectory
    protected Vector3 _direction;
    protected float _speed;
    protected float _spawnTime;
    protected float _maxLifeTime;
    
    //general
    public int p_bulletId;
    private bool _collided = false;
    protected LayerMask _layerMask;
    
    //Damage
    protected float _bulletSize;
    public int p_bulletDamage;
    public EnemyAttackModule p_attackModule;
    
    
    public EnemyBullet(EnemyShootingEvent ESE,
        Vector3 direction, float spawnTime, int bulletId,
        LayerMask layerMask)
    {
        
        _startPos = ESE.p_startPos;
        _lastPosition = ESE.p_startPos;
        _currentPosition = ESE.p_startPos;
        
        _direction = direction;
        _speed = ESE.p_bulletSpeed;
        _spawnTime = spawnTime;
        _maxLifeTime = ESE.p_bulletMaxAliveTime;
        
        p_bulletId = bulletId;
        _layerMask = layerMask;
        
        p_bulletDamage = ESE.p_bulletDamage;
        _bulletSize = ESE.p_bulletSize;
        p_attackModule = ESE.p_enemyAttackModule;
    }

    public virtual void UpdateBullet(float serverTime)
    {
        _currentPosition = GetNewPosition(serverTime);

        _collided = DoCollide(_lastPosition, _currentPosition, out RaycastHit hit);
        if(_collided) ManageCollision(hit);
        
        _lastPosition =  _currentPosition;
    }

    /// <summary>
    /// Move The bullet one step further
    /// </summary>
    /// <param name="serverTime">the current server time (since beginning)</param>
    /// <returns>True if a target was hit, false in the other case</returns>
    protected abstract Vector3 GetNewPosition(float serverTime);

    
    protected virtual bool DoCollide(Vector3 startPos, Vector3 endPos, out RaycastHit hit)
    {
        Vector3 delta = endPos - startPos;
        float length = delta.magnitude;
        Vector3 dir = delta / length;

        if(Physics.SphereCast(startPos,  _bulletSize,dir,
               out hit, length, _layerMask, QueryTriggerInteraction.Ignore))
        {
            return true;
        }
        return false;
    }

    public virtual bool ShouldBeDestroyed(float serverTime)
    {
        //if the bullet was alive for too long
        //or collided with something
        return (serverTime - _spawnTime > _maxLifeTime) || _collided;
    }

    protected abstract void ManageCollision(RaycastHit hit);
}
