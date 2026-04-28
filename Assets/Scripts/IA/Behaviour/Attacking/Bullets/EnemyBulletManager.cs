using System;
using UnityEngine;
using FishNet;
using FishNet.Object;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class EnemyBulletManager : NetworkBusListener
{
    [SerializeField] private GameObject _bulletPrefab;
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
            if(_spawnedBullets[i].MoveForward(serverTime, out PlayerHealth playerHealth))
            {
                //here, apply target hitting logic
                if(playerHealth != null)
                {
                    InvokeEvent(new PlayerTakeDamageEvent
                    {
                        p_playerN = playerHealth.NetworkObject,
                        p_value = _spawnedBullets[i].p_bulletStrenght
                    });
                    _spawnedBullets[i].p_attackModule.p_onHitPlayer?.Invoke(
                        playerHealth.NetworkObject.ObjectId,
                        _spawnedBullets[i].p_bulletStrenght);
                }
                
                KillVisualBulletObserverRPC(_spawnedBullets[i].p_bulletId);
                
                //replace RemoveAt by "swap remove" for performances
                _spawnedBullets.RemoveAt(i);
            }
            else if (_spawnedBullets[i].ShouldBeDestroyed(serverTime))
            {
                KillVisualBulletObserverRPC(_spawnedBullets[i].p_bulletId);
                _spawnedBullets.RemoveAt(i);
            }
        }
    }

    [Server]
    void AddBullets(EnemyShootingEvent ESE)
    {
        float networkTime = InstanceFinder.TimeManager.Tick * (float)InstanceFinder.TimeManager.TickDelta;

        BulletsSpawningInfos BSI = new BulletsSpawningInfos(ESE.p_startPos, ESE.p_bulletSpeed, ESE.p_bulletDamage, ESE.p_bulletSize, ESE.p_bulletMaxAliveTime);

        for (int i = 0; i < ESE.p_bulletAmount; i++)
        {
            Vector3 direction = RandomDirectionInCone(ESE.p_generalDirection, ESE.p_shootingSpreadAngle);
            EnemyBullet bullet = new EnemyBullet(ESE.p_startPos, direction, ESE.p_bulletSpeed, ESE.p_bulletSize,
                networkTime, ESE.p_bulletMaxAliveTime,_lastBulletId,
                ESE.p_bulletDamage, ESE.p_enemyAttackModule);
            
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
            GameObject newBullet = Instantiate(_bulletPrefab, BSI.p_startPos, Quaternion.identity);
            
            EnemyBulletVisuals EBV = newBullet.GetComponent<EnemyBulletVisuals>();
            EBV.SetupVariables(BSI.p_startPos, bulletInfo.p_direction, BSI.p_bulletsSpeed, BSI.p_bulletsSize, spawnTime, bulletInfo.p_bulletId, BSI.p_bulletsDamage);
            
        }
    }

    [ObserversRpc]
    void KillVisualBulletObserverRPC(int bulletId)
    {
        InvokeEvent(new BulletDestructionEvent{p_bulletId = bulletId});
    }
}

public struct BulletDestructionEvent
{
    public int p_bulletId;
}

public struct BulletsSpawningInfos
{
    public List<BulletBasicInfos> p_bulletsInfos;
    
    public Vector3 p_startPos;
    public float p_bulletsSpeed;
    public int p_bulletsDamage;
    public float p_bulletsSize;
    public float p_bulletsMaxAliveTime;
    
    public BulletsSpawningInfos(Vector3 startPos, float speed, int damage, float size, float lifeTime)
    {
        p_bulletsInfos = new List<BulletBasicInfos>();
        
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
