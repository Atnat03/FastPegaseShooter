using System;
using System.Collections.Generic;
using FishNet;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyCore : NetworkBusListener
{
    [SerializeField] private float _maxEnemySwelling;
    private float _swellingAmount;
    
    [SerializeField] private List<EnemyAttackingModule> _attackingModules = new List<EnemyAttackingModule>();
    [SerializeField] private List<EnemyLifeModule> _lifeModules = new List<EnemyLifeModule>();
    [SerializeField] private List<EnemyTargetingModule> _targetingModules = new List<EnemyTargetingModule>();
    [SerializeField] private EnemyMovingModule _movingModule;
    
    //Filled In Automatially
    private List<ScoreTargetingModule> _scoreModules = new List<ScoreTargetingModule>();

    public Guid p_gridReaderId;
    public PathfindingRequestManager p_pathRequester;
    public PathfindingGridReader p_gridReader;
    [HideInInspector]public  int p_enemySpawnCost;
    public override void OnStartServer()
    {
        base.OnStartServer();
        InitialiseEnemy();
        InstanceFinder.TimeManager.OnTick += OnNetworkTick;
        
        foreach (EnemyTargetingModule targetModule in _targetingModules)
        {
            if(targetModule is ScoreTargetingModule scoreTargetModule)
                _scoreModules.Add(scoreTargetModule);
        }
        
        foreach (EnemyAttackingModule module in _attackingModules)
            foreach (ScoreTargetingModule scoreModule in _scoreModules)
                module.p_onHitPlayer += scoreModule.OnHitPlayer;
        
        foreach (EnemyLifeModule module in _lifeModules)
            foreach (ScoreTargetingModule scoreModule in _scoreModules)
                module.p_onHitPlayer += scoreModule.OnDamageTaken;
    }

    public override void OnStopServer()
    {
        InstanceFinder.TimeManager.OnTick -= OnNetworkTick;
        base.OnStopServer();
    }
    public void InitialiseEnemy()
    {
        foreach (EnemyAttackingModule module in _attackingModules)
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
        foreach(EnemyAttackingModule module in _attackingModules)
            module.OnNetworkTick();

        foreach (EnemyTargetingModule module in _targetingModules)
            module.OnNetworkTick();
        
        _movingModule.OnNetworkTick();
    }

    public void OnPlayerMoving(int playerObjectId, Vector3 playerPosition)
    {
        _movingModule.OnPlayerMoving(playerObjectId, playerPosition);
    }
}
