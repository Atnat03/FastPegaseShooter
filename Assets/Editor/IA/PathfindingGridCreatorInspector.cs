using System.Collections.Generic;
using CustomConsole.Runtime.Logger;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathfindingGridCreator))]
public class PathfindingGridCreatorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        PathfindingGridCreator script = target as PathfindingGridCreator;
        base.OnInspectorGUI();
        
        GUILayout.Label("Debug");
        Color backgroundColor = GUI.color;
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = script.drawBounds ? Color.green : Color.red;
        if(GUILayout.Button("Draw Bounds")) script.drawBounds = !script.drawBounds;
        GUI.backgroundColor = script.drawBoundingBox ? Color.green : Color.red;
        if(GUILayout.Button("Draw Boundind Box")) script.drawBoundingBox = !script.drawBoundingBox;
        GUILayout.EndHorizontal();
        GUI.backgroundColor = script.drawObstacles ? Color.green : Color.red;
        if(GUILayout.Button("Draw Obstacles")) script.drawObstacles = !script.drawObstacles;
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = script.drawNodes ? Color.green : Color.red;
        if(GUILayout.Button("Draw Nodes")) script.drawNodes = !script.drawNodes;
        GUI.backgroundColor = script.drawNodesConnections ? Color.green : Color.red;
        if(GUILayout.Button("Draw Connections")) script.drawNodesConnections = !script.drawNodesConnections;
        GUILayout.EndHorizontal();
        
        GUI.backgroundColor = backgroundColor;

        if (GUILayout.Button("Save Grid"))
        {
            string path = EditorUtility.SaveFilePanel(
                "Save Grid",
                "Assets/ScriptableObjects/Pathfinding Grids",
                "MyGrid",
                "asset");
            
            if(string.IsNullOrEmpty(path)) return;
            if (path.StartsWith(Application.dataPath))
            {
                string relativePath = $"Assets{path.Substring(Application.dataPath.Length)}";

                PathfindingGridSO asset = ScriptableObject.CreateInstance<PathfindingGridSO>();
                
                AssetDatabase.CreateAsset(asset, relativePath);
                asset.nodes = new List<PathfindingNode>();
                foreach (PathfindingNode node in script.nodes)
                {
                    PathfindingNode newNode = new PathfindingNode(node.index, node.gridPosition, node.position, node.wallAvoidance);
                    newNode.neighborsIndex = new List<int>(node.neighborsIndex);
                    asset.nodes.Add(newNode);
                }
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
                CustomLogger.ImportantLog($"Created asset at : {relativePath}");
            }
        }
    }
}
