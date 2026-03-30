using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UI;

public interface IEnergyRequest
{
    public void OnGetEnergy(float energy);
}

public class EnergyManager : NetworkBusListener
{
    #region Properties
    
    public float CurrentEnergy => _currentEnergy.Value;
    public bool IsEnergyFull => _currentEnergy.Value >= _energyMax;
    
    #endregion

    #region Variables

    [SerializeField] private float _energyMax = 100f;
    [SerializeField] private float _valueOneBar = 20f;
    [SerializeField] private float _healFromOneBar = 10f;

    private int _totalBars;

    private readonly SyncVar<float> _currentEnergy = new SyncVar<float>();
    
    private float _displayedEnergy;
    private float _targetEnergy;
    private bool _isLerping;
    private int _previousIndexFull;
    
    //Actions
    public Action<int> OnCreateBarUI;
    public Action<int, float> OnUpdateUI;

    #endregion

    #region Fonctions

    public override void OnStartServer()
    {
        _currentEnergy.Value = _energyMax;
    }
    
    public override void OnStartClient()
    {
        ListenToEvent<OnModifyEnergyEvent>(data => ModifyEnergyServerRpc(data.value));
        
        ListenToEvent<RequestEnergyEvent>(data => data.requester.OnGetEnergy(_currentEnergy.Value));
        

        _totalBars = Mathf.CeilToInt(_energyMax / _valueOneBar);
        
        OnCreateBarUI?.Invoke(_totalBars);
        
        _displayedEnergy = _currentEnergy.Value;
        _targetEnergy = _currentEnergy.Value;
        _previousIndexFull = _totalBars;

        _currentEnergy.OnChange += OnEnergyChanged;

        UpdateVisualBars(_currentEnergy.Value);
    }

    private void Update()
    {
        if (!IsClientInitialized || !_isLerping) return;
        
        _displayedEnergy = Mathf.Lerp(_displayedEnergy, _targetEnergy, Time.deltaTime * 20);

        if (Mathf.Abs(_displayedEnergy - _targetEnergy) < 0.01f)
        {
            _displayedEnergy = _targetEnergy;
            _isLerping = false;
        }

        UpdateVisualBars(_displayedEnergy);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void ModifyEnergyServerRpc(float amount)
    {
        float prev = _currentEnergy.Value;
        _currentEnergy.Value = Mathf.Clamp(_currentEnergy.Value + amount, 0f, _energyMax);
    
        int prevBarIndex = Mathf.FloorToInt(prev / _valueOneBar);
        int newBarIndex = Mathf.FloorToInt(_currentEnergy.Value / _valueOneBar);
    
        if (newBarIndex > prevBarIndex)
        {
            int barsCompleted = newBarIndex - prevBarIndex;
            for (int i = 0; i < barsCompleted; i++)
            {
                AddHealthObserversRpc();
            }
        }
    }
    
    private void OnEnergyChanged(float prev, float next, bool asServer)
    {
        if (asServer) return;

        _targetEnergy = next;
        _isLerping = true;
    }

    private void UpdateVisualBars(float energy)
    {
        energy = Mathf.Clamp(energy, 0f, _energyMax);

        int activeBarIndex = Mathf.FloorToInt(energy / _valueOneBar);
        float activeFill = (energy % _valueOneBar) / _valueOneBar;

        if (Mathf.Approximately(energy, _energyMax))
        {
            activeBarIndex = _totalBars - 1;
            activeFill = 1f;
        }

        _previousIndexFull = activeBarIndex;
        
        OnUpdateUI?.Invoke(activeBarIndex, activeFill);
    }

    [Server]
    public void AddEnergy(float amount)
    {
        float prev = _currentEnergy.Value;
        _currentEnergy.Value = Mathf.Clamp(_currentEnergy.Value + amount, 0f, _energyMax);
    
        int prevBarIndex = Mathf.FloorToInt(prev / _valueOneBar);
        int newBarIndex = Mathf.FloorToInt(_currentEnergy.Value / _valueOneBar);
    
        if (newBarIndex > prevBarIndex)
        {
            int barsCompleted = newBarIndex - prevBarIndex;
            for (int i = 0; i < barsCompleted; i++)
            {
                AddHealthObserversRpc();
            }
        }
    }
    
    [ObserversRpc]
    private void AddHealthObserversRpc()
    {
        Debug.Log("Add health");
        InvokeEvent(new AddHealthFromBarEvent
        {
            value = _healFromOneBar
        });
    }

    #endregion
}

public struct AddHealthFromBarEvent
{
    public float value;
}

public struct OnModifyEnergyEvent
{
    public float value;
}