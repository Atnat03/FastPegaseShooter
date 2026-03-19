using FishNet;
using UnityEngine;

public struct EnemyBullet
{
    private Vector3 _startPos;
    private Vector3 _direction;
    private float _speed;
    private float _spawnTime;
    private Vector3 _lastPosition;

    public int p_bulletId;
    public int p_bulletStrenght;

    public EnemyBullet(Vector3 startPos, Vector3 direction, float speed, float serverSpawnTime, int bulletId, int strenght)
    {
        _startPos = startPos;
        _lastPosition = startPos;
        _direction = direction;
        _speed = speed;
        _spawnTime = serverSpawnTime;
        p_bulletId = bulletId;
        p_bulletStrenght = strenght;
    }

    /// <summary>
    /// Move The bullet one step further
    /// </summary>
    /// <param name="serverTime">the current server time (since beginning)</param>
    /// <returns>True if a target was hit, false in the other case</returns>
    public bool MoveForward(float serverTime, out IDamagable damagableObject)
    {
        float networkTime = InstanceFinder.TimeManager.Tick * (float)InstanceFinder.TimeManager.TickDelta - _spawnTime;
        Vector3 position = _startPos + _direction * _speed * networkTime;

        if(DoCollide(_lastPosition, position, out damagableObject)) return true;
        else
        {
            _lastPosition = position;
            return false;
        }
    }

    bool DoCollide(Vector3 startPos, Vector3 endPos, out IDamagable damagableObject)
    {
        Vector3 delta = endPos - startPos;
        float length = delta.magnitude;
        Vector3 dir = delta / length;

        //May change CompareTag by layer checking
        if(Physics.Raycast(startPos, dir, out RaycastHit hit, length))
        {
            damagableObject = hit.collider.GetComponent<IDamagable>();
            if(damagableObject == null) return false;
            
            if(hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        damagableObject = null;
        return false;
    }
}
