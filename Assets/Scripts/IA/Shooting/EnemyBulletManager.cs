using System;
using UnityEngine;
using FishNet;
using FishNet.Object;
using System.Collections.Generic;
using CustomConsole.Runtime.Logger;

public class EnemyBulletManager : NetworkBehaviour
{
    private EventBus _bus;
    private List<EnemyBullet> _spawnedBullets = new List<EnemyBullet>();

    private Action _unsubscribeAction;
    [SerializeField] private GameObject _bulletPrefab;

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

    //Executed at each network Tick
    private void OnNetworkTick()
    {
        float networkTime = (float)InstanceFinder.TimeManager.Tick * (float)InstanceFinder.TimeManager.TickDelta;
        for(int i = _spawnedBullets.Count - 1; i >= 0; i--)
        {
            if(_spawnedBullets[i].MoveForward(networkTime))
            {
                //here, apply target hitting logic


                //replace RemoveAt by "swap remove" for performences
                _spawnedBullets.RemoveAt(i);
                CustomLogger.HighlightLog("Ammo hit player !");
            }
        }
    }

    [Server]
    void AddBullet(EnemyShootingEvent ESE)
    {
        float networkTime = InstanceFinder.TimeManager.Tick * (float)InstanceFinder.TimeManager.TickDelta;
        EnemyBullet bullet = new EnemyBullet(ESE.p_startPos, ESE.p_direction, ESE.p_speed, networkTime);

        _spawnedBullets.Add(bullet);
        SpawnVisualBulletObserverRPC(ESE, networkTime);
    }

    //add needed parameters
    [ObserversRpc]
    void SpawnVisualBulletObserverRPC(EnemyShootingEvent ESE, float spawnTime)
    {
        //here spawn the visual bullet for feedback
        //may use object pulling to reduce lag when instantiating GO
        GameObject newBullet = Instantiate(_bulletPrefab, ESE.p_startPos, Quaternion.identity);
        
        EnemyBulletVisuals EBV = newBullet.GetComponent<EnemyBulletVisuals>();
        EBV.SetupVariables(ESE.p_startPos, ESE.p_direction, ESE.p_speed, spawnTime);
    }
}
