using System;
using System.Collections.Generic;
using Controller;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class BasicEnemyLife : NetworkBehaviour, IDamagable
{
    [SerializeField] private int _life;
    public readonly SyncVar<int> p_life = new SyncVar<int>();
    [SerializeField] private float _energyGainWhenTouch = 1;

    private Guid _gridReaderId;
    private int _enemySpawnCost;
    [HideInInspector] public float p_damageMultiplier = 1; //used by Elite WeakPoints
    
    [Header("HitMark")] //visuals
    [SerializeField] private Transform _hitMarkerParent;
    [SerializeField] private TextMeshProUGUI _textDmg;
    [SerializeField] private TextMeshProUGUI _textDmgCritique;
    [SerializeField] private int _cumulatifDmg = 0;
    [SerializeField] private float _elapsedCumulativeDmgTime = 0;
    private TextMeshProUGUI _hitMarker;

    private List<Action> _unsubscribeEvents = new List<Action>();
    private EventBus _bus;

    #region Init
    private void Awake()
    {
        _bus = EventBusInitialiser.instance.Bus;
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        p_life.Value = _life;
        p_life.OnChange += OnLifeChanged;
        _unsubscribeEvents.Add(_bus.Subscribe((SwapingGunEvent SGE) => p_damageMultiplier = SGE.dataSurcharge.damageMultiplier));
        _unsubscribeEvents.Add(_bus.Subscribe((EndOverloadEvent EOE) => p_damageMultiplier = 1));
    }
    public override void OnStopServer()
    {
        p_life.OnChange -= OnLifeChanged;

        foreach (Action unsubscribeEvent in _unsubscribeEvents)
        {
            unsubscribeEvent?.Invoke();
        }
        base.OnStopServer();
    }
    public void SetInfos(Guid _readerId, int cost)
    {
        _gridReaderId = _readerId;
        _enemySpawnCost = cost;
    }
    #endregion

    #region Server Logic
    [Server]
    protected void OnLifeChanged(int prev, int next, bool asServer)
    {
        if (next <= 0)
        {
            if (asServer)
            {
                Death(prev-next); // serveur uniquement
            }
        }
    }

    protected int GetDamageAmount(int rawDamage)
    {
        return Mathf.RoundToInt(rawDamage * p_damageMultiplier);
    }
    
    [Server]
    public bool TakeDamage(int rawDamageAmount, bool isCritical = false)
    {
        if (IsServerInitialized)
        {
            int damages = GetDamageAmount(rawDamageAmount);
            p_life.Value -= damages;
            TriggerHitMarkObserversRpc(isCritical, damages);
        }

        //No specific logic modifying critical behaviour
        return isCritical;
    }
    [Server]
    public void Death(int takenDamages)
    {
        foreach (Action unsubscribeEvent in _unsubscribeEvents)
        {
            unsubscribeEvent?.Invoke();
        }
        
        InstanceFinder.ServerManager.Despawn(gameObject);
        EventBusInitialiser.instance.Bus.InvokeEvent(new EnemyDyingEvent(_gridReaderId, _enemySpawnCost));
    }
    #endregion

    #region Client Logic
    [ObserversRpc]
    public void TriggerHitMarkObserversRpc(bool IsCritique, float dmg)
    {
        if (IsCritique)
        {
            _bus.InvokeEvent(new OnModifyEnergyEvent { value = _energyGainWhenTouch });
        }

        _cumulatifDmg += (int)dmg;

        if (_elapsedCumulativeDmgTime <= 0)
        {
            TextMeshProUGUI text = IsCritique ? _textDmgCritique : _textDmg;
            _hitMarker = Instantiate(text.gameObject, _hitMarkerParent).GetComponent<TextMeshProUGUI>();
            _hitMarker.SetText(_cumulatifDmg.ToString());
            _elapsedCumulativeDmgTime = 0.05f;

            Destroy(_hitMarker.gameObject, 0.5f);
        }
        else
        {
            if (_hitMarker != null)
                _hitMarker.SetText(_cumulatifDmg.ToString());
        }
    }

    private void Update()
    {
        if (_elapsedCumulativeDmgTime > 0)
        {
            _elapsedCumulativeDmgTime -= Time.deltaTime;
        }
        else
        {
            _cumulatifDmg = 0;
        }
    }
    #endregion
}

public struct EnemyDyingEvent
{
    public Guid p_gridReaderId;
    public int p_enemySpawnCost;

    public EnemyDyingEvent(Guid id, int cost)
    {
        p_gridReaderId = id;
        p_enemySpawnCost = cost;
    }
}

