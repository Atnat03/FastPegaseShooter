using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[AddComponentMenu("EnemyBehaviour/Core")]
public class EnemyCore : NetworkBusListener
{
    [SerializeField] private float _maxEnemySwelling;
    private float _swellingAmount;

    [SerializeField] private List<EnemyAttackModule> _attackingModules = new List<EnemyAttackModule>();
    [SerializeField] private List<EnemyLifeModule> _lifeModules = new List<EnemyLifeModule>();
    [SerializeField] private List<EnemyTargetModule> _targetingModules = new List<EnemyTargetModule>();
    [SerializeField] private EnemyMovementModule _movementModule;
    
    //Filled In Automatially
    //private List<ScoreTargetModule> _scoreModules = new List<ScoreTargetModule>();

    public Guid p_gridReaderId;
    public PathfindingRequestManager p_pathRequester;
    public PathfindingGridReader p_gridReader;
    [HideInInspector] public int p_enemySpawnCost;

    #region Charges Variables
    
    [SerializeField] private int _explosionChargedDamage = 50;
    
    public float p_negativeChargeMax = 5;
    public float p_currentNegativeCharge;
    
    public float p_positiveChargeMax = 5;
    public float p_currentPositiveCharge;
    #endregion

    #region Actions

    public Action p_OnChargeExplosion;
    public Action p_OnPositiveChargeChange;
    public Action p_OnNegativeChargeChange;

    #endregion

    
    public override void OnStartServer()
    {
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
                Debug.LogWarning($"Null EnemyAttackModule found in {gameObject.name}");
        }
    
        foreach (EnemyLifeModule module in _lifeModules)
        {
            if (module != null)
                module.InitialiseBehaviourModule(this);
            else
                Debug.LogWarning($"Null EnemyLifeModule found in {gameObject.name}");
        }
    
        foreach (EnemyTargetModule module in _targetingModules)
        {
            if (module != null)
                module.InitialiseBehaviourModule(this);
            else
                Debug.LogWarning($"Null EnemyTargetModule found in {gameObject.name}");
        }

        _movementModule?.InitialiseBehaviourModule(this);
    }

    public void SetInfos(Guid _readerId, PathfindingRequestManager pathfindingRequestManager,
        PathfindingGridReader pathfindingGridReader, int cost)
    {
        p_gridReaderId = _readerId;
        p_enemySpawnCost = cost;
        p_pathRequester = pathfindingRequestManager;
        p_gridReader = pathfindingGridReader;
    }

    private void OnNetworkTick()
    {
        foreach (EnemyAttackModule module in _attackingModules)
        {
            if (module != null)
                module.OnNetworkTick();
        }

        foreach (EnemyTargetModule module in _targetingModules)
        {
            if (module != null)
                module.OnNetworkTick();
        }

        _movementModule?.OnNetworkTick();
    }

    public void OnPlayerMoving(int playerObjectId, Vector3 playerPosition)
    {
        _movementModule?.OnPlayerMoving(playerObjectId, playerPosition);
    }

    #region Charges

    [Server]
    public void AddCharge(bool positive, float value)
    {
        if (positive)
        {
            p_currentPositiveCharge += value;
            OnPositiveChangeObserverRpc();
        }
        else
        {
            p_currentNegativeCharge += value;
            OnNegativeChangeObserverRpc();
        }

        CheckAllChargeAreFull();
    }

    [Server]
    private void ResetAllCharged()
    {
        p_currentPositiveCharge = 0;
        p_currentNegativeCharge = 0;
    }
    
    [Server]
    private void CheckAllChargeAreFull()
    {
        if (p_currentPositiveCharge >= p_positiveChargeMax && p_currentNegativeCharge >= p_negativeChargeMax)
        {
            //life module at position 0 is considered to be the main life module
            // => There is feedback in the inspector
            _lifeModules[0].TakeDamage(Owner.ClientId, _explosionChargedDamage);
            
            
            ResetAllCharged();
            ExplosionObserversRpc();
        }
    }

    [ObserversRpc]
    private void ExplosionObserversRpc()
    {
        p_OnChargeExplosion?.Invoke();
    }

    [ObserversRpc]
    private void OnPositiveChangeObserverRpc()
    {
        p_OnPositiveChargeChange?.Invoke();
    }
    [ObserversRpc]
    private void OnNegativeChangeObserverRpc()
    {
        p_OnNegativeChargeChange?.Invoke();
    }
    
    #endregion
}