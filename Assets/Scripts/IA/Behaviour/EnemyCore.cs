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
    [SerializeField] private float _maxEnemySwelling;
    private float _swellingAmount;

    [SerializeField] private List<EnemyAttackModule> _attackingModules = new List<EnemyAttackModule>();
    [SerializeField] private List<EnemyLifeModule> _lifeModules = new List<EnemyLifeModule>();
    [SerializeField] private List<EnemyTargetModule> _targetingModules = new List<EnemyTargetModule>();
    [SerializeField] private EnemyMovementModule _movementModule;
    
    public Guid p_gridReaderId;
    public PathfindingRequestManager p_pathRequester;
    public PathfindingGridReader p_gridReader;
    [HideInInspector] public int p_enemySpawnCost;

    #region Charges Variables

    public ChargeType p_affinityType = ChargeType.None;
    [SerializeField] private int _explosionChargedDamage = 50;

    public bool p_player1_IsPositive;
    public float p_player1_ChargeMax = 5;
    public float p_current_player1_Charge;
    
    public bool p_player2_IsPositive;
    public float p_player2_ChargeMax = 5;
    public float p_current_player2_Charge;
    
    public enum ChargeType{Negative, Positive, None}
    
    #endregion

    #region Actions

    public Action p_OnChargeExplosion;
    public Action<bool, float> p_OnPlayer1ChargeChange;
    public Action<bool, float> p_OnPlayer2ChargeChange;

    #endregion

    
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        InitialiseEnemy();
        InstanceFinder.TimeManager.OnTick += OnNetworkTick;
        
        ListenToEvent<SwapingGunEvent>(TriggerExplosionOnSwap);
        
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
        PathfindingGridReader pathfindingGridReader, int cost)
    {
        p_gridReaderId = _readerId;
        p_enemySpawnCost = cost;
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
        
    private void DeathEvent(int playerObjectId)
    {
        ClearPathReservation();
        InvokeEvent(new OnPlayerDoKill{p_owerId = playerObjectId});
    }

    #region Charges

    [Server]
    public void AddCharge(bool positive, float value, int isServer)
    {
        if (isServer == 0)
        {
            if (positive != p_player1_IsPositive)
            {
                p_current_player1_Charge = 0;
            }
            
            p_player1_IsPositive = positive;
            p_current_player1_Charge += value;
            
            OnPlayer1ChangeObserverRpc(p_current_player1_Charge, p_player1_IsPositive, p_current_player1_Charge/p_player1_ChargeMax);
        }
        else
        {
            if (positive != p_player2_IsPositive)
            {
                p_current_player2_Charge = 0;
            }
            
            p_player2_IsPositive = positive;
            p_current_player2_Charge += value;
            OnPlayer2ChangeObserverRpc(p_current_player2_Charge, p_player2_IsPositive, p_current_player2_Charge/p_player2_ChargeMax); 
        }
    }

    [Server]
    private void ResetAllCharged()
    {
        p_current_player2_Charge = 0;
        p_current_player1_Charge = 0;
        ResetChargesObserverRpc();
    }
    
    [ObserversRpc]
    private void ResetChargesObserverRpc()
    {
        p_current_player2_Charge = 0;
        p_current_player1_Charge = 0;
    }
    
    [Server]
    private void TriggerExplosionOnSwap(SwapingGunEvent data)
    {
        _lifeModules[0].TakeDamage(Owner.ClientId, _explosionChargedDamage, ChargeType.None);
            
        ResetAllCharged();
        ExplosionObserversRpc();
    }

    [ObserversRpc]
    private void ExplosionObserversRpc()
    {
        p_OnChargeExplosion?.Invoke();
    }
    
    [ObserversRpc]
    private void OnPlayer2ChangeObserverRpc(float value, bool positive, float ratio)
    {
        p_current_player2_Charge = value;
        p_OnPlayer1ChargeChange?.Invoke(positive, ratio);
    }
    
    [ObserversRpc]
    private void OnPlayer1ChangeObserverRpc(float value, bool positive, float ratio)
    {
        p_current_player1_Charge = value;
        p_OnPlayer2ChargeChange?.Invoke(positive, ratio);
    }
    #endregion
}