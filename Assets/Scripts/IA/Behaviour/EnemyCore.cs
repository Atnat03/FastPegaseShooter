using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using ScriptableObjectsDefinitions;
using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Core")]
public class EnemyCore : NetworkBusListener
{

    [SerializeField] private List<EnemyAttackModule> _attackingModules = new List<EnemyAttackModule>();
    [SerializeField] private List<EnemyLifeModule> _lifeModules = new List<EnemyLifeModule>();
    [SerializeField] private List<EnemyTargetModule> _targetingModules = new List<EnemyTargetModule>();
    [SerializeField] private EnemyMovementModule _movementModule;
    [SerializeField] private Collider _enemyCollider;
    
    public Guid p_gridReaderId;
    public PathfindingRequestManager p_pathRequester;
    public PathfindingGridReader p_gridReader;

    public Action OnDapExplosion;

    public EnemyCoreSO p_coreSo;

    public Action OnSpawn;
    public Action OnDeath;
    public bool p_isSpawning = false;
    public bool p_isDying = false;
    
    private float _spawnDeathTimer;

    
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        InitialiseEnemy();
        InstanceFinder.TimeManager.OnTick += OnNetworkTick;
        
        //Initialising Score Target Module
        List<ScoreTargetModule> scoreModules = new List<ScoreTargetModule>();
        foreach (EnemyTargetModule targetModule in _targetingModules)
        {
            if (targetModule != null && targetModule is ScoreTargetModule scoreTargetModule)
                scoreModules.Add(scoreTargetModule);
        }

        foreach (EnemyAttackModule module in _attackingModules)
        {
            if (module == null) continue;
            foreach (ScoreTargetModule scoreModule in scoreModules)
            {
                if (scoreModule != null)
                    module.p_onHitPlayer += scoreModule.OnHitPlayer;
            }
        }
        
        foreach (EnemyLifeModule module in _lifeModules)
        {
            if (module == null) continue;
            foreach (ScoreTargetModule scoreModule in scoreModules)
            {
                if (scoreModule != null)
                    module.p_onHitPlayer += scoreModule.OnDamageTaken;
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        OnSpawn?.Invoke();
    }

    public override void OnStopServer()
    {
        InstanceFinder.TimeManager.OnTick -= OnNetworkTick;
    }

    public void InitialiseEnemy()
    {
        foreach (EnemyAttackModule module in _attackingModules)
        {
            if (module != null)
                module.InitialiseBehaviourModule(this);
            else
                Debug.LogError($"Null EnemyAttackModule found in {gameObject.name}");
        }
    
        foreach (EnemyLifeModule module in _lifeModules)
        {
            if (module != null)
                module.InitialiseBehaviourModule(this);
            else
                Debug.LogError($"Null EnemyLifeModule found in {gameObject.name}");
        }
    
        foreach (EnemyTargetModule module in _targetingModules)
        {
            if (module != null)
                module.InitialiseBehaviourModule(this);
            else
                Debug.LogError($"Null EnemyTargetModule found in {gameObject.name}");
        }

        _movementModule?.InitialiseBehaviourModule(this);
    }

    public void SetInfos(Guid _readerId, PathfindingRequestManager pathfindingRequestManager,
        PathfindingGridReader pathfindingGridReader)
    {
        p_gridReaderId = _readerId;
        p_pathRequester = pathfindingRequestManager;
        p_gridReader = pathfindingGridReader;
        
        p_isSpawning = true;
        p_isDying = false;
    }

    private void OnNetworkTick()
    {
        float tickDelta = (float)InstanceFinder.TimeManager.TickDelta;
        //Spawning
        if (p_isSpawning)
        {
            _spawnDeathTimer += tickDelta;
            if (_spawnDeathTimer >= p_coreSo.p_spawningTime)
            {
                _spawnDeathTimer = 0;
                p_isSpawning = false;
            }
            return;
        }
        //Death
        if (p_isDying)
        {
            _spawnDeathTimer += tickDelta;
            if (_spawnDeathTimer >= p_coreSo.p_deathTime)
            {
                _spawnDeathTimer = 0;
                p_isDying = false;
                DespawnEnemy();
            }
            return;
        }
        
        foreach (EnemyAttackModule module in _attackingModules)
        {
            if (module != null)
                module.OnNetworkTick(tickDelta);
        }

        foreach (EnemyTargetModule module in _targetingModules)
        {
            if (module != null)
                module.OnNetworkTick(tickDelta);
        }

        _movementModule?.OnNetworkTick(tickDelta);
    }

    public void ExplodeOnDapWave()
    {
        ExplodeOnDapWaveObserverRpc();
        KillEnemy(-1, ChargeType.None);
    }

    void ExplodeOnDapWaveObserverRpc()
    {
        OnDapExplosion?.Invoke();
    }

    [Server]
    public void KillEnemy(int playerObjectId, ChargeType charge)
    {
        if(p_isDying) return;
        p_isDying = true;
        
        //if killed by dap wave
        if (charge == ChargeType.None)
        {
            InstanceFinder.ServerManager.Despawn(transform.root.gameObject);
            return;
        }
        
        float signedEnergyAmount = GetSignedEnergyAmount(charge);
        EventBus.InvokeEvent(new OnEnemyDieEvent(this, !p_coreSo.p_dropXpOrb ? 0 : signedEnergyAmount));

        NetworkConnection killerConn = InstanceFinder.ServerManager.Clients[playerObjectId];
        if (killerConn != null)
            PlayerDoKillTargetRpc(killerConn, playerObjectId);     
        
        if (!p_coreSo.p_dropXpOrb)
        {
            AddEnergyWhenEnemyKillObserversRpc(playerObjectId, signedEnergyAmount);
        }
        
        OnEnemyDeathObserverRpc();
    }

    [Server]
    public void DespawnEnemy()
    {
        InstanceFinder.ServerManager.Despawn(transform.root.gameObject);
    }
    
    [ObserversRpc]
    void OnEnemyDeathObserverRpc()
    {
        if(_enemyCollider) _enemyCollider.enabled = false;
        OnDeath?.Invoke();
    }
    
    [ObserversRpc]
    private void AddEnergyWhenEnemyKillObserversRpc(int id, float value)
    {
        InvokeEvent(new ModifyEnergyEvent
        {
            p_player = id,
            p_value = Mathf.Abs(value)
        });
    }

    [TargetRpc]
    private void PlayerDoKillTargetRpc(NetworkConnection conn, int index)
    {
        InvokeEvent(new OnPlayerDoKill { p_owerId = index });
    }
    
    float GetSignedEnergyAmount(ChargeType charge)
    {
        switch (charge)
        {
            case ChargeType.Positive:
                if (p_coreSo.p_pinataType == ChargeType.Positive)
                    return p_coreSo.p_pinataEnergyDropValue;
                return p_coreSo.p_baseEnergyDropValue;
            
            case ChargeType.Negative:
                if (p_coreSo.p_pinataType == ChargeType.Negative)
                    return -p_coreSo.p_pinataEnergyDropValue;
                return -p_coreSo.p_baseEnergyDropValue;
            
            default:
                return 0f;
        }
    }
}

public enum ChargeType{Negative, Positive, None}