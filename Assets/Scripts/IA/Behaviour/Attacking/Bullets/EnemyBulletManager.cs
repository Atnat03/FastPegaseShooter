using System;
using UnityEngine;
using FishNet;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemyBulletManager : NetworkBusListener
{
    [SerializeField] private GameObject _normalBulletPrefab;
    [SerializeField] private GameObject _splashBulletPrefab;
    [SerializeField] private GameObject _puddleBulletPrefab;
    
    public LayerMask _normalBulletLayerMask = int.MaxValue & ~(1 << 16); //enemy layer
    public LayerMask _splashBulletLayerMask = int.MaxValue & ~(1 << 6) & ~(1 << 7) & ~(1 << 16); //players layer and enemy layer
    public LayerMask _puddleBulletLayerMask = (1 << 6) | (1 << 7); //players layer 
    private Action _unsubscribeAction;

    private List<EnemyBullet> _spawnedBullets = new List<EnemyBullet>();
    private int _lastBulletId;
    
    public override void OnStartServer()
    {
        base.OnStartServer();

        InstanceFinder.TimeManager.OnTick += OnNetworkTick;
        ListenToEvent<EnemyShootingEvent>(AddBullets);
    }

    public override void OnStopServer()
    { 
        InstanceFinder.TimeManager.OnTick -= OnNetworkTick;
        _unsubscribeAction?.Invoke();

        base.OnStopServer();
    }

    //Executed at each network Tick only by server because of subscription
    private void OnNetworkTick()
    {
        float serverTime = InstanceFinder.TimeManager.Tick * (float)InstanceFinder.TimeManager.TickDelta;
        for(int i = _spawnedBullets.Count - 1; i >= 0; i--)
        {
            _spawnedBullets[i].UpdateBullet(serverTime);
            if(_spawnedBullets[i].ShouldBeDestroyed(serverTime))
            {
                KillVisualBulletObserverRPC(_spawnedBullets[i].p_bulletId);
                
                //replace RemoveAt by "swap remove" for performances
                _spawnedBullets.RemoveAt(i);
            }
        }
    }

    [Server]
    void AddBullets(EnemyShootingEvent ESE)
    {
        float networkTime = InstanceFinder.TimeManager.Tick * (float)InstanceFinder.TimeManager.TickDelta;

        BulletsSpawningInfos BSI = new BulletsSpawningInfos(ESE.p_startPos, ESE.p_bulletSpeed, ESE.p_bulletDamage, ESE.p_bulletSize, ESE.p_bulletMaxAliveTime, ESE.p_useGravity, ESE.p_bulletType);

        for (int i = 0; i < ESE.p_bulletAmount; i++)
        {
            Vector3 direction = RandomDirectionInCone(ESE.p_generalDirection, ESE.p_shootingSpreadAngle) * ESE.p_generalDirection.magnitude;

            EnemyBullet bullet;
            switch (ESE.p_bulletType)
            {
                case BulletTypes.Normal:
                    bullet = new NormalEnemyBullet(ESE, direction, networkTime, _lastBulletId, _normalBulletLayerMask);
                    break;
                case BulletTypes.Viscous:
                    bullet = new SplashEnemyBullet(ESE, direction, networkTime, _lastBulletId, _splashBulletLayerMask);
                    break;
                case BulletTypes.GooPuddle:
                    bullet = new PuddleEnemyBullet(ESE, direction, networkTime, _lastBulletId, _puddleBulletLayerMask);
                    break;
                
                default:
                    bullet = new NormalEnemyBullet( ESE, direction, networkTime, _lastBulletId, _normalBulletLayerMask);
                    break;
            }
            
            
            
            _spawnedBullets.Add(bullet);
            BSI.p_bulletsInfos.Add(new BulletBasicInfos(direction, _lastBulletId));
            
            _lastBulletId++;
        }
        
        SpawnVisualBulletObserverRPC(BSI, networkTime);
    }

    public Vector3 RandomDirectionInCone(Vector3 dir, float spreadAngle)
    {
        //reducing bullet concentration in center
        float cosTheta = Mathf.Cos(spreadAngle * Mathf.Deg2Rad);

        //choosing an angle between center of the cone (1) and the border (cosTheta)
        float z = Random.Range(cosTheta, 1f);
        //rotation angle
        float phi = Random.Range(0, 2 * Mathf.PI);

        float sinTheta = Mathf.Sqrt(1f - z * z);
        Vector3 localDir = new Vector3(sinTheta * Mathf.Cos(phi), sinTheta * Mathf.Sin(phi), z);
        return Quaternion.LookRotation(dir) * localDir;
    }

    //add needed parameters
    [ObserversRpc]
    void SpawnVisualBulletObserverRPC(BulletsSpawningInfos BSI, float spawnTime)
    {
        //here spawn the visual bullet for feedback
        //may use object pulling to reduce lag when instantiating GO
        foreach (BulletBasicInfos bulletInfo in BSI.p_bulletsInfos)
        {
            GameObject newBullet = Instantiate(GetObjectFromType(BSI.p_bulletType), BSI.p_startPos, Quaternion.identity);
            
            EnemyBulletVisuals EBV = newBullet.GetComponent<EnemyBulletVisuals>();
            EBV.SetupVariables(BSI.p_startPos, spawnTime, BSI.p_useGravity, bulletInfo.p_direction, BSI.p_bulletsSpeed, BSI.p_bulletsSize, bulletInfo.p_bulletId, BSI.p_bulletType);
        }
    }

    GameObject GetObjectFromType(BulletTypes type)
    {
        switch (type)
        {
            case BulletTypes.Normal:
                return _normalBulletPrefab;
            case BulletTypes.Viscous:
                return _splashBulletPrefab;
            case BulletTypes.GooPuddle:
                return _puddleBulletPrefab;
            
            default:
                return _normalBulletPrefab;
        }
    }

    [ObserversRpc]
    void KillVisualBulletObserverRPC(int bulletId)
    {
        InvokeEvent(new BulletDestructionEvent{p_bulletId = bulletId});
    }
}

public enum BulletTypes
{
    Normal,
    Viscous,
    GooPuddle
};

public struct BulletsSpawningInfos
{
    public List<BulletBasicInfos> p_bulletsInfos;
    
    public bool p_useGravity;
    public BulletTypes p_bulletType;
    
    
    public Vector3 p_startPos;
    public float p_bulletsSpeed;
    public int p_bulletsDamage;
    public float p_bulletsSize;
    public float p_bulletsMaxAliveTime;
    
    public BulletsSpawningInfos(Vector3 startPos, float speed, int damage, float size, float lifeTime, bool useGravity, BulletTypes bulletType)
    {
        p_bulletsInfos = new List<BulletBasicInfos>();
        
        p_useGravity = useGravity;
        p_bulletType = bulletType;
        
        p_startPos = startPos;
        p_bulletsSpeed = speed;
        p_bulletsDamage = damage;
        p_bulletsSize = size;
        p_bulletsMaxAliveTime = lifeTime;
    }
}
public struct BulletBasicInfos
{
    public Vector3 p_direction;
    public int p_bulletId;

    public BulletBasicInfos(Vector3 dir, int id)
    {
        p_direction = dir;
        p_bulletId = id;
    }
}
