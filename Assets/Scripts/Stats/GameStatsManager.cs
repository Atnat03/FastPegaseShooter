using System.Collections.Generic;
using UnityEngine;

public class GameStatsManager : NetworkBusListener
{
    public readonly Dictionary<int, PlayerStats> playerStats = new Dictionary<int, PlayerStats>();
    
    bool _initialized = false;

    public override void OnStartServer()
    {
        if (!_initialized)
        {
            _initialized = true;
        }
        
        ListenToEvent<AddHealthToPlayer>(RegisterHealthRegen);
        ListenToEvent<OnPlayerDeathEvent>(RegisterDeath);
    }

    void RegisterPlayer(int playerIndex) => playerStats.Add(playerIndex, new PlayerStats());
    
    void UnregisterPlayer(int playerIndex) => playerStats.Remove(playerIndex);
    
    void RegisterDamages(int playerIndex, float value, bool isCritical = false)
    {
        if (!playerStats.ContainsKey(playerIndex))RegisterPlayer(playerIndex);
        playerStats[playerIndex].p_damagesDealt +=  value;
        if (isCritical) playerStats[playerIndex].p_criticalDamagesDealt +=  value;
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
    public float p_damagesDealt;
    public float p_criticalDamagesDealt;
    public int p_killCount;
    public int p_deathCount;
    public float p_selfHealthRegen;
    public float p_broHealthRegen;

    public PlayerStats()
    {
        p_selfHealthRegen = 0;
        p_deathCount = 0;
        p_criticalDamagesDealt = 0;
        p_killCount = 0;
        p_deathCount = 0;
        p_selfHealthRegen = 0;
    }
}
