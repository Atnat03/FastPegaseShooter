using System;
using FishNet;
using FishNet.Object;
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

	private bool _canThrow = true;
	private float _elapsedTimeCooldown = 0f;

	private DroneBullet _currentDroneInTerrain = null;
	
	//Actions
	public Action OnStartThrow;
	public Action<float> OnCooldownUpdate;
	public Action<DroneBullet> OnThrow;
	
	#endregion


	#region Fonctions

	private void Update()
	{
		if (_elapsedTimeCooldown > 0)
		{
			_elapsedTimeCooldown -= Time.deltaTime;
			_canThrow = false;
            
			OnCooldownUpdate?.Invoke(_elapsedTimeCooldown / _cooldown);
            
			if (_elapsedTimeCooldown <= 0f)
			{
				_canThrow = true;
			}
		}
	}

	public void TryThrowDrone()
	{
		if (_canThrow)
		{
			ThrowDroneServerRpc();
			
			if (_bridgeAnimation != null)
			{
				//_bridgeAnimation.StartThrow();
			}
			else
			{
			}
            
			OnStartThrow?.Invoke();
            
			_elapsedTimeCooldown = _cooldown;
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
    
		drone.SetDrone(_dronePrefab, Owner.ClientId);
		drone.GetComponent<Rigidbody>().AddForce(_spawnPoint.forward * _throwForce, ForceMode.Impulse);
	}
	
	#endregion
}
