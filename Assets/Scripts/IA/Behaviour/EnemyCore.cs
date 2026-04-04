using System;
using System.Collections.Generic;
using FishNet;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyCore : NetworkBusListener
{
    [SerializeField] private float _maxEnemySwelling;
    private float _swellingAmount;
    
    [SerializeField] private List<EnemyAttackModule> _attackingModules = new List<EnemyAttackModule>();
    [SerializeField] private List<EnemyLifeModule> _lifeModules = new List<EnemyLifeModule>();
    [SerializeField] private List<EnemyTargetModule> _targetingModules = new List<EnemyTargetModule>();
    [SerializeField] private EnemyMovingModule _movingModule;
    
    //Filled In Automatially
    private List<ScoreTargetModule> _scoreModules = new List<ScoreTargetModule>();

    public Guid p_gridReaderId;
    public PathfindingRequestManager p_pathRequester;
    public PathfindingGridReader p_gridReader;
    [HideInInspector]public  int p_enemySpawnCost;
    public override void OnStartServer()
    {
        base.OnStartServer();
        InitialiseEnemy();
        InstanceFinder.TimeManager.OnTick += OnNetworkTick;
        
        foreach (EnemyTargetModule targetModule in _targetingModules)
        {
            if(targetModule is ScoreTargetModule scoreTargetModule)
                _scoreModules.Add(scoreTargetModule);
        }
        
        foreach (EnemyAttackModule module in _attackingModules)
            foreach (ScoreTargetModule scoreModule in _scoreModules)
                module.p_onHitPlayer += scoreModule.OnHitPlayer;
        
        foreach (EnemyLifeModule module in _lifeModules)
            foreach (ScoreTargetModule scoreModule in _scoreModules)
                module.p_onHitPlayer += scoreModule.OnDamageTaken;
    }

    public override void OnStopServer()
    {
        InstanceFinder.TimeManager.OnTick -= OnNetworkTick;
        base.OnStopServer();
    }
    public void InitialiseEnemy()
    {
        foreach (EnemyAttackModule module in _attackingModules)
            module.InitialiseBehaviourModule(this);
        foreach (EnemyLifeModule module in _lifeModules)
            module.InitialiseBehaviourModule(this);
        
        _movingModule.InitialiseBehaviourModule(this);
    }
    
    public void SetInfos(Guid _readerId, PathfindingRequestManager pathfindingRequestManager, PathfindingGridReader pathfindingGridReader,  int cost)
    {
        p_gridReaderId = _readerId;
        p_enemySpawnCost = cost;
        p_pathRequester = pathfindingRequestManager;
        p_gridReader = pathfindingGridReader;
    }
    
    private void OnNetworkTick()
    {
        foreach(EnemyAttackModule module in _attackingModules)
            module.OnNetworkTick();

        foreach (EnemyTargetModule module in _targetingModules)
            module.OnNetworkTick();
        
        _movingModule.OnNetworkTick();
    }

    public void OnPlayerMoving(int playerObjectId, Vector3 playerPosition)
    {
        _movingModule.OnPlayerMoving(playerObjectId, playerPosition);
    }
}
