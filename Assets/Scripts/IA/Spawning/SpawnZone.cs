using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomConsole.Runtime.Console;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PathfindingGridReader))]
public class SpawnZone : NetworkBusListener
{
    [SerializeField] private PathfindingRequestManager _pathfindingRequestManager;
    [SerializeField] private int _budgetMin;
    [SerializeField] private int _budgetMax;
    [SerializeField] private int _currentBudget;

    private bool _zoneActivated;
    [SerializeField] private float spawnDelayFirstWave;
    [SerializeField] private List<MobSpawnSO> spawnMobsFirstWave = new List<MobSpawnSO>();
    
    
    [SerializeField] private float spawnDelay;
    [SerializeField] private List<MobSpawnSO> spawnMobs = new List<MobSpawnSO>();

    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
    
    private PathfindingGridReader _gridReader;
    private List<EnemyCore> _spawnedEnemies = new List<EnemyCore>();

    public Action<SpawnZone> p_onSpawnZoneComplete;

    public override void OnStartServer()
    {
        _gridReader = GetComponent<PathfindingGridReader>();
        
        ListenToEvent<EnemyDyingEvent>(EDE =>
        {
            if (EDE.p_gridReaderId == _gridReader.p_id)
            {
                _currentBudget -= EDE.p_enemySpawnCost;
                _spawnedEnemies.Remove(EDE.p_enemyCore);
                
                if (IsSpawnZoneComplete())
                {
                    p_onSpawnZoneComplete?.Invoke(this);
                }
            }
            
        });

        ListenToEvent<PlayerPositionUpdateEvent>(PPUE =>
        {
            for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
            {
                if(!_spawnedEnemies[i]) _spawnedEnemies.RemoveAt(i);
                else _spawnedEnemies[i].OnPlayerMoving(PPUE.p_networkObjectId, PPUE.p_playerPosition);
            }
        });
    }

    [ServerRpc(RequireOwnership = false)]
    [CallableFunction("Trigger Zone")]
    public void TriggerZone()
    {
        if(_zoneActivated) return;
        
        _zoneActivated = true;
        StartSpawning();
    }

    [Server]
    async void StartSpawning()
    {
        await SpawnFirstWave();
        await SpawnSecondWave();
    }
    
    [Server]
    public void SpawnEnemy(GameObject enemyPrefab, int enemyCost)
    {
        Vector3 position = GetValidSpawnPoint().position;
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        
        
        EnemyCore enemyCore =  enemy.GetComponent<EnemyCore>();
        enemyCore.SetInfos(_gridReader.p_id, _pathfindingRequestManager, _gridReader, enemyCost);
        
        _spawnedEnemies.Add(enemyCore);
        
        InstanceFinder.ServerManager.Spawn(enemy);
    }
    Transform GetValidSpawnPoint() => _spawnPoints[Random.Range(0, _spawnPoints.Count)];

    [Server]
    async Task SpawnFirstWave()
    {
        while (spawnMobsFirstWave.Count > 0)
        {
            MobSpawnSO mobSpawnSo = spawnMobsFirstWave[0];
            spawnMobsFirstWave.RemoveAt(0);
            SpawnEnemy(mobSpawnSo.p_prefab, mobSpawnSo.p_cost);
            _currentBudget += mobSpawnSo.p_cost;
            
            await Task.Delay((int)(spawnDelayFirstWave * 1000));
        }
    }

    [Server]
    async Task SpawnSecondWave()
    {
        int remainingTime = (int)(spawnDelay * 1000);
        
        while (spawnMobs.Count > 0 && _zoneActivated)
        {
            await Task.Delay(50);
            remainingTime -= 50;
            remainingTime = Math.Max(0, remainingTime);
            
            //Hover budget => do nothing
            if (_currentBudget > _budgetMax)
            {
                continue;
            }

            //In budget => waiting for countDown to reach 0 and budget to be enough before spawning
            if (_currentBudget > _budgetMin && remainingTime <= 0 && _currentBudget + spawnMobs[0].p_cost < _budgetMax)
            {
                MobSpawnSO mobSpawnSo = spawnMobs[0];
                spawnMobs.RemoveAt(0);
                SpawnEnemy(mobSpawnSo.p_prefab, mobSpawnSo.p_cost);
                _currentBudget += mobSpawnSo.p_cost;
                
                remainingTime = (int)(spawnDelay * 1000);
                continue;
            }

            //Under budget => spawning immediately
            if (_currentBudget < _budgetMin)
            {
                MobSpawnSO mobSpawnSo = spawnMobs[0];
                spawnMobs.RemoveAt(0);
                SpawnEnemy(mobSpawnSo.p_prefab, mobSpawnSo.p_cost);
                _currentBudget += mobSpawnSo.p_cost;
                
                remainingTime = (int)(spawnDelay * 1000);
            }
        }
    }

    bool IsSpawnZoneComplete()
    {
        return _spawnedEnemies.Count == 0 && spawnMobsFirstWave.Count == 0 && spawnMobs.Count == 0;
    }
    
}
