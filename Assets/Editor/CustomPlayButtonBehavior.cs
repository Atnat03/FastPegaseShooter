using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class CustomPlayButtonBehavior
{
    static CustomPlayButtonBehavior()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChange;
    }

    private static void OnPlayModeChange(PlayModeStateChange change)
    {
        switch (change)
        {
            case PlayModeStateChange.EnteredEditMode:
                break;
            case PlayModeStateChange.ExitingEditMode:
                break;
            case PlayModeStateChange.EnteredPlayMode:
                SceneManager.LoadScene(0);
                break;
            case PlayModeStateChange.ExitingPlayMode:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, null);
        }
    }
}
