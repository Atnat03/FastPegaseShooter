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

    [Header("UI")]
    [SerializeField] private GameObject _playerUIEnd;

    #endregion

    #region Fonctions

    public override void OnStartServer()
    {
        ListenToEvent<OnPlayerDeathEvent>(OnPlayerDied);
        ListenToEvent<OnPlayerRespawnEvent>(OnPlayerRespawned);
    }

    public override void OnStartClient()
    {
        _playerUIEnd.SetActive(false);
        _isGameOver.OnChange += OnGameOverChange;
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

        if (_deadPlayerIds.Count == 0)
        {
            _isGameOver.Value = false;
        }
    }

    [ObserversRpc]
    private void TriggerGameOverObserversRpc()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _playerUIEnd.SetActive(true);
    }

    private void OnGameOverChange(bool prev, bool next, bool asServer)
    {
        if (!next)
        {
            _playerUIEnd.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    #endregion
}