using System;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using UnityEngine;

public class DroneThrower : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables
	
	[SerializeField] private ArmBridgeAnimation _bridgeAnimation;
	
	[Header("Throw")]
	[SerializeField] private DroneBullet _droneBulletPrefab;
	[SerializeField] private Drone _dronePrefab;
	[SerializeField] private Transform _spawnPoint;
    
	[Header("Settings")]
	[SerializeField] private float _cooldown = 2f;
	[SerializeField] private int _damage = 10;
	[SerializeField] private float _throwForce = 10f;
	[SerializeField] private int _numberBounces = 2;

	private readonly SyncVar<bool> _canThrow = new(true);
	private readonly SyncVar<float> _elapsedTimeCooldown = new(0);

	private DroneBullet _currentDroneInTerrain = null;
	
	//Actions
	public Action OnStartThrow;
	public Action<float> OnCooldownUpdate;
	public Action<DroneBullet> OnThrow;
	
	#endregion


	#region Fonctions

	public override void OnStartNetwork()
	{
		_elapsedTimeCooldown.OnChange += OnCooldownValueChange;
	}

	private void OnCooldownValueChange(float prev, float next, bool asServer)
	{
		OnCooldownUpdate?.Invoke(next / _cooldown);
	}

	private void Update()
	{
		if (!IsServerInitialized) return;
		
		if (_elapsedTimeCooldown.Value > 0)
		{
			_elapsedTimeCooldown.Value -= Time.deltaTime;
			_canThrow.Value = false;
			
			if (_elapsedTimeCooldown.Value <= 0f)
			{
				_canThrow.Value = true;
			}
		}
	}

	public void TryThrowDrone()
	{
		if (_canThrow.Value)
		{
			ThrowDroneServerRpc();
            
			OnStartThrow?.Invoke();
		}
	}

	[ServerRpc]
	void ThrowDroneServerRpc()
	{
		if (_currentDroneInTerrain != null)
		{
			InstanceFinder.ServerManager.Despawn(_currentDroneInTerrain.gameObject);
		}
		
		DroneBullet drone = Instantiate(_droneBulletPrefab, _spawnPoint.position, _spawnPoint.rotation);
		InstanceFinder.ServerManager.Spawn(drone.gameObject);
		_currentDroneInTerrain = drone;
    
		drone.SetDrone(_dronePrefab, Owner);
		drone.GetComponent<Rigidbody>().AddForce(_spawnPoint.forward * _throwForce, ForceMode.Impulse);
		
		_elapsedTimeCooldown.Value = _cooldown;
	}
	
	#endregion
}
