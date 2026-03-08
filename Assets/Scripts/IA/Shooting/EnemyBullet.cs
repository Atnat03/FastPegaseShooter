using FishNet;
using UnityEngine;

public struct EnemyBullet
{
    private Vector3 _startPos;
    private Vector3 _direction;
    private float _speed;
    private float _spawnTime;
    private Vector3 _lastPosition;

    public EnemyBullet(Vector3 startPos, Vector3 direction, float speed, float serverSpawnTime)
    {
        _startPos = startPos;
        _lastPosition = startPos;
        _direction = direction;
        _speed = speed;
        _spawnTime = serverSpawnTime;
    }

    public bool MoveForward(float serverTime)
    {
        float networkTime = InstanceFinder.TimeManager.Tick * (float)InstanceFinder.TimeManager.TickDelta - _spawnTime;
        Vector3 position = _startPos + _direction * _speed * networkTime;

        if(DoCollide(_lastPosition, position)) return true;
        else
        {
            _lastPosition = position;
            return false;
        }
    }

    bool DoCollide(Vector3 startPos, Vector3 endPos)
    {
        Vector3 delta = endPos - startPos;
        float length = delta.magnitude;
        Vector3 dir = delta / length;

        //May change CompareTag by layer checking
        if(Physics.Raycast(startPos, dir, out RaycastHit hit, length))
        {
            if(hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
}
