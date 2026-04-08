using System;
using FishNet;
using FishNet.Connection;
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

	private readonly SyncVar<bool> _canThrow = new(false);

	public bool _hasDrone = false;
	
	private DroneBullet _currentDroneInTerrain = null;
	
	//Actions
	public Action OnStartThrow;
	public Action<float> OnCooldownUpdate;
	public Action<DroneBullet> OnThrow;
	
	#endregion


	#region Fonctions

	public override void OnStartNetwork()
	{
		if (IsServerInitialized)
			_hasDrone = true;
	}

	public void TryThrowDrone()
	{
		if (_hasDrone)
		{
			ThrowDroneServerRpc();
			_hasDrone = false;
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
	}

	[TargetRpc]
	public void GiveDroneBackTargetRpc(NetworkConnection target)
	{
		Cons.Print("GiveDroneBackTargetRpc " + target.ClientId, ColorConsole.Pink);
		_hasDrone = true;
	}
	
	#endregion
}
