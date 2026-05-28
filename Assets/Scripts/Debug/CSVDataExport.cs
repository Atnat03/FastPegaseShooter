using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CSVDataExport : MonoBusListener
{
    public bool autoOpenFile = true;
    private int gameID;
    private List<string[]> rows = new List<string[]>();
    
    void Start()
    {
        gameID = PlayerPrefs.GetInt("GameID", 0);
        
        rows.Add(new string[]
        {
            "Time",
            "EntityType",
            "EntityID",
            "weapon",
            "target",
            "damages",
            "player1PVs",
            "player2PVs",
            "player1Energy",
            "player2Energy",
            "subArenaID",
        });
        
        ListenToEvent<OnDataLog>(AddLog);
    }


    void AddLog(OnDataLog data)
    {
        rows.Add(new []
        {
            Time.time.ToString("F2"),
            data.entityName,
            data.EntityID.ToString(),
            data.weapon,
            data.targetName,
            data.damages.ToString("F2"),
            data.player1PVs.ToString("F2"),
            data.player2PVs.ToString("F2"),
            data.player1Energy.ToString("F2"),
            data.player2Energy.ToString("F2"),
            data.ArenaID.ToString(),
        });
    }
    
    private void OnApplicationQuit()
    {
        if(rows.Count < 2) return;
        ExportCSV();
        PlayerPrefs.SetInt("GameID", ++gameID);
    }

    void ExportCSV()
    {
        string fileName = "game_" + gameID;

        string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string folderPath = Path.Combine(documentsPath, "GamesData");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string path = Path.Combine(folderPath, fileName + ".csv");

        StringBuilder sb = new StringBuilder();

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(";", row));
        }

        File.WriteAllText(path, sb.ToString());

        if(autoOpenFile)Application.OpenURL("file://" + folderPath); 
    }
    
    [ContextMenu("resetGameCount")]
    public void ResetGameCount()
    {
        PlayerPrefs.SetInt("GameID", 0);
    }
}


struct OnDataLog
{
    public string entityName;
    public int EntityID;
    public string weapon;
    public string targetName;
    public float damages;
    public float player1PVs;
    public float player2PVs;
    public float player1Energy;
    public float player2Energy;
    public int ArenaID;
}
