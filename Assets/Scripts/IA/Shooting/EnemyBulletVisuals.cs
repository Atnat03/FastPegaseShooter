using System;
using FishNet;
using UnityEngine;

public class EnemyBulletVisuals : MonoBehaviour
{
    private Vector3 _startPos;
    private Vector3 _direction;
    private float _speed;
    private float _spawnTime;

    private int _bulletId;

    private float _maxEnabledTime = 5f;
    private Action _unsubscribeAction;

    public void SetupVariables(Vector3 startPos, Vector3 direction, float speed, float spawnTime, int bulletId)
    {
        _startPos = startPos;
        _direction = direction;
        _speed = speed;
        _spawnTime = spawnTime;
        
        _bulletId = bulletId;
        
        transform.position = _startPos;
        
        InstanceFinder.TimeManager.OnTick += OnNetworkTick;
        _unsubscribeAction = EventBusInitialiser.instance.Bus.Subscribe((BulletDestructionEvent BDE) =>
        {
            if (this != null && BDE.p_bulletId == _bulletId)
            {
                _unsubscribeAction();
                KillBullet();
            }
        });
    }

    public void KillBullet()
    {
        if(gameObject != null)
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if(InstanceFinder.TimeManager != null)
            InstanceFinder.TimeManager.OnTick -= OnNetworkTick;
    }

    private void OnNetworkTick()
    {
        float networkTime = InstanceFinder.TimeManager.Tick * (float)InstanceFinder.TimeManager.TickDelta - _spawnTime;
        transform.position = _startPos + _direction * _speed * networkTime;

        //only for debug
        if (networkTime >= _maxEnabledTime)
        {
            KillBullet();
        }
    }
}
