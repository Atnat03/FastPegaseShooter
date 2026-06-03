using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using MyPrint;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class SpawnPlayer : NetworkBehaviour
{
    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private LevelSpawnPoints[] _spawnPointsByLevel;

    private HashSet<int> _spawnedClients = new HashSet<int>();
    private List<NetworkObject> players = new List<NetworkObject>();
    
    private void OnEnable()
    {
        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoadEnd;
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnPlayerConnectionState;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SetPlayerPos;
    }
    
    private void OnDisable()
    {
        if (InstanceFinder.SceneManager == null) return;
        
        InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnPlayerConnectionState;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SetPlayerPos;
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
        Scene targetScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("PersistentObjects");

        NetworkObject playerObj = Instantiate(_playerPrefab);

        if(targetScene.isLoaded)
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(playerObj.gameObject, targetScene);

        playerObj.transform.position =
            _spawnPointsByLevel[0].p_spawnPoints[Random.Range(0, _spawnPointsByLevel[0].p_spawnPoints.Length)].position;

        InstanceFinder.ServerManager.Spawn(playerObj, player);
        players.Add(playerObj);
        FPSController fps = playerObj.GetComponent<FPSController>();

        if (fps != null)
            SetUpLayerTargetRpc(player, fps);
    }
    
    [TargetRpc]
    private void SetUpLayerTargetRpc(NetworkConnection conn, FPSController fpsController)
    {
        fpsController.SetUpLayer();
    }

    private void SetPlayerPos(Scene scene, LoadSceneMode arg1)
    {
        foreach (NetworkObject player in players)
        {
            //player.transform.position = _spawnPointsByLevel[scene.buildIndex].p_spawnPoints[Random.Range(0, _spawnPointsByLevel[scene.buildIndex].p_spawnPoints.Length)].position;
            player.transform.position = new Vector3(-22.96f, 1.52f, -30.15f);            //je teste un truc

        }
    }
}

[Serializable]
public class LevelSpawnPoints
{
    public Transform[] p_spawnPoints;
}