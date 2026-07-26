using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public struct OnShowLoadingScreen { }
public struct OnHideLoadingScreen { }

public class ClientSceneLoader : NetworkBusListener
{
    [Header("Scene to load")]
    [SerializeField] private string targetSceneName = "MyScene";
    
    public void LoadSceneForAll()
    {
        if (!InstanceFinder.IsServerStarted) return;

        StartLoadingUIObserverRpc();
        
        NetworkObject[] playerObjects = GetAllPlayerNetworkObjects();
        
        SceneLoadData sld = new SceneLoadData(targetSceneName)
        {
            ReplaceScenes = ReplaceOption.All,
            Options = new LoadOptions
            {
                AllowStacking = false,
                AutomaticallyUnload = true,
            },
            MovedNetworkObjects = GetAllPlayerNetworkObjects() 
        };

        NotifySceneLoadingObserversRpc();
        InvokeEvent(new OnSceneLoadingEvent());

        DespawnAllNetworkObject(playerObjects);
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }

    [ObserversRpc]
    private void StartLoadingUIObserverRpc()
    {
        InvokeEvent(new OnShowLoadingScreen());
    }
    
    [ObserversRpc]
    private void NotifySceneLoadingObserversRpc() 
    {
        InvokeEvent(new OnSceneLoadingEvent());
    }
    
    private NetworkObject[] GetAllPlayerNetworkObjects()
    {
        Dictionary<int, NetworkConnection> clients = InstanceFinder.ServerManager.Clients;
        List<NetworkObject> nobs = new List<NetworkObject>();

        foreach (NetworkConnection client in clients.Values)
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

public struct OnSceneLoadingEvent
{
    
}
