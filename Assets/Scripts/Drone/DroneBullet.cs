using System;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using MyPrint;
using UnityEngine;

public class DroneBullet : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables

	Rigidbody rb;
	private Drone _dronePrefab;
	private bool _hasSpawned;
	private NetworkConnection _throwerId;
	private PlayerEnergy _playerEnergy;
	
	#endregion


	#region Fonctions

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}
	
	public void SetDrone(Drone dronePrefab, NetworkConnection throwerId, PlayerEnergy playerEnergy)
	{
		_dronePrefab = dronePrefab;
		_throwerId = throwerId;
		_playerEnergy = playerEnergy;
	}
	
	private void OnCollisionEnter(Collision collision)
	{
		if (!IsServerInitialized || _hasSpawned)
			return;

		_hasSpawned = true;

		rb.useGravity = false;
		rb.isKinematic = true;

		SpawnDrone();
		
		InstanceFinder.ServerManager.Despawn(gameObject);
	}
	
	private void SpawnDrone()
	{
		Drone droneInstance = Instantiate(_dronePrefab, transform.position, Quaternion.identity);
		InstanceFinder.ServerManager.Spawn(droneInstance.gameObject);
		//droneInstance.SetThrower(_throwerId, _playerEnergy);
	}

	#endregion
}
