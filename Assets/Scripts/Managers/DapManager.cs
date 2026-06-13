using System;
using System.Collections;
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

	public float GetPercentageDap => _dapPercentage.Value;
	
	#endregion

	#region Variables

	[SerializeField] private Canvas _globalCanva;

	[Header("Bars Settings")]
	[SerializeField] private float _maxEnergy = 100f;
	[SerializeField] public Transform _dapBarParent;

	[Header("Dap Settings")]
	[SerializeField] private float _percentageGainPerSecond = 2;
	[SerializeField] private float _distanceToDap = 10f;
	
	private readonly SyncVar<float> _dapPercentage = new(0);
	private readonly SyncVar<bool> _canDapping = new(false);
	
	private List<Transform> _playerList = new List<Transform>();
	
	private HashSet<int> _playersReadyToDap = new();
	private HashSet<int> _triggeredThresholds = new();
	
	[Header("Idle Detection")]
	[SerializeField] private float _idleTimeToTrigger = 20f;

	private float _lastChangeTime;
	private float _lastValue;
	private bool _idleTriggered;
	
	//Actions
	public Action<float> OnPercentageChange;
	public Action<int> OnMessageUpdate;
	public Action<Vector3> OnDapping;
	public Action<float> OnDapReachPercentage;
	
	#endregion
	
	#region Fonctions

	public override void OnStartNetwork()
	{
		ListenToEvent<OnAddDapPercentage>(AddPercentage);
		ListenToEvent<OnPlayerSpawnEvent>(OnPlayerSpawn);
		ListenToEvent<OnAskForDapp>(AskForDapp);

		_dapPercentage.OnChange += OnDapChange;
		_canDapping.OnChange += OnCanDappingChange;

		_dapPercentage.Value = 0;
		
		_lastChangeTime = Time.time;
		_lastValue = _dapPercentage.Value;
		_idleTriggered = false;
	}
	public override void OnStartClient()
	{
		OnPercentageChange?.Invoke(_dapPercentage.Value);
		
		if (!IsClientInitialized)
			return;
		
		StartCoroutine(SetupCanvas());
	}

	private IEnumerator SetupCanvas()
	{
		yield return new WaitUntil(() =>
		{
			NetworkObject localObj = InstanceFinder.ClientManager.Connection?.FirstObject;
			if (localObj == null) return false;
			return localObj.GetComponentInChildren<FPSController>() != null;
		});

		FPSController fps = InstanceFinder.ClientManager.Connection.FirstObject
			.GetComponentInChildren<FPSController>();

		Camera cam = fps.Camera.transform.GetChild(0).GetComponent<Camera>();

		_globalCanva.renderMode = RenderMode.ScreenSpaceCamera;
		_globalCanva.worldCamera = cam;
		_globalCanva.sortingLayerID = SortingLayer.NameToID("UI");
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
			_canDapping.Value = false;
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
			OnDapReachPercentage?.Invoke(GetPercentageDap);
		}
	}
	
	private void CheckThresholds(float value)
	{
		float percent = (value / _maxEnergy) * 100f;

		TryTriggerThreshold(25, percent);
		TryTriggerThreshold(50, percent);
		TryTriggerThreshold(75, percent);
		TryTriggerThreshold(100, percent);
	}

	
	private void TryTriggerThreshold(int threshold, float percent)
	{
		if (percent >= threshold && !_triggeredThresholds.Contains(threshold))
		{
			_triggeredThresholds.Add(threshold);

			InvokeEvent(new OnDapReachPercentage{percentage = threshold});
		}
	}
	
	private void OnDapChange(float prev, float next, bool asServer)
	{
		if (_dapPercentage.Value >= _maxEnergy)
		{
			_dapPercentage.Value = _maxEnergy;
			_canDapping.Value = true;

			OnMessageUpdate?.Invoke(0);
		}
		
		UpdateUI(next);
		
		CheckThresholds(next);
		
		if (!Mathf.Approximately(next, _lastValue))
		{
			_lastValue = next;
			_lastChangeTime = Time.time;
			_idleTriggered = false;
		}
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
		
		HandleIdleTimer();
		
		if (_playerList.Count < 2)
			return;

		if (_dapPercentage.Value < _maxEnergy)
		{
			_canDapping.Value = false;
			return;
		}

		_canDapping.Value = Vector3.Distance(_playerList[0].position, _playerList[1].position) <= _distanceToDap;
	}

	private void UpdateUI(float energy)
	{
		float fillAmount = Mathf.Clamp01(energy / _maxEnergy);

		OnPercentageChange?.Invoke(fillAmount);
	}
	
	private void HandleIdleTimer()
	{
		if (_idleTriggered)
			return;

		if (Time.time - _lastChangeTime >= _idleTimeToTrigger)
		{
			_idleTriggered = true;
			
			InvokeEvent(new OnDapWaitTooLongWithoutChange());
		}
	}
	
	public void SetGlobalCanvaOrder(int order) => _globalCanva.sortingOrder = order;
	
	#endregion
}