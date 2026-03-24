using System;
using FishNet;
using UnityEngine;

public class EnemyBulletVisuals : MonoBehaviour
{
    private Vector3 _startPos;
    private Vector3 _direction;
    private float _speed;
    private float _spawnTime;
    private float _damage = 10;

    private int _bulletId;

    private Action _unsubscribeAction;

    public void SetupVariables(Vector3 startPos, Vector3 direction, float speed, float spawnTime, int bulletId, float damage)
    {
        _startPos = startPos;
        _direction = direction;
        _speed = speed;
        _spawnTime = spawnTime;
        _damage = damage;

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
    }

    //collision with player shouldn't be done in visual updater
    /*public void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out PlayerHealth player))
        {
            EventBusInitialiser.instance.Bus.InvokeEvent(new PlayerTakeDamageEvent
            {
                playerN = player.NetworkObject,
                value = _damage
            });
        }
        KillBullet();
    }*/
}
