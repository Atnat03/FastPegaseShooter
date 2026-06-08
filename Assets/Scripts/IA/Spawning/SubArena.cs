using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using GameKit.Dependencies.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PathfindingGridReader))]
public class SubArena : NetworkBusListener
{
    [Header("----- General -----")]
    [SerializeField] private bool _zoneActivated;
    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
    [SerializeField] private SubArenaGauge _arenaGaugePrefab;
    
    [Header("----- First Wave -----")]
    [SerializeField] private float spawnDelayFirstWave;
    [SerializeField] private List<MobSpawnSO> spawnMobsFirstWave = new List<MobSpawnSO>();

    [Header("----- Infinite Spawn -----")]
    [SerializeField] private int _currentBudget;
    [SerializeField] private List<MobSpawnSO> _spawnMobs = new();
    [SerializeField] private List<SpawningState> _spawningStates = new();
    
    [Header("----- Corrosion -----")]
    [SerializeField] private int _maxSpawnEnemy = 10;
    [SerializeField] private int _corrosionDamage = 5;
    [SerializeField] private int _corrosionDelay = 3;

    private float _enemyTotalWeight;
    private PathfindingRequestManager _pathfindingRequestManager;
    private int _currentSpawnPointIndex = 0;
    
    [Header("----- Debug -----")]
    [SerializeField] private int _maxEnabledTime;

    [SerializeField]private int _currentStateIndex;
    private PathfindingGridReader _gridReader;
    private List<EnemyCore> _spawnedEnemies = new List<EnemyCore>();

    #region Initialisation

    public override void OnStartServer()
    {
        _gridReader = GetComponent<PathfindingGridReader>();
        
        ListenToEvent<OnDapEvent>(ODE =>
        {
            _zoneActivated = false;
        });
        
        ListenToEvent<OnEnemyDieEvent>(OEDE =>
        {
            if(_spawnedEnemies.Contains(OEDE.p_enemy))
            {
                NotifySubArenaUpdateObserverRpc(_gridReader.p_id);
                _spawnedEnemies.Remove(OEDE.p_enemy);
            }
        });
        
        ListenToEvent<ForceStopEnemySpawn>(FSES =>
        {
            _zoneActivated = false;
        });
        
        InvokeEvent(new GetPathfindingRequestManagerRequest
        {
            p_OnGetPathfindingRequestManager = (PRM) =>
            {
                _pathfindingRequestManager = PRM;
            }
        });

        //to trigger a spawn point shuffle
        _currentSpawnPointIndex = _spawnPoints.Count;
        
        InitialiseSpawnProbability();
    }

    void InitialiseSpawnProbability()
    {
        _spawnMobs = _spawnMobs.OrderByDescending(s => s.p_spawnProba).ToList();
        _enemyTotalWeight = 0;
        
        foreach (MobSpawnSO spawnMob in _spawnMobs)
            _enemyTotalWeight += spawnMob.p_spawnProba;
    }

    #endregion

    #region Visual Notification

    [ObserversRpc]
    void NotifySubArenaStartObserverRpc(Guid arenaId)
    {
        EventBus.InvokeEvent(
            new OnSubArenaStartEvent(
                arenaId,
                _arenaGaugePrefab)
            );
    }
    [ObserversRpc]
    void NotifySubArenaUpdateObserverRpc(Guid arenaId)
    {
        EventBus.InvokeEvent(
            new OnSubArenaUpdateEvent(
                arenaId, 
                _spawnedEnemies.Count/(float)_maxSpawnEnemy,
                _spawningStates[_currentStateIndex].p_state)
            );
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
        if(!IsServerStarted) return;
        NotifySubArenaStartObserverRpc(_gridReader.p_id);
        await SpawnFirstWave();
        await InfiniteSpawn();
    }

    #endregion

    #region Utilities

