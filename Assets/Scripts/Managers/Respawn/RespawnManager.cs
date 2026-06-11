using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Managers;
using UnityEngine;

public class RespawnManager : NetworkBusListener
{
    #region Variables

    private readonly HashSet<int> _deadPlayerIds = new HashSet<int>();
    private readonly SyncVar<bool> _isGameOver = new SyncVar<bool>(false);
    
    #endregion

    #region Fonctions

    public override void OnStartServer()
    {
        ListenToEvent<OnPlayerDeathEvent>(OnPlayerDied);
        ListenToEvent<OnPlayerRespawnEvent>(OnPlayerRespawned);
    }
    
    [Server]
    private void OnPlayerDied(OnPlayerDeathEvent data)
    {
        _deadPlayerIds.Add(data.p_playerN.ObjectId);

        int totalPlayers = ServerManager.Clients.Count;

        if (_deadPlayerIds.Count >= totalPlayers && totalPlayers > 0)
        {
            _isGameOver.Value = true;
            TriggerGameOverObserversRpc();
        }
    }

    [Server]
    private void OnPlayerRespawned(OnPlayerRespawnEvent data)
    {
        _deadPlayerIds.Remove(data.p_playerN.ObjectId);
    }

    [ObserversRpc]
    private void TriggerGameOverObserversRpc()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Quit()
    {
        Application.Quit();
    }

    #endregion
}