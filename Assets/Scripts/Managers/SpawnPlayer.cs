using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class SpawnPlayer : NetworkBehaviour
{
    [SerializeField] private string _sceneToDisable;
    
    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private Transform[] _spawnPoints;

    private HashSet<int> _spawnedClients = new HashSet<int>();

    private void OnEnable()
    {
        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoadEnd;
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnPlayerConnectionState;
    }

    private void OnDisable()
    {
        InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnPlayerConnectionState;
    }

    private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
    {
        if (!args.QueueData.AsServer) return;

        foreach (var conn in InstanceFinder.ServerManager.Clients.Values)
        {
            if (!_spawnedClients.Contains(conn.ClientId))
            {
                SpawnPlayers(conn);
                _spawnedClients.Add(conn.ClientId);
            }
        }
    }

    private void OnPlayerConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState != RemoteConnectionState.Started) return;
        if (_spawnedClients.Contains(conn.ClientId)) return;

        SpawnPlayers(conn);
        _spawnedClients.Add(conn.ClientId);
    }

    [Server]
    private void SpawnPlayers(NetworkConnection player)
    {
        Debug.Log("SpawnPlayers called");
		
        NetworkObject playerObj = Instantiate(_playerPrefab);
        InstanceFinder.ServerManager.Spawn(playerObj, player);
		
		Vector3 randomPos = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;
		
        playerObj.transform.position = randomPos;

        FPSController fps = playerObj.GetComponent<FPSController>();
        
        if(fps!=null)
            SetUpLayerTargetRpc(player, fps);
    }
    
    [TargetRpc]
    private void SetUpLayerTargetRpc(NetworkConnection conn, FPSController fpsController)
    {
        fpsController.SetUpLayer();
    }
}