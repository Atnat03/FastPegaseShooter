using System;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using UnityEngine;

public class PlayerEnergy : NetworkBusListener
{
	#region Properties

	public float CurrentEnergy => _currentEnergy.Value;
	public float EnergyOneBar => _valueOneBar;

	#endregion

	#region Variables

	[SerializeField] private float _maxEnergy = 100f;
	[SerializeField] private float _valueOneBar = 20f;
	[SerializeField] private float _convertionTaux = 10;
	
	[Header("Cost")]
	public int p_costThrowGrenade = 1;
	public int p_costThrowDrone = 2;
	public int p_costThrowHeal = 1;

	private int _totalBars;

	private readonly SyncVar<float> _currentEnergy = new();

	//Events UI
	public Action<int> OnCreateBarUI;
	public Action<int, float> OnUpdateUI;

	#endregion

	#region Fonctions

	public override void OnStartNetwork()
	{
		_currentEnergy.OnChange += OnEnergyChanged;

		ListenToEvent<ModifyEnergyEvent>(ModifyEnergy);
		ListenToEvent<ConsumeEnergyEvent>(ConsumeEnergy);
		ListenToEvent<SetEnergyEvent>(SetEnergy);

		_totalBars = Mathf.CeilToInt(_maxEnergy / _valueOneBar);

		if (IsServerInitialized)
			_currentEnergy.Value = _maxEnergy;

		//Créer UI
		OnCreateBarUI?.Invoke(_totalBars);

		UpdateUI(_currentEnergy.Value);
	}
	
	public override void OnStartClient()
	{
		_currentEnergy.OnChange += OnEnergyChanged;
	}
	
	private void OnEnergyChanged(float prev, float next, bool asServer)
	{
		UpdateUI(next);
	}

	private void SetEnergy(SetEnergyEvent data)
	{
		if (data.p_player != Owner) return;

		_currentEnergy.Value = Mathf.Clamp(data.p_ratio * _maxEnergy, 0, _maxEnergy);
		UpdateUI(_currentEnergy.Value);
	}

	private void ModifyEnergy(ModifyEnergyEvent data)
	{
		if (!IsServerInitialized) return;
		if (data.p_player != OwnerId) return;

		_currentEnergy.Value += data.p_value * _convertionTaux;
		_currentEnergy.Value = Mathf.Clamp(_currentEnergy.Value, 0, _maxEnergy);
	}
	
	private void ConsumeEnergy(ConsumeEnergyEvent data)
	{
		if (!IsServerInitialized) return;
		if (data.p_player != Owner) return;
		
		_currentEnergy.Value += data.p_value;

		_currentEnergy.Value = Mathf.Clamp(_currentEnergy.Value, 0, _maxEnergy);
	}


	private void UpdateUI(float energy)
	{
		energy = Mathf.Clamp(energy, 0f, _maxEnergy);

		int activeBarIndex = Mathf.FloorToInt(energy / _valueOneBar);
		float activeFill = (energy % _valueOneBar) / _valueOneBar;

		//Cas énergie max
		if (Mathf.Approximately(energy, _maxEnergy))
		{
			activeBarIndex = _totalBars - 1;
			activeFill = 1f;
		}
		

		OnUpdateUI?.Invoke(activeBarIndex, activeFill);
	}

	public bool CanThrow(float nbBar)
	{
		return (CurrentEnergy - nbBar * _valueOneBar) >= 0;
	}

	#endregion
}

public struct AddHealthFromBarEvent
{
	public float value;
}

public struct ModifyEnergyEvent
{
	public int p_player;
	public float p_value;
}

public struct ConsumeEnergyEvent
{
	public NetworkConnection p_player;
	public float p_value;
}

public struct SetEnergyEvent
{
	public NetworkConnection p_player;
	public float p_ratio;
}