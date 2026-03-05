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

public class EnergyManager : NetworkBehaviour
{
	#region Properties
	
	public float CurrentEnergy => _currentEnergy.Value;
	
	#endregion


	#region Variables

	[SerializeField] private float _energyMax = 100f;
	[SerializeField] private float _valueOneBar = 20f;
	[SerializeField] private float _healFromOneBar = 10f;

	private int _totalBars;
	private List<Image> _energyBarsImageList = new List<Image>();

	private readonly SyncVar<float> _currentEnergy = new SyncVar<float>();
	
	private float _displayedEnergy;
	private float _targetEnergy;
	private bool _isLerping;
	private int _previousIndexFull;

	[SerializeField] private Image _imageBarPrefab;
	[SerializeField] private Transform _barParent;
	
	[SerializeField] private Color _energyBarColorFull;
	[SerializeField] private Color _energyBarColorNotFull;

	private EventBus _bus;

	#endregion


	#region Fonctions

	public override void OnStartServer()
	{
		_currentEnergy.Value = _energyMax;

		_bus = EventBusInitialiser.instance.Bus;
	}
	
	public override void OnStartClient()
	{
		_bus = EventBusInitialiser.instance.Bus;
		_bus.Subscribe((OnModifyEnergyEvent data) => ModifyEnergyServerRpc(data.value));
		_bus.Subscribe((RequestEnergyEvent data) => 
		{
			_bus.InvokeEvent(new RequestEnergyResponseEvent { energy = _currentEnergy.Value });
		});
		
		_totalBars = Mathf.CeilToInt(_energyMax / _valueOneBar);

		for (int i = 0; i < _totalBars; i++)
		{
			Image newImage = Instantiate(_imageBarPrefab, _barParent);
			_energyBarsImageList.Add(newImage);
		}
		
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
		_currentEnergy.Value = Mathf.Clamp(_currentEnergy.Value + amount, 0f, _energyMax);
	}
	
	private void OnEnergyChanged(float prev, float next, bool asServer)
	{
		if (asServer) return;

		_targetEnergy = next;
		_isLerping = true;
		_bus.InvokeEvent(new RequestEnergyResponseEvent { energy = next });
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

		if (_previousIndexFull < activeBarIndex)
		{
			AddHealthObserversRpc();
		}
		
		_previousIndexFull = activeBarIndex;
		
		for (int i = 0; i < _energyBarsImageList.Count; i++)
		{
			if (i < activeBarIndex)
			{
				_energyBarsImageList[i].fillAmount = 1f;
				_energyBarsImageList[i].color = _energyBarColorFull;
			}
			else if (i == activeBarIndex)
			{
				_energyBarsImageList[i].fillAmount = activeFill;
				
				if(activeFill >= 1)
					_energyBarsImageList[i].color = _energyBarColorFull;
				else
					_energyBarsImageList[i].color = _energyBarColorNotFull;
			}
			else
			{
				_energyBarsImageList[i].fillAmount = 0f;
			}

		}
	}

	[Server]
	public void AddEnergy(float amount)
	{
		_currentEnergy.Value = Mathf.Clamp(_currentEnergy.Value + amount, 0f, _energyMax);
	}

	[Server]
	public void RemoveEnergy(float amount)
	{
		_currentEnergy.Value = Mathf.Clamp(_currentEnergy.Value - amount, 0f, _energyMax);
	}
	
	[ObserversRpc]
	private void AddHealthObserversRpc()
	{
		Debug.Log("Add health");
		_bus.InvokeEvent(new AddHealthFromBarEvent
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
