using System;
using FishNet.Object.Synchronizing;
using UnityEngine;

public struct OnAddDapPercentage
{ }

public class DapManager : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[Header("Bars Settings")]
	[SerializeField] private float _maxEnergy = 100f;
	[SerializeField] private float _valueOneBar = 20f;
	
	[Header("Dap Settings")]
	[SerializeField] private float _percentageGainPerSecond = 2;
	
	private int _totalBars;
	
	private readonly SyncVar<float> _dapPercentage = new(0);
	
	//Actions
	public Action<int, float> OnPercentageChange;
	public Action<int> OnCreateBarUI;
	
	#endregion


	#region Fonctions

	public override void OnStartNetwork()
	{
		ListenToEvent<OnAddDapPercentage>(AddPercentage);

		_dapPercentage.OnChange += OnDapChange;
		
		_totalBars = Mathf.CeilToInt(_maxEnergy / _valueOneBar);
		
		if (IsServerInitialized)
			_dapPercentage.Value = _maxEnergy;
		
		OnCreateBarUI?.Invoke(_totalBars);
	}

	public override void OnStartClient()
	{
		_dapPercentage.OnChange += OnDapChange;
	}
	
	private void AddPercentage(OnAddDapPercentage data)
	{
		if (IsServerInitialized)
		{
			_dapPercentage.Value += _percentageGainPerSecond;
		}
	}
	
	private void OnDapChange(float prev, float next, bool asServer)
	{
		UpdateUI(next);
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
		
		OnPercentageChange?.Invoke(activeBarIndex, activeFill);
	}
	
	#endregion
}