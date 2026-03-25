using System;
using System.Collections.Generic;
using FishNet;
using UnityEngine;

public class EnemyCore : NetworkBusListener
{
    [SerializeField] private float _maxEnemySwelling;
    private float _swellingAmount;
    
    [SerializeField] private List<EnemyAttackingModule> _attackingModules = new List<EnemyAttackingModule>();
    [SerializeField] private List<EnemyLifeModule> _lifeModules = new List<EnemyLifeModule>();
    [SerializeField] private EnemyMovingModule _movingModule;
    
    //Filled In Automatially
    private List<ScoreTargetingModule> _scoreModules = new List<ScoreTargetingModule>();

    public Guid p_gridReaderId;
    [HideInInspector]public  int p_enemySpawnCost;
    public override void OnStartServer()
    {
        base.OnStartServer();
        InitialiseEnemy();
        InstanceFinder.TimeManager.OnTick += OnNetworkTick;
        
        //detect all scoreTargetingModule on gameObject
        GetComponents<ScoreTargetingModule>(_scoreModules);
        
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
    }
    
    public void SetInfos(Guid _readerId, int cost)
    {
        p_gridReaderId = _readerId;
        p_enemySpawnCost = cost;
    }
    
    private void OnNetworkTick()
    {
        foreach(EnemyAttackingModule module in _attackingModules)
            module.OnNetworkTick();
        
        _movingModule.OnNetworkTick();
    }

    public void OnPlayerMoving(int playerObjectId, Vector3 playerPosition, PathfindingGridReader gridReader)
    {
        _movingModule.OnPlayerMoving(playerObjectId, playerPosition, gridReader);
    }
}
