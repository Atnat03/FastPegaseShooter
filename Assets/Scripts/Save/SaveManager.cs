using System;
using System.IO;
using UnityEngine;
using UnityEngine.Windows;
using File = System.IO.File;

public static class SaveManager
{
    public const string PNG_FOLDER = "PNG/";
    #if UNITY_EDITOR
    public static readonly string SAVE_PATH = Application.dataPath + "/Saves/";
    #else
    public static readonly string SAVE_PATH = Application.persistentDataPath + "/Rubikards_Saves/";
    #endif
    private static bool initialized = false;
    
    public static void Init()
    {
        initialized  = true;
        if (!System.IO.Directory.Exists(SAVE_PATH))
        {
            System.IO.Directory.CreateDirectory(SAVE_PATH);
        }
        if (!System.IO.Directory.Exists($"{SAVE_PATH}{PNG_FOLDER}"))
        {
            System.IO.Directory.CreateDirectory($"{SAVE_PATH}{PNG_FOLDER}");
        }
    }

    public static void Save<T>(T objectToSave, string fillPath = null) where T : ISavable<T>
    {
        if(!initialized) Init();
        
        string savePath = fillPath ?? $"{SAVE_PATH}{typeof(T).Name}.json";
        
        #if UNITY_EDITOR
        string saveString = JsonUtility.ToJson(objectToSave, true);
        #else
        string saveString = JsonUtility.ToJson(objectToSave, false);
        #endif
        
        System.IO.File.WriteAllText(savePath, saveString);
    }

    public static T Load<T>(string fillPath = null) where T : class, ISavable<T>, new() 
    {
        if(!initialized) Init();

        string savePath = fillPath ?? $"{SAVE_PATH}{typeof(T).Name}.json";
        
        try
        {
            string json = System.IO.File.ReadAllText(savePath);
            T loadedObject = JsonUtility.FromJson<T>(json);
            return loadedObject;
        }
        catch
        {
            Debug.LogWarning($"Save Manager failed to load at {savePath}, but default value mode is enabled");
            return new T();
        }
    }

    public static string SaveTextureToPNGFormat(Texture2D textureToSave, string fileName)
    {
        if(!initialized) Init();
        
        byte[] bytes = textureToSave.EncodeToPNG();
        string savePath = $"{SAVE_PATH}{PNG_FOLDER}";
        string filename = $"{fileName}_{System.IO.Directory.GetFiles(savePath, "*.png").Length}.png";
        
        #if UNITY_EDITOR
        string path = AbsoluteToRelativePath(Path.Combine(savePath, filename));
        #else
        string path = System.IO.Path.Combine(savePath, filename);
        #endif
        
        System.IO.File.WriteAllBytes(path, bytes);
        return path;
    }

    public static bool TryLoadTextureToPNGFormat(string filePath, out Texture2D loadedTexture)
    {
        string path = filePath.StartsWith("Assets") ? RelativeToAbsolutePath(filePath) : filePath;
        if(!File.Exists(path))
        {
            loadedTexture = null;
            return false;
        }
        
        byte[] bytes = System.IO.File.ReadAllBytes(path);
        loadedTexture = new Texture2D(2, 2);
        loadedTexture.LoadImage(bytes);
        return true;
    }

    public static string AbsoluteToRelativePath(string absolutePath)
    {
        string applicationPath = Application.dataPath;
        if (absolutePath.StartsWith(applicationPath))
            return $"Assets{absolutePath.Substring(applicationPath.Length)}";
        return absolutePath;
    }

    public static string RelativeToAbsolutePath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return null;
        if(relativePath.StartsWith("Assets/"))
        {
            return Path.Combine(Application.dataPath, relativePath.Substring("Assets/".Length));
        }
        return relativePath;
    }
}
