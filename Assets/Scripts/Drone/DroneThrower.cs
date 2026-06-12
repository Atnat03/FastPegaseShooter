using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;

public class DroneThrower : NetworkBusListener
{
	public NetworkObject Bro => _bro.Value;
	
	#region Variables
	
	[SerializeField] private ArmBridgeAnimation _bridgeAnimation;
	[SerializeField] private GunSwitching _gunSwitching;
	[SerializeField] private PlayerCapacity _playerCapacity;
	
	[Header("Throw")]
	[SerializeField] private Drone _dronePrefab;
	[SerializeField] private Transform _spawnPoint;
	
	private float _currentChargeTime = 0f;
	private bool _isCharging = false;
	private bool _isCanceled = false;

	private static readonly List<DroneThrower> Players = new();
	private readonly SyncVar<NetworkObject> _bro = new(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
	
	private readonly SyncVar<bool> _canThrow = new(false);
	
	private Drone _currentDroneInTerrain = null;

	public bool p_unlockCapa = true;
	
	//Actions
	public Action OnThrowing;
	public Action OnGetDrone;
	
	#endregion
	
	#region Fonctions
	
	public override void OnStartServer()
	{
		base.OnStartServer();

		Players.Add(this);

		if (Players.Count == 2)
		{
			Players[0]._bro.Value = Players[1].NetworkObject;
			Players[1]._bro.Value = Players[0].NetworkObject;
		}
	}

	public override void OnStopServer()
	{
		Players.Remove(this);
	}

	public override void OnStartNetwork()
	{
		if (IsServerInitialized)
		{
			OnGetDrone?.Invoke();
		}
	}
	
	public void TryThrowDrone()
	{
		if (_isCanceled) return;
		if (Bro == null)
		{
			Debug.LogWarning("Bro not assigned!");
			return;
		}
		if (!_playerCapacity.CanDrone) return;
		if (!p_unlockCapa) return;
		
		InvokeEvent(new OnUseCapacity
		{
			p_capacityData = Capacity.Drone
		});
		
		InvokeEvent(new OnDataLog
		{
			entityName = transform.GetRootTransform().gameObject.name,
			EntityID = ObjectId,
			weapon = "Drone",
			skillUsed = "Drone",
			ArenaID = -1,
		});
		
		InvokeEvent(new OnDroneUsed_TUTO());
		
		_isCharging = false;

		NetworkObject targetNetObj = Bro;

		if (_bridgeAnimation != null)
		{
			_bridgeAnimation.StartThrowDrone(targetNetObj);
			_gunSwitching.IGunMain.TryCancelShooting();
			_gunSwitching.ISurchargeMain.StopReload();
		}
		else
		{
			ThrowDroneServerRpc(targetNetObj);
		}

		OnThrowing?.Invoke();

	}
	
	[ServerRpc]
	public void ThrowDroneServerRpc(NetworkObject targetNetObj)
	{
		if (_currentDroneInTerrain != null)
		{
			InstanceFinder.ServerManager.Despawn(_currentDroneInTerrain.gameObject);
		}
		
		_currentDroneInTerrain = Instantiate(_dronePrefab, _spawnPoint.position, Quaternion.identity);
		InstanceFinder.ServerManager.Spawn(_currentDroneInTerrain.gameObject);
		
		_currentDroneInTerrain.SetTarget(targetNetObj.transform, _gunSwitching.IsPositive);
	}

	[TargetRpc]
	public void GiveDroneBackTargetRpc(NetworkConnection target)
	{
		OnGetDrone?.Invoke();
	}

	#endregion
}
