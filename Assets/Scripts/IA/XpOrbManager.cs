using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

public class XpOrbManager : NetworkBusListener
{
    [SerializeField] private EnergyOrb _energyOrbPrefab;
    [SerializeField] private float _orbSpawnRadius = 0.5f;
    [SerializeField] private float _orbVerticalOffset = 0.1f;
    [SerializeField] private float _maxXpInOrb = 5f;
    
    [Header("----- Materials -----")]
    [SerializeField] private Material _positiveMat;
    [SerializeField] private Material _negativeMat;
    
    private Pooler<EnergyOrb> _orbPool;
    Dictionary<int, EnergyOrb> _spawnedOrbs = new();
    private int _lastOrbId;
    
    public override void OnStartServer()
    {
        base.OnStartServer();

        ListenToEvent<OnEnemyDieEvent>(OEDE =>
        {
            if (OEDE.p_energyToDropInOrb == 0)
                return;

            AddXpOrbs(
                OEDE.p_enemy.transform.position,
                OEDE.p_energyToDropInOrb);
        });
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        _orbPool = new Pooler<EnergyOrb>(_energyOrbPrefab, 5);
    }

    private void Update()
    {
        foreach (EnergyOrb orb in _spawnedOrbs.Values)
        {
            orb.UpdateOrb();
        }
    }

    [Server]
    void AddXpOrbs(Vector3 position, float rawAmount)
    {
        AddXpOrbObserverRpc(position, rawAmount);
    }

    [ObserversRpc]
    void AddXpOrbObserverRpc(Vector3 position, float rawAmount)
    {
        float amount = Mathf.Abs(rawAmount);

        while (amount > _maxXpInOrb)
        {
            EnergyOrb newOrb = _orbPool.Spawn(GetSpawnPosition(position), Quaternion.identity);
            newOrb.SetUpOrb(
                _lastOrbId,
                rawAmount < 0 ? -_maxXpInOrb : _maxXpInOrb,
                rawAmount < 0 ? _negativeMat : _positiveMat,
                this
            );
            
            _spawnedOrbs.Add(_lastOrbId, newOrb);
            _lastOrbId++;
            
            amount -= _maxXpInOrb;
        }

        EnergyOrb orb = _orbPool.Spawn(GetSpawnPosition(position), Quaternion.identity);
        orb.SetUpOrb(
            _lastOrbId,
            rawAmount < 0 ? -amount : amount,
            rawAmount < 0 ? _negativeMat : _positiveMat,
            this
        );
        _spawnedOrbs.Add(_lastOrbId++, orb);
        _lastOrbId++;
    }

    Vector3 GetSpawnPosition(Vector3 centralPos)
    {
        Vector3 offSet = Random.insideUnitSphere * _orbSpawnRadius;
        offSet.y = _orbVerticalOffset;
        return centralPos + offSet;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReturnOrbToPoolServerRpc(int orbId)
    {
        ReturnOrbToPoolObserverRpc(orbId);
    }
    
    [ObserversRpc]
    void ReturnOrbToPoolObserverRpc(int orbId)
    {
        _orbPool.ReturnToPool(_spawnedOrbs[orbId]);
        _spawnedOrbs.Remove(orbId);
    }
}
