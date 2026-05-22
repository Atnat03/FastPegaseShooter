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
public class SubArena : NetworkBusListener
{
    [SerializeField] private PathfindingRequestManager _pathfindingRequestManager;
    
    [SerializeField] private int _currentBudget;

    private bool _zoneActivated;
    [SerializeField] private float spawnDelayFirstWave;
    [SerializeField] private List<MobSpawnSO> spawnMobsFirstWave = new List<MobSpawnSO>();

    [SerializeField] private List<MobSpawnProba> spawnMobs = new List<MobSpawnProba>();

    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
    
    private PathfindingGridReader _gridReader;
    private List<EnemyCore> _spawnedEnemies = new List<EnemyCore>();


    #region Initialisation

    public override void OnStartServer()
    {
        _gridReader = GetComponent<PathfindingGridReader>();
        

        CustomLogger.ImportantLog("Not sure if this listening is usefull => to test");
        ListenToEvent<PlayerPositionUpdateEvent>(PPUE =>
        {
            if (PPUE.p_isHeartBeat) return; //Heart beat isn't usefull for pathfinding
            
            for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
            {
                if(!_spawnedEnemies[i]) _spawnedEnemies.RemoveAt(i);
                else _spawnedEnemies[i].OnPlayerMoving(PPUE.p_networkObjectId, PPUE.p_playerPosition);
            }
        });
    }

    #endregion

    #region SubArena Starting

    [ServerRpc(RequireOwnership = false)]
    public void TriggerSubArenaSpawning()
    {
        if(_zoneActivated) return;
        
        _zoneActivated = true;
        StartSpawning();
    }

    [Server]
    async void StartSpawning()
    {
        await SpawnFirstWave();
        await InfiniteSpawn();
    }

    #endregion

    #region Utilities

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

    #endregion

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
    async Task InfiniteSpawn()
    {
        
    }
}

[System.Serializable]
public struct MobSpawnProba
{
    public MobSpawnSO p_mobSpawnSo;
    public int p_spawnProba;
}
