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

    #region Energy Variables

    [SerializeField] private ChargeType _pinataType = ChargeType.None;
    [SerializeField] private float _baseEnergyDropValue = 5;
    [SerializeField] private float _pinataEnergyDropValue = 10;
    [SerializeField] private bool _dropXpOrb = false;
    #endregion

    
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

        
        _lifeModules[0].OnDeath += DeathEvent;
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

    public void ClearPathReservation()
    {
        _movementModule.ClearPathReservation();
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

    public void OnPlayerMoving(int playerObjectId, Vector3 playerPosition)
    {
        _movementModule?.OnPlayerMoving(playerObjectId, playerPosition);
    }
        
    private void DeathEvent(int playerObjectId, ChargeType charge)
    {
        float signedEnergyAmount = GetSignedEnergyAmount(charge);
        Debug.Log(signedEnergyAmount);
        EventBus.InvokeEvent(new OnEnemyDieEvent(this, !_dropXpOrb ? 0 : signedEnergyAmount));
        
        if(_movementModule != null)
            ClearPathReservation();
        
        InvokeEvent(new OnPlayerDoKill{p_owerId = playerObjectId});
        
        if (!_dropXpOrb)
        {
            InvokeEvent(new ModifyEnergyEvent
            {
                p_player = playerObjectId,
                p_value = Mathf.Abs(signedEnergyAmount)
            });
        }
    }

    float GetSignedEnergyAmount(ChargeType charge)
    {
        switch (charge)
        {
            case ChargeType.Positive:
                if (_pinataType == ChargeType.Positive)
                    return _pinataEnergyDropValue;
                return _baseEnergyDropValue;
            
            case ChargeType.Negative:
                if (_pinataType == ChargeType.Negative)
                    return -_pinataEnergyDropValue;
                return -_baseEnergyDropValue;
            
            default:
                return 0f;
        }
    }
}

public enum ChargeType{Negative, Positive, None}