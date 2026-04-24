using System;
using CustomConsole.Runtime.Logger;
using FishNet;
using UnityEngine;

public class EnemyBullet
{
    private Vector3 _startPos;
    private Vector3 _direction;
    private float _speed;
    private float _spawnTime;
    private Vector3 _lastPosition;
    private float _maxLifeTime;
    private float _bulletSize;

    public int p_bulletId;
    public int p_bulletStrenght;
    public EnemyAttackModule p_attackModule;

    public EnemyBullet(Vector3 startPos, Vector3 direction, float speed, float bulletSize,
        float serverSpawnTime, float maxLifeTime,
        int bulletId, int strenght, EnemyAttackModule attackModule)
    {
        _startPos = startPos;
        _lastPosition = startPos;
        _direction = direction;
        _speed = speed;
        _spawnTime = serverSpawnTime;
        p_bulletId = bulletId;
        p_bulletStrenght = strenght;
        _maxLifeTime = maxLifeTime;
        p_attackModule = attackModule;
        _bulletSize = bulletSize;
    }

    /// <summary>
    /// Move The bullet one step further
    /// </summary>
    /// <param name="serverTime">the current server time (since beginning)</param>
    /// <returns>True if a target was hit, false in the other case</returns>
    public bool MoveForward(float serverTime, out PlayerHealth playerHealth)
    {
        float networkTime = serverTime - _spawnTime;
        Vector3 position = _startPos + (_direction * _speed * networkTime);

        if(DoCollide(_lastPosition, position, out playerHealth)) return true;
        
        _lastPosition = position;
        return false;
    }

    bool DoCollide(Vector3 startPos, Vector3 endPos, out PlayerHealth playerHealthObject)
    {
        Vector3 delta = endPos - startPos;
        float length = delta.magnitude;
        Vector3 dir = delta / length;

        //May change CompareTag by layer checking
        if(Physics.SphereCast(startPos,  _bulletSize,dir, out RaycastHit hit, length))
        {
            PlayerVisuelBridge PVB = hit.collider.GetComponent<PlayerVisuelBridge>();
            if(PVB == null)
            {
                playerHealthObject = null;
                return false;
            }
            playerHealthObject = PVB.PlayerHealth;
            
            if(hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        playerHealthObject = null;
        return false;
    }

    public bool ShouldBeDestroyed(float serverTime)
    {
        //if the bullet was alive for too long
        return serverTime - _spawnTime > _maxLifeTime;
    }
}
