using MyPrint;
using UnityEngine;

public class PlayerLocalData : MonoBehaviour
{
    public static PlayerLocalData Instance { get; private set; }

    public int LocalPlayerGunId { get; private set; } = 0;
    public string LocalPlayerName { get; private set; } = "";
    
    public int ExpectedPlayerCount { get; private set; } = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayerData(int gunId, string playerName, int expectedPlayerCount)
    {
        LocalPlayerGunId = gunId;
        LocalPlayerName = playerName;
        ExpectedPlayerCount = expectedPlayerCount;
        
        Cons.Print($"PlayerLocalData saved: Skin={gunId}, Name={playerName}", ColorConsole.Pink, ConsoleStyle.Bold);
    }

    public void ClearData()
    {
        LocalPlayerGunId = 0;
        LocalPlayerName = "";
    }
}