    [Server]
    MobSpawnSO GetNextEnemyToSpawn()
    {
        float random01 = Random.Range(0f, 1f);
        float currentWeight = 0;
        foreach (var mobSpawn in _spawnMobs)
        {
            currentWeight += mobSpawn.p_spawnProba/_enemyTotalWeight;
            if(random01 <= currentWeight)
            {
                //CustomLogger.HighlightLog($"Chosen Enemy : {mobSpawn.mob.name}, random01 : {random01}");
                return mobSpawn;
            }
        }
        
        //fallback for float imprecision
        return _spawnMobs[^1];
    }

    [Server]
    public void SpawnEnemy(GameObject enemyPrefab)
    {
        Vector3 position = GetNextSpawnPoint().position;
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        
        
        EnemyCore enemyCore =  enemy.GetComponentInChildren<EnemyCore>();
        enemyCore.SetInfos(_gridReader.p_id, _pathfindingRequestManager, _gridReader);
        
        _spawnedEnemies.Add(enemyCore);
        
        InstanceFinder.ServerManager.Spawn(enemy);
    }
    Transform GetNextSpawnPoint()
    { 
        if (_currentSpawnPointIndex >=  _spawnPoints.Count)
        {
            _spawnPoints.Shuffle();
            _currentSpawnPointIndex = 0;
        }

        return _spawnPoints[_currentSpawnPointIndex++];
    }

    #endregion

    #region Wave Spawning
    [Server]
    async Task SpawnFirstWave()
    {
        try
        {
            while (spawnMobsFirstWave.Count > 0 && _zoneActivated && Application.isPlaying)
            {
                MobSpawnSO mobSpawnSo = spawnMobsFirstWave[0];
                spawnMobsFirstWave.RemoveAt(0);
                SpawnEnemy(mobSpawnSo.p_prefab);
            
                await Task.Delay((int)(spawnDelayFirstWave * 1000));
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }
    }

    [Server]
    async Task InfiniteSpawn()
    {
        try
        {
            int enabledTime = 0;
            int currentStateTime = 0;
            
            
            while (Application.isPlaying && _zoneActivated && enabledTime < _maxEnabledTime)
            {
                //Corrosion
                if (_spawnedEnemies.Count >= _maxSpawnEnemy)
                {
                    int t = 0;
                    while (_spawnedEnemies.Count >= _maxSpawnEnemy &&
                           enabledTime < _maxEnabledTime && _zoneActivated
                           && Application.isPlaying)
                    {
                        await Task.Delay(500);
                        t += 500;
                        if (t >= _corrosionDelay * 1000)
                        {
                            EventBus.InvokeEvent(new OnCorrosionEvent(_corrosionDamage));
                            t -= _corrosionDelay * 1000;
                        }
                    }
                }
                
                MobSpawnSO nextMobToSpawn = GetNextEnemyToSpawn();

                //generating budget
                while (Application.isPlaying &&
                       _currentBudget < nextMobToSpawn.p_cost &&
                       enabledTime < _maxEnabledTime && _zoneActivated)
                {
                    if (currentStateTime >= _spawningStates[_currentStateIndex].p_stateDuration)
                    {
                        currentStateTime = 0;
                        _currentStateIndex = (_currentStateIndex + 1) % _spawningStates.Count;
                        NotifySubArenaUpdateObserverRpc(_gridReader.p_id);
                    }
                    
                    _currentBudget += _spawningStates[_currentStateIndex].p_state.p_budgetPerSecond;
                    //CustomLogger.ImportantLog($"Budget : {_currentBudget}, state :  {_spawningStates[_currentStateIndex].p_state.p_name}");
                    enabledTime++;
                    currentStateTime++;
                    await Task.Delay(1000);
                }

                //Enemy Spawning
                if(Application.isPlaying && _zoneActivated)
                {
                    //CustomLogger.Log($"Spawn enemy : {nextMobToSpawn.name}");
                    _currentBudget -= nextMobToSpawn.p_cost;
                    SpawnEnemy(nextMobToSpawn.p_prefab);
                    NotifySubArenaUpdateObserverRpc(_gridReader.p_id);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }
    }
    #endregion
}

[System.Serializable]
public struct SpawningState
{
    [Tooltip("Duration is expressed in seconds")] public int p_stateDuration;
    public SubArenaStateSO p_state;
}
