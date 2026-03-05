using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathfindingGridReader))]
public class GridReaderInspector : Editor
{
    public override void OnInspectorGUI()
    {
        PathfindingGridReader script = (PathfindingGridReader)target;
        
        GUILayout.Label($"Reader Id : {script.p_id}");
        DrawDefaultInspector();
    }
}
