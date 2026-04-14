using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class NetworkSceneLoader : NetworkBehaviour
{
    public void LoadGameScene(string sceneName)
    {
        if (!IsServerInitialized) return;

        SceneLoadData sld = new SceneLoadData(sceneName);
        sld.ReplaceScenes = ReplaceOption.All;
        sld.Options.AllowStacking = false;
        
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        
        Debug.Log($"Server loading scene: {sceneName} for all clients");
    }
}