using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StatsPanelBehaviour : NetworkBusListener
{
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] GameStatsManager _gameStatsManager;
    [SerializeField] List<PlayerStatsDisplay> playerStatsDisplayList = new();
    readonly Dictionary<int, PlayerStatsDisplay> playerStatsDisplayDict = new Dictionary<int, PlayerStatsDisplay>();
    [SerializeField] GameObject _statsPanel;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        _gameStatsManager.onRegisterPlayer += RegisterPlayer;
        InvokeEvent(new OnPlayerRegisterEvent{ownerId = OwnerId});
        
        RefreshData();
        ClosePanel();
    }

    private void OnEnable()
    {
        InputAction action = _playerInput.actions["StatsPanel"];
    
        action.started += OpenPanel;  
        action.canceled += ClosePanel;  
    }

    private void OnDisable()
    {
        InputAction action = _playerInput.actions["StatsPanel"];
    
        action.started -= OpenPanel;
        action.canceled -= ClosePanel;
    }

    void RegisterPlayer(int playerIndex)
    {
        
        playerStatsDisplayDict.Add(playerIndex, playerIndex == OwnerId ? playerStatsDisplayList[0] : playerStatsDisplayList[1]);
    } 

    void OpenPanel(InputAction.CallbackContext context) => OpenPanel();
    void OpenPanel()
    {
        RefreshData();
        _statsPanel.SetActive(true);
    }

    private void RefreshData()
    {
        foreach (var playerData in _gameStatsManager.playerStats)
        {
            if (playerStatsDisplayDict.TryGetValue(playerData.Key, out var display))
            {
                var stats = playerData.Value;

                display.p_damagesDealtText.text = "Damages dealt: " + stats.p_damagesDealt;
                display.p_criticalDamagesDealtText.text = "Crit Damages dealt: " + stats.p_criticalDamagesDealt;
                display.p_killCountText.text = "Kills: " + stats.p_killCount;
                display.p_deathCountText.text = "Deaths: " + stats.p_deathCount;
                display.p_selfHealthRegenText.text = "Self Heal: " + stats.p_selfHealthRegen;
                display.p_broHealthRegenText.text = "Ally Heal: " + stats.p_broHealthRegen;
            }
        }
    }

    void ClosePanel(InputAction.CallbackContext context) => ClosePanel();
    void ClosePanel() => _statsPanel.SetActive(false);
    
    
}

[Serializable]
public class PlayerStatsDisplay
{
    public TextMeshProUGUI p_damagesDealtText;
    public TextMeshProUGUI p_criticalDamagesDealtText;
    public TextMeshProUGUI p_killCountText;
    public TextMeshProUGUI p_deathCountText;
    public TextMeshProUGUI p_selfHealthRegenText;
    public TextMeshProUGUI p_broHealthRegenText;
}
