using System;
using System.Collections;
using System.Collections.Generic;
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
        
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }

    private NetworkObject[] GetAllPlayerNetworkObjects()
    {
        Dictionary<int, NetworkConnection> clients = InstanceFinder.ServerManager.Clients;
        List<NetworkObject> nobs = new List<NetworkObject>();

        foreach (var client in clients.Values)
        {
            if (client.FirstObject != null)
                nobs.Add(client.FirstObject);
        }

        return nobs.ToArray();
    }
}

public struct ForceStopEnemySpawn
{
    
}
