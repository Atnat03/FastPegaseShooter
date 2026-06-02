using System;
using System.Collections.Generic;
using Controller;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[AddComponentMenu("EnemyBehaviour/Core")]
public class EnemyCore : NetworkBusListener
{
    [SerializeField] private List<EnemyAttackModule> _attackingModules = new List<EnemyAttackModule>();
    [SerializeField] private List<EnemyLifeModule> _lifeModules = new List<EnemyLifeModule>();
    [SerializeField] private List<EnemyTargetModule> _targetingModules = new List<EnemyTargetModule>();
    [SerializeField] private EnemyMovementModule _movementModule;
    
    public Guid p_gridReaderId;
    public PathfindingRequestManager p_pathRequester;
    public PathfindingGridReader p_gridReader;

    public Action OnDapExplosion;

    [SerializeField] private EnemyCoreSO _coreSo;

    
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
    }

    private void OnNetworkTick()
    {
        float tickDelta = (float)InstanceFinder.TimeManager.TickDelta;
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

    public void KillEnemy(int playerObjectId, ChargeType charge)
    {

        //if killed by dap wave
        if (charge != ChargeType.None)
        {
            InstanceFinder.ServerManager.Despawn(transform.root.gameObject);
            return;
        }
        
        float signedEnergyAmount = GetSignedEnergyAmount(charge);
        
        EventBus.InvokeEvent(new OnEnemyDieEvent(this, !_coreSo.p_dropXpOrb ? 0 : signedEnergyAmount));
        
        
        InvokeEvent(new OnPlayerDoKill{p_owerId = playerObjectId});
        
        if (!_coreSo.p_dropXpOrb)
        {
            InvokeEvent(new ModifyEnergyEvent
            {
                p_player = playerObjectId,
                p_value = Mathf.Abs(signedEnergyAmount)
            });
        }
        
        InstanceFinder.ServerManager.Despawn(gameObject);
    }

    float GetSignedEnergyAmount(ChargeType charge)
    {
        switch (charge)
        {
            case ChargeType.Positive:
                if (_coreSo.p_pinataType == ChargeType.Positive)
                    return _coreSo.p_pinataEnergyDropValue;
                return _coreSo.p_baseEnergyDropValue;
            
            case ChargeType.Negative:
                if (_coreSo.p_pinataType == ChargeType.Negative)
                    return -_coreSo.p_pinataEnergyDropValue;
                return -_coreSo.p_baseEnergyDropValue;
            
            default:
                return 0f;
        }
    }
}

public enum ChargeType{Negative, Positive, None}