using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCore : NetworkBusListener
{
    [SerializeField] private float _maxEnemySwelling;
    private float _swellingAmount;
    
    [SerializeField] private List<EnemyAttackingModule> _attackingModules = new List<EnemyAttackingModule>();
    [SerializeField] private List<EnemyLifeModule> _lifeModules = new List<EnemyLifeModule>();
    [SerializeField] private EnemyMovingModule _movingModules;
    
    public Guid p_gridReaderId;
    [HideInInspector]public  int p_enemySpawnCost;

    public override void OnStartServer()
    {
        base.OnStartServer();
        InitialiseEnemy();
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

    public void OnPlayerMoving(int playerObjectId, Vector3 playerPosition, PathfindingGridReader gridReader)
    {
        _movingModules.OnPlayerMoving(playerObjectId, playerPosition, gridReader);
    }
}
