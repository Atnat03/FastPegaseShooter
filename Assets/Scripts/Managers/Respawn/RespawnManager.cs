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
        ListenToEvent<OnPlayerRespawnEvent>(OnPlayerRespawned);
    }

    [Server]
    private void OnPlayerRespawned(OnPlayerRespawnEvent data)
    {
        _deadPlayerIds.Remove(data.p_playerN.ObjectId);
    }

    public void Quit()
    {
        Application.Quit();
    }

    #endregion
}