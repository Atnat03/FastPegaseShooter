using CustomConsole.Runtime.Logger;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathfindingGridReader))]
public class PathfindingGridReaderInspector : Editor
{
    public override void OnInspectorGUI()
    {
        PathfindingGridReader script = (PathfindingGridReader)target;
        
        GUILayout.Label($"Reader Id : {script.p_id}");
        DrawDefaultInspector();
        
        Color backgroundColor = GUI.color;
        GUILayout.Label("Debug");
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = script.drawNodes ? Color.green : Color.red;
        if(GUILayout.Button("Draw Nodes")) script.drawNodes = !script.drawNodes;
        GUI.backgroundColor = script.drawNodesConnections ? Color.green : Color.red;
        if(GUILayout.Button("Draw Connections")) script.drawNodesConnections = !script.drawNodesConnections;
        GUILayout.EndHorizontal();
        
        GUI.backgroundColor = backgroundColor;
    }
}
