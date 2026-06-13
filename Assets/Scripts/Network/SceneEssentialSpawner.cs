using FishNet.Object;
using MyPrint;
using UnityEngine;

public struct OnSpawnEssential
{
    public NetworkObject obj;
}

public class SceneEssentialSpawner : NetworkBusListener
{
    [SerializeField] private NetworkObject sceneEssentialPrefab;
    private static bool _spawned = false;

    public static NetworkObject EssentialObject { get; private set; }
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        if (_spawned) return;
        _spawned = true;

        NetworkObject instance = Instantiate(sceneEssentialPrefab);
        ServerManager.Spawn(instance);
        
        Cons.Print("Spawn Essential", ColorConsole.Orange);
        
        EssentialObject = instance;
    }
}