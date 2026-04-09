using System;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public struct AddEnergyEvent
{
	public NetworkConnection p_player;
	public float p_value;
}

public struct SetEnergyEvent
{
	public NetworkConnection p_player;
	public float p_ratio;
}

public class PlayerEnergy : NetworkBusListener
{
	#region Properties

	public float CurrentEnergy => _currentEnergy.Value;
	
	#endregion


	#region Variables

	[SerializeField] private float _maxEnergy;
	[SerializeField] private float _convertionTaux;
	private readonly SyncVar<float> _currentEnergy = new();
	
	//Actions
	public Action<float> OnAddEnergy;
	
	#endregion


	#region Fonctions

	public override void OnStartNetwork()
	{
		_currentEnergy.OnChange += OnEnergyChanged;

		ListenToEvent<AddEnergyEvent>(AddEnergy);
		ListenToEvent<SetEnergyEvent>(SetEnergy);

		if (IsServerInitialized)
			_currentEnergy.Value = _maxEnergy / 2f;
	}
	
	private void OnEnergyChanged(float prev, float next, bool asServer)
	{
		UpdateUI();
	}

	private void SetEnergy(SetEnergyEvent data)
	{
		if (data.p_player == Owner)
		{
			_currentEnergy.Value = data.p_ratio * _maxEnergy;
			
			_currentEnergy.Value = Mathf.Clamp(_currentEnergy.Value, 0, _maxEnergy);

			UpdateUI();
		}
	}

	private void AddEnergy(AddEnergyEvent data)
	{
		if (data.p_player == Owner)
		{
			float ratio = data.p_value * _convertionTaux;
			_currentEnergy.Value += ratio;
			
			_currentEnergy.Value = Mathf.Clamp(_currentEnergy.Value, 0, _maxEnergy);

			UpdateUI();
		}
	}
	
	void UpdateUI() =>OnAddEnergy?.Invoke(_currentEnergy.Value / _maxEnergy);
	
	#endregion
}
