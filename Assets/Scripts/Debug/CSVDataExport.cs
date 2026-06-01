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
            "targetID",
            "damages",
            "player1PVs",
            "player2PVs",
            "player1Energy",
            "player2Energy",
            "subArenaID",
        });
        
        ListenToEvent<OnDataLog>(AddLog);
        
        AddLog(
            new OnDataLog()
            {
                entityName = "init",
                EntityID = -1,
                weapon = "init",
                targetName = "init",
                targetID = -1,
                damages = 0,
                player1PVs = 100,
                player2PVs = 100,
                player1Energy = 100,
                player2Energy = 100,
                ArenaID = -1,
            });
    }


    void AddLog(OnDataLog data)
    {
        string[] prev = rows.Count > 1 ? rows[^1] : new string[12];

        rows.Add(new[]
        {
            Time.time.ToString("F2"),                                         
            data.entityName   ?? prev[1],                                    
            data.EntityID     .HasValue ? data.EntityID.Value.ToString()     : prev[2],
            data.weapon       ?? prev[3],                                 
            data.targetName   ?? prev[4],
            data.targetID     .HasValue ? data.targetID.Value.ToString()    : prev[5],
            data.damages      .HasValue ? data.damages.Value.ToString("F2")  : prev[6], 
            data.player1PVs   .HasValue ? data.player1PVs.Value.ToString("F2") : prev[7],
            data.player2PVs   .HasValue ? data.player2PVs.Value.ToString("F2") : prev[8],
            data.player1Energy.HasValue ? data.player1Energy.Value.ToString("F2") : prev[9], 
            data.player2Energy.HasValue ? data.player2Energy.Value.ToString("F2") : prev[10], 
            data.ArenaID      .HasValue ? data.ArenaID.Value.ToString()      : prev[11],
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
    public int?   EntityID;
    public string weapon;
    public string targetName;
    public int?   targetID;
    public float? damages;
    public float? player1PVs;
    public float? player2PVs;
    public float? player1Energy;
    public float? player2Energy;
    public int?   ArenaID;
}
