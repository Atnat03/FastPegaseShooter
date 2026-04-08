using System;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;

public class DroneThrower : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables
	
	[SerializeField] private ArmBridgeAnimation _bridgeAnimation;
	[SerializeField] private PlayerEnergy _playerEnergy;
	
	[Header("Throw")]
	[SerializeField] private DroneBullet _droneBulletPrefab;
	[SerializeField] private Drone _dronePrefab;
	[SerializeField] private Transform _spawnPoint;
    
	[Header("Settings")]
	[SerializeField] private float _cooldown = 2f;
	[SerializeField] private int _damage = 10;
	[SerializeField] private float _throwForce = 10f;
	[SerializeField] private int _numberBounces = 2;
	
	[Header("Charge Throw")]
	[SerializeField] private float _minThrowForce = 5f;
	[SerializeField] private float _maxThrowForce = 25f;
	[SerializeField] private float _maxChargeTime = 2f;

	private float _currentChargeTime = 0f;
	private bool _isCharging = false;
	private bool _isCanceled = false;

	private readonly SyncVar<bool> _canThrow = new(false);

	public bool _hasDrone = false;
	
	private DroneBullet _currentDroneInTerrain = null;
	
	//Actions
	public Action OnThrowing;
	public Action OnThrowingActivation;
	public Action OnGetDrone;
	
	#endregion


	#region Fonctions

	public override void OnStartNetwork()
	{
		if (IsServerInitialized)
		{
			_hasDrone = true;
			OnGetDrone?.Invoke();
		}
	}

	public void TryThrowDrone()
	{
		if (!_hasDrone) return;
		if (_isCanceled) return;
		
		_isCharging = false;

		float chargeRatio = _currentChargeTime / _maxChargeTime;
		float finalForce = Mathf.Lerp(_minThrowForce, _maxThrowForce, chargeRatio);
		
		ThrowDroneServerRpc(finalForce);

		_hasDrone = false;
		OnThrowing?.Invoke();
	}

	[ServerRpc]
	void ThrowDroneServerRpc(float force)
	{
		if (_currentDroneInTerrain != null)
		{
			InstanceFinder.ServerManager.Despawn(_currentDroneInTerrain.gameObject);
		}
    
		DroneBullet drone = Instantiate(_droneBulletPrefab, _spawnPoint.position, _spawnPoint.rotation);
		InstanceFinder.ServerManager.Spawn(drone.gameObject);
		_currentDroneInTerrain = drone;

		drone.SetDrone(_dronePrefab, Owner, _playerEnergy);
    
		drone.GetComponent<Rigidbody>().AddForce(_spawnPoint.forward * force, ForceMode.Impulse);
	}

	[TargetRpc]
	public void GiveDroneBackTargetRpc(NetworkConnection target)
	{
		Cons.Print("GiveDroneBackTargetRpc " + target.ClientId, ColorConsole.Pink);
		_hasDrone = true;
		OnGetDrone?.Invoke();
	}

	public void StartThrowDrone()
	{
		if (!_hasDrone) return;

		_isCharging = true;
		_currentChargeTime = 0f;
		
		OnThrowingActivation?.Invoke();
	}

	public void CancelThrow()
	{
		_cancelOffsetTime = 0;
		_isCanceled = true;
		_isCharging = false;
		_currentChargeTime = 0;
	}
	
	private void Update()
	{
		if (!IsOwner) return;

		if (_isCharging)
		{
			_currentChargeTime += Time.deltaTime;
			_currentChargeTime = Mathf.Clamp(_currentChargeTime, 0, _maxChargeTime);
		}

		if (_isCanceled)
		{
			_cancelOffsetTime +=  Time.deltaTime;

			if (_cancelOffsetTime >= 0.5f)
			{
				_isCanceled = false;
			}
		}
	}

	private float _cancelOffsetTime = 0;

	#endregion
}
