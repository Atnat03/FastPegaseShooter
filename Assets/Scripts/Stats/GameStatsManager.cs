using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStatsManager : NetworkBusListener
{
    public readonly Dictionary<int, PlayerStats> playerStats = new Dictionary<int, PlayerStats>();
    bool _initialized = false;

    public Action<int> onRegisterPlayer;

    public override void OnStartClient()
    {
        if (!_initialized)
        {
            _initialized = true;
        }
        
        ListenToEvent<AddHealthToPlayer>(RegisterHealthRegen);
        ListenToEvent<OnPlayerDeathEvent>(RegisterDeath);
        ListenToEvent<OnPlayerDoDamage>(RegisterDamages);
    }

    public void RegisterPlayer(int playerIndex)
    {
        playerStats.Add(playerIndex, new PlayerStats());
        onRegisterPlayer?.Invoke(playerIndex);
    }
    
    void UnregisterPlayer(int playerIndex) => playerStats.Remove(playerIndex);
    
    void RegisterDamages(OnPlayerDoDamage data)
    {
        if (!playerStats.ContainsKey(data.playerID))RegisterPlayer(data.playerID);
        playerStats[data.playerID].p_damagesDealt +=  data.damageAmount;
        if (data.isCritical) playerStats[data.playerID].p_criticalDamagesDealt +=  data.damageAmount;
    }

    void RegisterKill(int playerIndex)
    {
        if (!playerStats.ContainsKey(playerIndex))RegisterPlayer(playerIndex);
        playerStats[playerIndex].p_killCount++;
    } 

    void RegisterDeath(OnPlayerDeathEvent data) => RegisterDeath(data.p_playerN.OwnerId);
    void RegisterDeath(int playerIndex)
    {
        if (!playerStats.ContainsKey(playerIndex))RegisterPlayer(playerIndex);
        playerStats[playerIndex].p_deathCount++;
        Debug.Log("appel on death count : " + playerStats[playerIndex].p_deathCount);
    } 

    void RegisterHealthRegen(AddHealthToPlayer data) => RegisterHealthRegen(data.p_playerId, data.p_value);
    void RegisterHealthRegen(int playerIndex, float value)
    {
        if (!playerStats.ContainsKey(playerIndex))RegisterPlayer(playerIndex);
        if(playerIndex == OwnerId)playerStats[playerIndex].p_selfHealthRegen +=  value;
        else playerStats[playerIndex].p_broHealthRegen +=  value;
    }

}

public class PlayerStats
{
    public float p_damagesDealt = 0;
    public float p_criticalDamagesDealt = 0;
    public int p_killCount = 0;
    public int p_deathCount = 0;
    public float p_selfHealthRegen = 0;
    public float p_broHealthRegen = 0;
}
