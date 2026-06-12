using FishNet.Object;
using UnityEngine;

public class SceneEssentialSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject sceneEssentialPrefab;
    private static bool _spawned = false;

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        if (_spawned) return;
        _spawned = true;

        NetworkObject instance = Instantiate(sceneEssentialPrefab);
        ServerManager.Spawn(instance);
    }
}