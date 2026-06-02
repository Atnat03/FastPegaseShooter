using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PathfindingGridReader))]
public class SpawnZoneTutorial : NetworkBusListener
{
    [SerializeField] private PathfindingRequestManager _pathfindingRequestManager;

    private bool _zoneActivated;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private List<MobSpawnSO> _spawnWave = new List<MobSpawnSO>();
    
    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
    
    private PathfindingGridReader _gridReader;
    private int _spawnedEnemies;

    public Action<SpawnZoneTutorial> p_onSpawnZoneComplete;

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        
        _gridReader = GetComponent<PathfindingGridReader>();
        
        ListenToEvent<OnEnemyDieEvent>(OEDE =>
        {
            if(OEDE.p_enemy.p_gridReaderId == _gridReader.p_id)
            {
                _spawnedEnemies--;
                if(IsSpawnZoneComplete())
                {
                    p_onSpawnZoneComplete?.Invoke(this);
                }
            }
        });
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartSpawning()
    {
        if(_zoneActivated) return;
        
        _zoneActivated = true;
        Spawn();
    }

    async void Spawn()
    {
        await SpawnWave();
    }
    
    [Server]
    public void SpawnEnemy(GameObject enemyPrefab)
    {
        Vector3 position = GetValidSpawnPoint().position;
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        
        
        EnemyCore enemyCore =  enemy.GetComponentInChildren<EnemyCore>();
        enemyCore.SetInfos(_gridReader.p_id, _pathfindingRequestManager, _gridReader);
        
        _spawnedEnemies++;
        
        InstanceFinder.ServerManager.Spawn(enemy);
    }

    [Server]
    async Task SpawnWave()
    {
        try
        {
            while (_spawnWave.Count > 0)
            {
                MobSpawnSO mobSpawnSo = _spawnWave[0];
                _spawnWave.RemoveAt(0);
                SpawnEnemy(mobSpawnSo.p_prefab);

                await Task.Delay((int)(_spawnDelay * 1000));
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    Transform GetValidSpawnPoint() => _spawnPoints[Random.Range(0, _spawnPoints.Count)];
    bool IsSpawnZoneComplete() => _spawnedEnemies <= 0 && _spawnWave.Count <= 0;
    
}
