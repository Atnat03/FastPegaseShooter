using System;
using FishNet;
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
	private int _throwerId;
	
	#endregion


	#region Fonctions

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}
	
	public void SetDrone(Drone dronePrefab, int throwerId)
	{
		_dronePrefab = dronePrefab;
		_throwerId = throwerId;
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
		droneInstance.SetThrower(_throwerId);
	}

	#endregion
}
