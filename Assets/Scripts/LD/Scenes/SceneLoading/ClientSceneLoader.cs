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

public class ClientSceneLoader : MonoBehaviour
{
    [Header("Scene to load")]
    [SerializeField] private string targetSceneName = "MyScene";

    [Header("Options")]
    // Si true, les joueurs sont déplacés dans la nouvelle scène
    // Si false, ils restent aussi dans l'ancienne (multi-scene)
    [SerializeField] private bool movePLayersToNewScene = true;

    /// <summary>
    /// Appelle cette méthode depuis ton trigger (sur le serveur uniquement)
    /// </summary>
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

        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }

    private NetworkObject[] GetAllPlayerNetworkObjects()
    {
        if (!movePLayersToNewScene) return Array.Empty<NetworkObject>();

        // Récupère tous les NetworkObjects des clients connectés
        var clients = InstanceFinder.ServerManager.Clients;
        var nobs = new List<NetworkObject>();

        foreach (var client in clients.Values)
        {
            if (client.FirstObject != null)
                nobs.Add(client.FirstObject);
        }

        return nobs.ToArray();
    }
    
    void OnEnable()
    {
        InstanceFinder.SceneManager.OnLoadStart += OnLoadStart;
        InstanceFinder.SceneManager.OnLoadEnd += OnLoadEnd;
    }

    void OnDisable()
    {
        InstanceFinder.SceneManager.OnLoadStart -= OnLoadStart;
        InstanceFinder.SceneManager.OnLoadEnd -= OnLoadEnd;
    }

    private void OnLoadStart(SceneLoadStartEventArgs args)
    {
        CustomLogger.ImportantLog($"[SceneLoader] Début chargement");
    }

    private void OnLoadEnd(SceneLoadEndEventArgs args)
    {
        CustomLogger.ImportantLog($"[SceneLoader] Fin chargement. Scènes chargées : {args.LoadedScenes.Length}, Skipped : {args.SkippedSceneNames.Length}");
        foreach (var s in args.SkippedSceneNames)
            CustomLogger.ImportantLog($"[SceneLoader] Scène skippée : {s}");
    }
}
