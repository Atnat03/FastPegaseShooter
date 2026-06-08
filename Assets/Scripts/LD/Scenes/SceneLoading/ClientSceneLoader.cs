using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientSceneLoader : MonoBusListener
{
    [Header("Scene to load")]
    [SerializeField] private string targetSceneName = "MyScene";

    
    public void LoadSceneForAll()
    {
        if (!InstanceFinder.IsServerStarted) return;

        NetworkObject[] playerObjects = GetAllPlayerNetworkObjects();
        
        SceneLoadData sld = new SceneLoadData(targetSceneName)
        {
            ReplaceScenes = ReplaceOption.All, // Garde l'ancienne scène active
            Options = new LoadOptions
            {
                AllowStacking = false,
                AutomaticallyUnload = true,
            },
            MovedNetworkObjects = GetAllPlayerNetworkObjects() // Déplace les joueurs
        };

        InvokeEvent(new ForceStopEnemySpawn());
        
        DespawnAllNetworkObject(playerObjects);
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }

    private NetworkObject[] GetAllPlayerNetworkObjects()
    {
        Dictionary<int, NetworkConnection> clients = InstanceFinder.ServerManager.Clients;
        List<NetworkObject> nobs = new List<NetworkObject>();

        foreach (var client in clients.Values)
        {
            if (client.FirstObject == null)
                continue;
            
            nobs.Add(client.FirstObject);
        }

        return nobs.ToArray();
    }

    void DespawnAllNetworkObject(NetworkObject[] objectsToKeep)
    {
        List<NetworkObject> SceneObjects = new();

        foreach (var kvp in InstanceFinder.ServerManager.Objects.Spawned)
        {
            NetworkObject nob = kvp.Value;
            
            if(nob == null || !nob.IsSpawned || objectsToKeep.Contains(nob) || !nob.IsSceneObject)
                continue;
            
            SceneObjects.Add(nob);
        }

        foreach (NetworkObject nob in SceneObjects)
            InstanceFinder.ServerManager.Despawn(nob);
    }
}

public struct ForceStopEnemySpawn
{
    
}
