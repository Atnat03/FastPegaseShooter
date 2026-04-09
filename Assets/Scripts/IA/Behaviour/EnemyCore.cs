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
    private List<ScoreTargetModule> _scoreModules = new List<ScoreTargetModule>();

    public Guid p_gridReaderId;
    public PathfindingRequestManager p_pathRequester;
    public PathfindingGridReader p_gridReader;
    [HideInInspector] public int p_enemySpawnCost;

    #region Charges Variables

    [Header("Charges")] 
    [SerializeField] private int _explosionChargedDamage = 50;
    
    [SerializeField] private float _negativeChargeMax = 5;
    private readonly SyncVar<float> _currentN_charge = new SyncVar<float>();
    
    [SerializeField] private float _positiveChargeMax = 5;
    private readonly SyncVar<float> _currentP_charge = new SyncVar<float>();

    [Header("View")] 
    [SerializeField] private GameObject _positiveUI;
    [SerializeField] private Image _positiveCurValue;
    [SerializeField] private GameObject _negativeUI;
    [SerializeField] private Image _negativeCurValue;
    
    [SerializeField] private ParticleSystem _explosionParticle;
    
    #endregion

    
    public override void OnStartServer()
    {
        InitialiseEnemy();
        InstanceFinder.TimeManager.OnTick += OnNetworkTick;

        foreach (EnemyTargetModule targetModule in _targetingModules)
        {
            if (targetModule != null && targetModule is ScoreTargetModule scoreTargetModule)
                _scoreModules.Add(scoreTargetModule);
        }

        foreach (EnemyAttackModule module in _attackingModules)
        {
            if (module == null) continue;
            foreach (ScoreTargetModule scoreModule in _scoreModules)
            {
                if (scoreModule != null)
                    module.p_onHitPlayer += scoreModule.OnHitPlayer;
            }
        }

        foreach (EnemyLifeModule module in _lifeModules)
        {
            if (module == null) continue;
            foreach (ScoreTargetModule scoreModule in _scoreModules)
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

    public void AddCharge(bool positive, float value)
    {
        if (!IsServerInitialized)
            return;
        
        if (positive)
        {
            _currentP_charge.Value += value;
        }
        else
        {
            _currentN_charge.Value += value;
        }

        CheckAllChargeAreFull();
    }

    private void ResetAllCharged()
    {
        _currentP_charge.Value = 0;
        _currentN_charge.Value = 0;
    }
    
    private void CheckAllChargeAreFull()
    {
        if (_currentP_charge.Value >= _positiveChargeMax && _currentN_charge.Value >= _negativeChargeMax)
        {
            foreach (EnemyLifeModule life in _lifeModules)
            {
                life.TakeDamage(Owner.ClientId, _explosionChargedDamage);
            }
            
            ResetAllCharged();
            ExplosionObserversRpc();
        }
    }

    [ObserversRpc]
    private void ExplosionObserversRpc()
    {
        Destroy(Instantiate(_explosionParticle, transform.position + Vector3.up, Quaternion.identity), 2f);
    }

    private void OnEnable()
    {
        _currentP_charge.OnChange += OnPositiveChange;
        _currentN_charge.OnChange += OnNegativeChange;
    }

    private void OnPositiveChange(float prev, float next, bool asServer)
    {
        _positiveUI.SetActive(next > 0);
        _positiveCurValue.fillAmount = next / _positiveChargeMax;
    }
    
    private void OnNegativeChange(float prev, float next, bool asServer)
    {
        _negativeUI.SetActive(next > 0);
        _negativeCurValue.fillAmount = next / _negativeChargeMax;
    }
    
    #endregion
}