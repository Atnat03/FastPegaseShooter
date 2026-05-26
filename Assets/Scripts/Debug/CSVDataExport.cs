using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CSVDataExport : MonoBehaviour
{
    public bool autoOpenFile = true;
    private int gameID;
    private List<string[]> rows = new List<string[]>();
    
    void Start()
    {
        gameID = PlayerPrefs.GetInt("GameID", 0);
        
        rows.Add(new string[]
        {
            "Generation",
            "AvgFitness",
            "AvgFitnessNoFall",
            "MaxFitness",
            "FallenCount",
            "TrainingDuration",
            "MutationRate",
            "MutationPower",
        });
    }


    void AddLog(OnDataLog data)
    {
        
    }
    
    private void OnApplicationQuit()
    {
        ExportCSV();
        PlayerPrefs.SetInt("GameID", gameID++);
    }

    void ExportCSV()
    {
        string fileName = "game_" + gameID;

        string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string folderPath = Path.Combine(documentsPath);

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

        Debug.Log("CSV sauvegardé ici : " + path);

        if(autoOpenFile)Application.OpenURL("file://" + folderPath); 
    }
}

struct OnDataLog
{
    public string entityName;
    public string weapon;
    public string targetName;
    public float damages;
    public float player1PVs;
    public float player2PVs;
    public int ArenaID;
}
