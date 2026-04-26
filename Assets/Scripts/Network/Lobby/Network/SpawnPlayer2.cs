using System.Collections.Generic;
using Controller;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing.Server;
using FishNet.Transporting;
using UnityEngine;

public class SpawnPlayer2 : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private readonly HashSet<int> spawnedClients = new();

    public override void OnStartServer()
    {
        base.OnStartServer();
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnClientState;
        Invoke(nameof(SpawnHost), 0.1f);
    }

    private void SpawnHost()
    {
        foreach (var kvp in InstanceFinder.ServerManager.Clients)
        {
            int clientId = kvp.Key;
            if (!spawnedClients.Contains(clientId))
            {
                SpawnPlayer(clientId);
                spawnedClients.Add(clientId);
            }
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (InstanceFinder.ServerManager != null)
            InstanceFinder.ServerManager.OnRemoteConnectionState -= OnClientState;
    }

    private void OnClientState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            int clientId = conn.ClientId;
            if (!spawnedClients.Contains(clientId))
            {
                SpawnPlayer(clientId);
                spawnedClients.Add(clientId);
            }
        }
    }

    [Server]
    private void SpawnPlayer(int clientId)
    {
        if (!InstanceFinder.ServerManager.Clients.ContainsKey(clientId))
        {
            Debug.LogWarning($"Client {clientId} not found");
            return;
        }

        Transform spawn = spawnPoints[Mathf.Abs(clientId) % spawnPoints.Length];
        GameObject obj = Instantiate(playerPrefab, spawn.position, spawn.rotation);
        InstanceFinder.ServerManager.Spawn(obj, InstanceFinder.ServerManager.Clients[clientId]);

        // ✅ Plus rien à faire ici — PlayerSetup.OnStartClient() s'en charge
        Debug.Log($"Spawned player for client {clientId}");
    }
}