using System;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnPlayer : NetworkBehaviour
{
	#region Variables

	[SerializeField] private NetworkObject _playerPrefab;
	[SerializeField] private Transform[] _spawnPoints;

	#endregion
	
	#region Fonctions

	public void Awake()
	{
		InstanceFinder.ServerManager.OnAuthenticationResult += SpawnPlayers;
	}

	public void OnDestroy()
	{
		InstanceFinder.ServerManager.OnAuthenticationResult -= SpawnPlayers;
	}

	[Server]
	private void SpawnPlayers(NetworkConnection player, bool DidConnect)
	{
		Debug.Log("SpawnPlayers called");
		
		NetworkObject playerObj = Instantiate(_playerPrefab);
		InstanceFinder.ServerManager.Spawn(playerObj, player);
		
		Vector3 randomPos = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;
		
		playerObj.transform.position = randomPos;
		
		SetUpLayerTargetRpc(player, playerObj.GetComponent<FPSController>());
	}

	[TargetRpc]
	private void SetUpLayerTargetRpc(NetworkConnection conn, FPSController fpsController)
	{
		fpsController.SetUpLayer();
	}
	
	#endregion
}
