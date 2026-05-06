using System;
using FishNet;
using UnityEngine;

public class EnemyBulletVisuals : MonoBusListener
{
    private Vector3 _startPos;
    private float _spawnTime;
    private bool _useGravity;
    
    private Vector3 _direction;
    private float _speed;

    private int _bulletId;
    private BulletTypes _bulletTypes;


    public void SetupVariables(Vector3 startPos, float spawnTime, bool useGravity, Vector3 direction, float speed, float bulletSize, int bulletId, BulletTypes bulletType)
    {
        _startPos = startPos;
        _spawnTime = spawnTime;
        _useGravity = useGravity;
        
        _direction = direction;
        _speed = speed;

        _bulletId = bulletId;
        _bulletTypes = bulletType;
        
        transform.position = _startPos;
        
        transform.localScale = bulletType == BulletTypes.GooPuddle ? new Vector3(bulletSize, transform.localScale.y, bulletSize) : Vector3.one * bulletSize;
        
        InstanceFinder.TimeManager.OnTick += OnNetworkTick;
        ListenToEvent<BulletDestructionEvent>(BDE =>
        {
            if (this != null && BDE.p_bulletId == _bulletId)
            {
                KillBullet();
            }
        });
    }

    public void KillBullet()
    {
        if(gameObject != null)
            Destroy(gameObject);
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if(InstanceFinder.TimeManager != null)
            InstanceFinder.TimeManager.OnTick -= OnNetworkTick;
    }

    private void OnNetworkTick()
    {
        float networkTime = InstanceFinder.TimeManager.Tick * (float)InstanceFinder.TimeManager.TickDelta - _spawnTime;
        networkTime *= _speed;
        
        if(!_useGravity)
            transform.position = _startPos + _direction * networkTime;
        else
        {
            transform.position = _startPos +
                                 _direction * networkTime +
                                 0.5f * Physics.gravity * networkTime * networkTime;
        }
    }
}
