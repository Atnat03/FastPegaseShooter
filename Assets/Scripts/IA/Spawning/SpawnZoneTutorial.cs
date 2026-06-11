using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using MyPrint;
using Tuto;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PathfindingGridReader))]
public class SpawnZoneTutorial : NetworkBusListener
{
    public bool IsComplete => _spawnedEnemySet.Count == 0 && _spawnWave.Count == 0 && _zoneActivated;
    public int ZoneIndex => _zoneIndex;
    
    private PathfindingRequestManager _pathfindingRequestManager;

    private bool _zoneActivated;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private List<MobSpawnSO> _spawnWave = new List<MobSpawnSO>();
    
    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
    [SerializeField] private int _zoneIndex;
    
    private PathfindingGridReader _gridReader;
    private int _spawnedEnemies;
    private HashSet<EnemyCore> _spawnedEnemySet = new();
    
    public Action<SpawnZoneTutorial> p_onSpawnZoneComplete;
    
    [Header("Infinite wave")]
    [SerializeField] private bool _infiniteWave;
    [SerializeField] private float _infiniteSpawnInterval = 5f;
    [SerializeField] private MobSpawnSO _infiniteEnemyData;

    private bool _stopInfiniteSpawn;

    [Header("Corrosion")]
    [SerializeField] private Image _corrosionImage;
    [SerializeField] private int _maxMobsInArena = 10;
    [SerializeField] private int _corrosionDelay_Miliss = 500;
    [SerializeField] private int _corrosionDamage = 5;
    
    private int  _currentMobsInArena;
    private bool _spawnPaused;
    private bool _corrosionActivated;
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        _gridReader = GetComponent<PathfindingGridReader>();
        
        ListenToEvent<OnEnemyDieEvent>(OEDE =>
        {
            if (OEDE.p_enemy.p_gridReaderId != _gridReader.p_id) return;

            if (!_spawnedEnemySet.Remove(OEDE.p_enemy)) return;

            _spawnedEnemies--;
            _currentMobsInArena--;

            UpdateGaugeObservers(_currentMobsInArena, _maxMobsInArena);

            if (_spawnPaused && _currentMobsInArena < _maxMobsInArena)
                _spawnPaused = false;
            
            if (IsSpawnZoneComplete())
            {
                Debug.Log($"[Zone {_zoneIndex}] Invoking p_onSpawnZoneComplete, listeners: {p_onSpawnZoneComplete?.GetInvocationList()?.Length ?? 0}");
                p_onSpawnZoneComplete?.Invoke(this);
            }
        });
        
        ListenToEvent<OnStartSpawner_TUTO>(StartSpawning);
        ListenToEvent<OnDapEvent>(StopInfiniteWave);
        
        InvokeEvent(new GetPathfindingRequestManagerRequest
        {
            p_OnGetPathfindingRequestManager = (PRM) =>
            {
                _pathfindingRequestManager = PRM;
            }
        });
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartSpawning(OnStartSpawner_TUTO data)
    {
        if (data.spawnIndex != _zoneIndex) return;
        if (_zoneActivated) return;
        
        _zoneActivated = true;
        Spawn();
    }

    async void Spawn()
    {
        await SpawnWave();

        if (_infiniteWave)
        {
            _ = SpawnInfiniteEnemies();
        }
    }
    
    [Server]
    public void SpawnEnemy(GameObject enemyPrefab)
    {
        if (_currentMobsInArena >= _maxMobsInArena)
        {
            if (!_spawnPaused)
            {
                _spawnPaused = true;
                ActivateCorrosion();
            }
            return;
        }

        Vector3 position = GetValidSpawnPoint().position;
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        
        EnemyCore enemyCore = enemy.GetComponentInChildren<EnemyCore>();
        enemyCore.SetInfos(_gridReader.p_id, _pathfindingRequestManager, _gridReader);
        
        InstanceFinder.ServerManager.Spawn(enemy);
        InvokeEvent(new OnEnemySpawnEvent());
        
        _spawnedEnemySet.Add(enemyCore);
        _spawnedEnemies++;
        _currentMobsInArena++;

        UpdateGaugeObservers(_currentMobsInArena, _maxMobsInArena);
        
    }

    [Server]
    void ActivateCorrosion()
    {
        if (_corrosionActivated) return;
        _corrosionActivated = true;

        _ = ApplyCorrosion();
    }

    async Task ApplyCorrosion()
    {
        while (_corrosionActivated)
        {
            await Task.Delay(_corrosionDelay_Miliss);

            EventBus.InvokeEvent(new OnCorrosionEvent(_corrosionDamage));
        }
    }

    [Server]
    async Task SpawnInfiniteEnemies()
    {
        try
        {
            while (!_stopInfiniteSpawn)
            {
                await Task.Delay((int)(_infiniteSpawnInterval * 1000));

                if (_stopInfiniteSpawn) break;

                while (_spawnPaused && !_stopInfiniteSpawn)
                {
                    await Task.Delay(500);
                }

                if (!_stopInfiniteSpawn)
                    SpawnEnemy(_infiniteEnemyData.p_prefab);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    [Server]
    async Task SpawnWave()
    {
        try
        {
            while (_spawnWave.Count > 0)
            {
                while (_spawnPaused && _spawnWave.Count > 0)
                {
                    await Task.Delay(500);
                }

                if (_spawnWave.Count == 0) break;

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

    [ObserversRpc]
    void UpdateGaugeObservers(int current, int max, bool isActivated = true)
    {
        if (_corrosionImage == null) return;
        
        InvokeEvent(new OnFillAmount_TUTO
        {
            activated = isActivated,
            speed = 10,
            maxPercentage = (max > 0 ? (float)current / max : 0f) * 100,
            type = AnimationBar.None
        });
    }
    
    [Server]
    public void StopInfiniteWave(OnDapEvent data)
    {
        _stopInfiniteSpawn = true;
        UpdateGaugeObservers(0, 0, false);
    }

    Transform GetValidSpawnPoint() => _spawnPoints[Random.Range(0, _spawnPoints.Count)];

    bool IsSpawnZoneComplete()
    {
        bool clear = _spawnedEnemySet.Count == 0 && _spawnWave.Count == 0;

        Debug.Log($"[Zone {_zoneIndex}] IsComplete check — enemies: {_spawnedEnemySet.Count}, wave remaining: {_spawnWave.Count}, result: {clear}");
        
        return clear;
    }
}