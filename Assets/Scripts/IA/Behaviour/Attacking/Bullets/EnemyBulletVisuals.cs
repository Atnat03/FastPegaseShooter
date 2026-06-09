using FishNet;
using UnityEngine;

public class EnemyBulletVisuals : MonoBusListener, IPoolable
{
    private Vector3 _startPos;
    private float _spawnTime;
    private bool _useGravity;
    
    private Vector3 _direction;
    private float _speed;

    private int _bulletId;
    private BulletTypes _bulletTypes;

    private EnemyBulletManager _enemyBulletManager;


    public void SetupVariables(Vector3 startPos, float spawnTime, bool useGravity, Vector3 direction, float speed, float bulletSize, int bulletId, BulletTypes bulletType, EnemyBulletManager EBM)
    {
        _enemyBulletManager = EBM;
            
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
                _enemyBulletManager.ReturnBulletToPool(this, _bulletTypes);
            }
        });
    }

    private void OnNetworkTick()
    {
        if(gameObject == null) return;
        
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

    public void Spawn() { }

    public void ReturnToPool()
    {
        UnsubscribeAll();
        
        if(InstanceFinder.TimeManager != null)
            InstanceFinder.TimeManager.OnTick -= OnNetworkTick;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        InstanceFinder.TimeManager.OnTick -= OnNetworkTick;
    }
}
