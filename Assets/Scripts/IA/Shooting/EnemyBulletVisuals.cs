using System;
using FishNet;
using UnityEngine;

public class EnemyBulletVisuals : MonoBehaviour
{
    private Vector3 _startPos;
    private Vector3 _direction;
    private float _speed;
    private float _spawnTime;

    private float _maxEnabledTime = 5f;

    public void SetupVariables(Vector3 startPos, Vector3 direction, float speed, float spawnTime)
    {
        _startPos = startPos;
        _direction = direction;
        _speed = speed;
        _spawnTime = spawnTime;
        
        transform.position = _startPos;
        
        InstanceFinder.TimeManager.OnTick += OnNetworkTick;
    }

    public void KillBullet()
    {
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        InstanceFinder.TimeManager.OnTick -= OnNetworkTick;
    }

    private void OnNetworkTick()
    {
        float networkTime = InstanceFinder.TimeManager.Tick * (float)InstanceFinder.TimeManager.TickDelta - _spawnTime;
        transform.position = _startPos + _direction * _speed * networkTime;
        Debug.Log($"speed: {_speed}");

        //only for debug
        if (networkTime >= _maxEnabledTime)
        {
            Debug.Log(networkTime);
            KillBullet();
        }
    }
}
