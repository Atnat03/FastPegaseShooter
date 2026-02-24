using System;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class SpawnPlayer : NetworkBehaviour
{
	#region Variables

	[SerializeField] private NetworkObject _playerPrefab;
	[SerializeField] private NetworkObject _playerCamera;

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
		NetworkObject playerObj = Instantiate(_playerPrefab);
		InstanceFinder.ServerManager.Spawn(playerObj, player);
		
		SetUpLayerTargetRpc(player, playerObj.GetComponent<FPSController>());
	}

	[TargetRpc]
	private void SetUpLayerTargetRpc(NetworkConnection conn, FPSController fpsController)
	{
		fpsController.SetUpLayer();
	}
	
	#endregion
}
