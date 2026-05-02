using CustomConsole.Runtime.Logger;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathfindingReaderVisualizer))]
public class PathfindingGridReaderVisualizerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        PathfindingReaderVisualizer script = (PathfindingReaderVisualizer)target;
        
        Color backgroundColor = GUI.color;
        GUILayout.Label("Debug");
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = script.drawNodes ? Color.green : Color.red;
        if(GUILayout.Button("Draw Nodes")) script.drawNodes = !script.drawNodes;
        GUI.backgroundColor = script.drawNodesConnections ? Color.green : Color.red;
        if(GUILayout.Button("Draw Connections")) script.drawNodesConnections = !script.drawNodesConnections;
        GUILayout.EndHorizontal();
        GUI.backgroundColor = backgroundColor;
        
        Undo.RecordObject(script, "Modify Pathfinding Debug");
        EditorGUI.BeginChangeCheck();

        script.walkingCostGradient = EditorGUILayout.GradientField("walkingCostGradient", script.walkingCostGradient);
        script.upperWalkingCostValue = EditorGUILayout.IntField("upperWalkingCostValue", script.upperWalkingCostValue);
        script.nodeDrawSize = EditorGUILayout.FloatField("nodeDrawSize", script.nodeDrawSize);

        if (EditorGUI.EndChangeCheck())
        {
            script.needToRebuildMatrix = true;
            EditorUtility.SetDirty(script);
        }
    }
}
