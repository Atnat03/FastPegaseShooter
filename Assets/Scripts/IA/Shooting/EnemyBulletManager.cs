using System;
using UnityEngine;
using FishNet;
using FishNet.Object;
using System.Collections.Generic;

public class EnemyBulletManager : NetworkBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    private EventBus _bus;
    private Action _unsubscribeAction;

    private List<EnemyBullet> _spawnedBullets = new List<EnemyBullet>();
    private int _lastBulletId;
    
    void Awake()
    {
        //Possible Because changed Script Execution Order
        _bus = EventBusInitialiser.instance.Bus;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log($"bus : {_bus == null}");

        InstanceFinder.TimeManager.OnTick += OnNetworkTick;
        _unsubscribeAction = _bus.Subscribe((EnemyShootingEvent ESE) => AddBullet(ESE));
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
                EventBusInitialiser.instance.Bus.InvokeEvent(new PlayerTakeDamageEvent
                {
                    playerN = playerHealth.NetworkObject,
                    value = _spawnedBullets[i].p_bulletStrenght
                });
                
                
                //EnemyBullet bullet = _spawnedBullets[i];
                //playerHealth.TakeDamage(bullet.p_bulletStrenght);
                KillVisualBulletObserverRPC(_spawnedBullets[i].p_bulletId);
                
                //replace RemoveAt by "swap remove" for performences
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
    void AddBullet(EnemyShootingEvent ESE)
    {
        float networkTime = InstanceFinder.TimeManager.Tick * (float)InstanceFinder.TimeManager.TickDelta;
        EnemyBullet bullet = new EnemyBullet(ESE.p_startPos, ESE.p_direction, ESE.p_speed, networkTime, ESE.p_aliveTime,_lastBulletId, ESE.p_damage);

        _lastBulletId++;
        
        _spawnedBullets.Add(bullet);
        SpawnVisualBulletObserverRPC(ESE, networkTime, bullet.p_bulletId);
    }

    //add needed parameters
    [ObserversRpc]
    void SpawnVisualBulletObserverRPC(EnemyShootingEvent ESE, float spawnTime, int bulletId)
    {
        //here spawn the visual bullet for feedback
        //may use object pulling to reduce lag when instantiating GO
        GameObject newBullet = Instantiate(_bulletPrefab, ESE.p_startPos, Quaternion.identity);
        
        EnemyBulletVisuals EBV = newBullet.GetComponent<EnemyBulletVisuals>();
        EBV.SetupVariables(ESE.p_startPos, ESE.p_direction, ESE.p_speed, spawnTime, bulletId, ESE.p_damage);
    }

    [ObserversRpc]
    void KillVisualBulletObserverRPC(int bulletId)
    {
        _bus.InvokeEvent(new BulletDestructionEvent{p_bulletId = bulletId});
    }
}

public struct BulletDestructionEvent
{
    public int p_bulletId;
}
