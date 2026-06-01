using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using MyPrint;
using Unity.Networking.Transport;
using UnityEngine;

public struct OnAddDapPercentage
{
	public float p_ratio;
}

public struct OnAskForDapp
{
	public int p_connection;
}

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
	[SerializeField] private float _distanceToDap = 10f;
	
	private int _totalBars;
	
	private readonly SyncVar<float> _dapPercentage = new(0);
	private readonly SyncVar<bool> _canDapping = new(false);
	
	private List<Transform> _playerList = new List<Transform>();
	
	private HashSet<int> _playersReadyToDap = new();
	
	//Actions
	public Action<int, float> OnPercentageChange;
	public Action<int> OnCreateBarUI;
	public Action<int> OnMessageUpdate;
	public Action<Vector3> OnDapping;
	
	#endregion
	
	#region Fonctions

	public override void OnStartNetwork()
	{
		ListenToEvent<OnAddDapPercentage>(AddPercentage);
		ListenToEvent<OnPlayerSpawnEvent>(OnPlayerSpawn);
		ListenToEvent<OnAskForDapp>(AskForDapp);

		_dapPercentage.OnChange += OnDapChange;
		_canDapping.OnChange += OnCanDappingChange;
		
		_totalBars = Mathf.CeilToInt(_maxEnergy / _valueOneBar);
		
		OnCreateBarUI?.Invoke(_totalBars);
		_dapPercentage.Value = 0;
	}
	public override void OnStartClient()
	{
		_dapPercentage.OnChange += OnDapChange;
	}

	private void OnPlayerSpawn(OnPlayerSpawnEvent data)
	{
		_playerList.Add(data.Transform);
	}
	
	private void AskForDapp(OnAskForDapp data)
	{
		if (!IsServerInitialized)
			return;
		
		if(!_canDapping.Value)
			return;
		
		_playersReadyToDap.Add(data.p_connection);

		AskForDapObserverRpc();

		if (_playersReadyToDap.Count >= 2)
		{
			Vector3 pos = (_playerList[0].position + _playerList[1].position) / 2f;
			DappingObserverRpc(pos);
			
			InvokeEvent(new OnDapEvent());

			_dapPercentage.Value = 0;
			_playersReadyToDap.Clear();
		}
	}
	
	[ObserversRpc]
	void AskForDapObserverRpc()
	{
		OnMessageUpdate?.Invoke(2);
	}

	[ObserversRpc]
	private void DappingObserverRpc(Vector3 pos)
	{
		Cons.Print("DAPPING !!", ColorConsole.Orange);
		
		OnMessageUpdate?.Invoke(-1);
		
		OnDapping?.Invoke(pos);
	}

	private void AddPercentage(OnAddDapPercentage data)
	{
		if (IsServerInitialized)
		{
			_dapPercentage.Value += _percentageGainPerSecond * data.p_ratio;
		}
	}
	
	private void OnDapChange(float prev, float next, bool asServer)
	{
		if (_dapPercentage.Value >= _maxEnergy)
		{
			OnMessageUpdate?.Invoke(0);
		}
		
		UpdateUI(next);
	}
	
	private void OnCanDappingChange(bool prev, bool next, bool asServer)
	{
		if (next)
		{
			OnMessageUpdate?.Invoke(1);
		}
		else
		{
			OnMessageUpdate?.Invoke(0);
		}
	}

	private void Update()
	{
		if (!IsServerInitialized) 
			return;
		
		if (_dapPercentage.Value >= _maxEnergy)
		{
			_canDapping.Value = 
				(_playerList[0].position-_playerList[1].position).magnitude <= _distanceToDap;
		}
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