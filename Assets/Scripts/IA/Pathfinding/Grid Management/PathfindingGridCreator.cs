using System;
using System.Collections.Generic;
using CustomConsole.Runtime.Logger;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class PathfindingGridCreator : EditorWindow
{
    
    [Header("Bounding box")]
    public Vector3 boundsOffset =  Vector3.zero;
    [SerializeField] private float boundsHeight = 0.5f;
    [SerializeField] private List<Vector2> boundsVertices = new List<Vector2>();
    
    [Header("Grid Generation Parameters")]
    [SerializeField] private float detectionPrecision = 0.3f;
    [SerializeField] private float maxVerticalDistance = 0.3f;
    [SerializeField] private float agentHeight = 0.5f;
    
    [Header("Node Parameters")]
    [SerializeField] private int wallAvoidanceDistance = 3;
    
    [Header("Debug")]
    [SerializeField] private float nodeSize = 0.025f;
    [SerializeField] private Gradient wallAvoidanceGradient = new Gradient();

    [HideInInspector] public List<PathfindingNode> nodes = new List<PathfindingNode>();

    private int xRaycastAmount, zRaycastAmount;
    
    //Debug Variables
    List<Vector3> obstaclesDebug =  new List<Vector3>();
    [HideInInspector] public bool drawBounds = true;
    [HideInInspector] public bool drawBoundingBox = false;
    [HideInInspector] public bool drawObstacles = false;
    [HideInInspector] public bool drawNodes = false;
    [HideInInspector] public bool drawNodesConnections = true;
    //

    [MenuItem("Tools/Pathfinding Grid Creator")]
    public static void ShowWindow()
    {
        PathfindingGridCreator window = GetWindow<PathfindingGridCreator>();
        
        window.titleContent = new GUIContent("Pathfinding Grid Creator");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }


    public void CreateGUI()
    {
        Debug.Log("CreateGUI");
    }

    public void OnGUI()
    {
        DrawGUI();
        if(boundsVertices.Count < 3) return;
        
        InitializeNodes();
    }
    private void OnSceneGUI(SceneView obj)
    {
        DrawGizmos();
    }

    void DrawGUI()
    {
        GUIStyle titleStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            fontSize = 18
        };
        titleStyle.normal.textColor = Color.white;
        
        GUILayout.Label("Bounding Box", titleStyle);
        boundsHeight = EditorGUILayout.FloatField("Bounds Height", boundsHeight);
        boundsOffset = EditorGUILayout.Vector3Field("Bounds Offset", boundsOffset);
        DrawList(boundsVertices);
        GUILayout.Label("Grid Generation Parameters", titleStyle);
        detectionPrecision = EditorGUILayout.FloatField("Detection Precision", detectionPrecision);
        maxVerticalDistance = EditorGUILayout.FloatField("Max Vertical Distance", maxVerticalDistance);
        agentHeight = EditorGUILayout.FloatField("Agent Height", agentHeight);
        
        GUILayout.Label("Node Parameters", titleStyle);
        wallAvoidanceDistance = EditorGUILayout.IntField("Wall Avoidance Distance", wallAvoidanceDistance);
        
        GUILayout.Label("Debug", titleStyle);
        nodeSize = EditorGUILayout.FloatField("Node Size", nodeSize);
        wallAvoidanceGradient = EditorGUILayout.GradientField("Wall Avoidance Gradient", wallAvoidanceGradient);
        
        
        GUILayout.Label("Debug");
        Color backgroundColor = GUI.color;
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = drawBounds ? Color.green : Color.red;
        if(GUILayout.Button("Draw Bounds")) drawBounds = !drawBounds;
        GUI.backgroundColor = drawBoundingBox ? Color.green : Color.red;
        if(GUILayout.Button("Draw Boundind Box")) drawBoundingBox = !drawBoundingBox;
        GUILayout.EndHorizontal();
        GUI.backgroundColor = drawObstacles ? Color.green : Color.red;
        if(GUILayout.Button("Draw Obstacles")) drawObstacles = !drawObstacles;
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = drawNodes ? Color.green : Color.red;
        if(GUILayout.Button("Draw Nodes")) drawNodes = !drawNodes;
        GUI.backgroundColor = drawNodesConnections ? Color.green : Color.red;
        if(GUILayout.Button("Draw Connections")) drawNodesConnections = !drawNodesConnections;
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
                foreach (PathfindingNode node in nodes)
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
    
    void DrawList<T>(List<T> list)
    {
        Color oldColor = GUI.color;
        Color red = new Color(0.8f, 0.3f, 0.3f);
        Color green = new Color(0.3f, 0.8f, 0.3f);
        
        GUILayout.BeginVertical("box");
        for (int i = list.Count-1; i >= 0; i--)
        {
            object value = list[i];
            EditorGUILayout.BeginHorizontal();
            if(typeof(T) == typeof(Vector2))
            {
                value = EditorGUILayout.Vector2Field("", (Vector2)value);
            }
            else if (typeof(Object).IsAssignableFrom(typeof(T)))
            {
                value = EditorGUILayout.ObjectField("", (Object)value, typeof(T), true);
            }

            GUI.color = red;
            if (GUILayout.Button("-",  GUILayout.Width(50)))
            {
                list.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            GUI.color = oldColor;
            EditorGUILayout.EndHorizontal();

            list[i] = (T)value;
        }
        GUILayout.BeginHorizontal();
        
        GUI.color = green;
        if (GUILayout.Button("+"))
        {
            list.Add(default(T));
        }
        GUI.color = red;
        if (GUILayout.Button("-"))
        {
            list.RemoveAt(list.Count - 1);
        }
        GUI.color = oldColor;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    Vector3[] GetRaycastPositions()
    {
        (Vector2,Vector2) minMaxBoundingBox = GetBoundsBoxMinMax(); 
        Vector2 boundingBoxExtent = minMaxBoundingBox.Item2 - minMaxBoundingBox.Item1;
        Vector2 boundingBoxCenter = GetBoundingBoxCenter(minMaxBoundingBox.Item1, minMaxBoundingBox.Item2);
        
        xRaycastAmount = (int)(boundingBoxExtent.x / detectionPrecision);
        zRaycastAmount = (int)(boundingBoxExtent.y / detectionPrecision);
        
        float xStartOffset = (boundingBoxExtent.x - detectionPrecision * (xRaycastAmount-1))*0.5f + boundingBoxCenter.x;
        float zStartOffset = (boundingBoxExtent.y - detectionPrecision * (zRaycastAmount-1))*0.5f + boundingBoxCenter.y;
        
        Vector3 offset = boundsOffset + new Vector3(-boundingBoxExtent.x*0.5f+xStartOffset, boundsHeight*0.5f, -boundingBoxExtent.y*0.5f+zStartOffset);
        Vector3[] raycastPositions = new Vector3[xRaycastAmount * zRaycastAmount];
        for (int i = 0; i < xRaycastAmount; i++)
        {
            for (int j = 0; j < zRaycastAmount; j++)
            {
                raycastPositions[i * zRaycastAmount + j] = offset + new Vector3(i * detectionPrecision, 0, j * detectionPrecision);
            }
        }
        
        return raycastPositions;
    }

    (Vector2,Vector2) GetBoundsBoxMinMax()
    {
        Vector2 min = new Vector3(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector3(float.MinValue, float.MinValue);
        foreach (Vector3 v in boundsVertices)
        {
            if(v.x < min.x) min.x = v.x;
            if(v.y < min.y) min.y = v.y;
            
            if (v.x > max.x) max.x = v.x;
            if (v.y > max.y) max.y = v.y;
        }
        return new (min, max);
    }
    Vector2 GetBoundingBoxCenter(Vector2 min, Vector2 max) => min + (max - min)*0.5f;
    bool IsInsideBound(Vector2 pos)
    {
        int edgeCross = 0;

        for (int i = 0; i < boundsVertices.Count; i++)
        {
            Vector2 p1 = boundsVertices[i];
            Vector2 p2 = boundsVertices[(i + 1) % boundsVertices.Count];

            if ((p1.y > pos.y) != (p2.y > pos.y))
            {
                float x0 = p1.x + (pos.y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y);

                if (pos.x < x0)
                    edgeCross++;
            }
        }

        return edgeCross % 2 != 0;
    }

    void InitializeNodes()
    {
        nodes.Clear();
        obstaclesDebug.Clear();
        
        Vector3[] raycastPositions = GetRaycastPositions();
        Dictionary<int, List<PathfindingNode>> nodesPerCell = new();
        int currentNodeIndex = 0;
        
        for(int i = 0; i < raycastPositions.Length; i++)
        {
            RaycastHit[] hits = Physics.RaycastAll(raycastPositions[i], Vector3.down, boundsHeight);

            nodesPerCell[i] = new List<PathfindingNode>();
            
            List<(float,float)> obstacles = new List<(float,float)>();
            for (int j = hits.Length-1; j >= 0; j--)
            {
                if (hits[j].collider.gameObject.layer == 8 &&
                    hits[j].collider.gameObject.CompareTag("PathFindingObstacle"))
                {
                    float lenght = hits[j].collider.bounds.max.y - hits[j].collider.bounds.min.y;
                    RaycastHit[] backHits = Physics.RaycastAll(
                        new Vector3(raycastPositions[i].x,hits[j].point.y-lenght-0.1f,raycastPositions[i].z),
                        Vector3.up,
                        lenght);
                    foreach (RaycastHit hit in backHits)
                    {
                        if (hit.collider == hits[j].collider)
                        {
                            obstaclesDebug.Add(hits[j].point);
                            obstaclesDebug.Add(hit.point);
                            obstacles.Add((hit.point.y, hits[j].point.y));
                        }
                    }
                    
                }
            }
            
            for(int j = 0; j < hits.Length; j++)
            {
                Vector2 pos = new Vector2(raycastPositions[i].x-boundsOffset.x, raycastPositions[i].z-boundsOffset.z);
                if(!CanAgentWalkUnder(hits[j], hits, obstacles) ||
                   hits[j].collider.gameObject.layer != 8 ||
                   IsInsideObstacle(hits[j], obstacles) ||
                   hits[j].collider.gameObject.CompareTag("PathFindingObstacle") ||
                   !IsInsideBound(pos))
                {
                    continue;
                }
                
                PathfindingNode node = new PathfindingNode(
                    currentNodeIndex,
                    new Vector2Int(i%zRaycastAmount, i/zRaycastAmount),
                    hits[j].point,
                    0);
                currentNodeIndex++;
                nodesPerCell[i].Add(node);
                nodes.Add(node);
            }
        }

        List<int> borderNodes = new List<int>();
        //Neighbor attribution
        for (int i = 0; i < raycastPositions.Length; i++)//raycast positions
        {
            foreach (int neighborIndex in GetNeighborsIndex(i))//neighboring raycasts' positions
            {
                foreach (PathfindingNode node in nodesPerCell[i])//nodes by raycast's position
                {
                    foreach (var neighbor in nodesPerCell[neighborIndex])//nodes' neighbors by raycasts' positions
                    {
                        if(Mathf.Abs(node.position.y - neighbor.position.y) <= maxVerticalDistance) node.neighborsIndex.Add(neighbor.index);
                    }
                }
            }
        }
        for(int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].neighborsIndex.Count < 8)
            {
                nodes[i].wallAvoidance = wallAvoidanceDistance;
                borderNodes.Add(i);
            }
        }
        
        //WallAvoidance
        foreach (int borderNode in borderNodes)
        {
            UpdateNeighboringNodesWallValue(borderNode, wallAvoidanceDistance);
        }
    }
    void UpdateNeighboringNodesWallValue(int node, int wallAvoidanceValue)
    {
        if (wallAvoidanceValue < 0)return;
        
        foreach (int neighbor in nodes[node].neighborsIndex)
        {
            if(nodes[neighbor].wallAvoidance < wallAvoidanceValue-1)
            {
                nodes[neighbor].wallAvoidance = wallAvoidanceValue - 1;
                UpdateNeighboringNodesWallValue(neighbor, wallAvoidanceValue - 1);
            }
        }
    }
    List<int> GetNeighborsIndex(int index)
    {
        List<int> neighbors = new List<int>();

        int currentX = index / zRaycastAmount;
        int currentY = index % zRaycastAmount;

        for (int offsetY = -1; offsetY <= 1; offsetY++)
        {
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                if (offsetX == 0 && offsetY == 0) continue;

                int newX = currentX + offsetX;
                int newY = currentY + offsetY;

                if (newX < 0 || newX >= xRaycastAmount) continue;
                if (newY < 0 || newY >= zRaycastAmount) continue;

                int newIndex = newX * zRaycastAmount + newY;

                neighbors.Add(newIndex);
            }
        }

        return neighbors;
    }

    bool CanAgentWalkUnder(RaycastHit hit, RaycastHit[] hits, List<(float,float)> bounds)
    {
        foreach (RaycastHit hit1 in hits)
        {
            float distance = hit1.point.y - hit.point.y; 
            if (distance < agentHeight && distance > 0) return false;
        }
        foreach ((float,float) bound in bounds)
        {
            float distance = bound.Item1 - hit.point.y; 
            if (distance < agentHeight && distance > 0) return false;
        }
        return true;
    }

    bool IsInsideObstacle(RaycastHit hit, List<(float,float)> bounds)
    {
        foreach ((float,float) minMax in bounds)
        {
            if(hit.point.y >= minMax.Item1 && hit.point.y <= minMax.Item2) return true;
        }
        return false;
    }

    #if UNITY_EDITOR
    private void DrawGizmos()
    {
        if(boundsVertices.Count < 3) return;
        
        Handles.color = Color.yellow;
        Handles.color = Color.yellow;
        if(drawBounds)
        {for (int i = 0; i < boundsVertices.Count; i++)
        {
            Vector2 v1 = boundsVertices[i];
            Vector2 v2 = boundsVertices[(i + 1)%boundsVertices.Count];

            Vector3 pos1 = new Vector3(v1.x,-boundsHeight*0.5f,v1.y) + boundsOffset;
            Vector3 pos2 = new Vector3(v1.x,boundsHeight*0.5f,v1.y) + boundsOffset;
            
            Vector3 pos3 = new Vector3(v2.x,-boundsHeight*0.5f,v2.y) + boundsOffset;
            Vector3 pos4 = new Vector3(v2.x,boundsHeight*0.5f,v2.y) + boundsOffset;
            
            Handles.DrawLine(pos1, pos2);
            Handles.Label(pos2 + Vector3.up*0.1f, $"{i}");
            Handles.DrawLine(pos1, pos3);
            Handles.DrawLine(pos2, pos4);
        }}
        
        Handles.color = new Color(0.9f,0.9f,0.3f);
        if(drawNodes)
        {
            if (drawBoundingBox)
            {
                (Vector2, Vector2) BBMinMax = GetBoundsBoxMinMax();
                Vector2 BBExtent = BBMinMax.Item2 - BBMinMax.Item1;
                Vector2 BBCenter = GetBoundingBoxCenter(BBMinMax.Item1, BBMinMax.Item2);
                Handles.DrawWireCube(boundsOffset + new Vector3(BBCenter.x, 0, BBCenter.y),
                    new Vector3(BBExtent.x, boundsHeight, BBExtent.y));
            }
        }
        
        if(drawNodes || drawNodesConnections)
        {
            foreach (PathfindingNode node in nodes)
            {
                Handles.color = wallAvoidanceGradient.Evaluate(wallAvoidanceDistance == 0 ? 0 : (node.wallAvoidance / (float)wallAvoidanceDistance)); 
                if(drawNodes) Handles.SphereHandleCap(0, node.position, Quaternion.identity, nodeSize, EventType.Repaint);
                
                if(drawNodesConnections)
                {
                    foreach (int n in node.neighborsIndex)
                    {
                        float t = wallAvoidanceDistance == 0 ? 0 : Mathf.Min(node.wallAvoidance, nodes[n].wallAvoidance) / (float)wallAvoidanceDistance; 
                        Handles.color = wallAvoidanceGradient.Evaluate(t);
                        Handles.DrawLine(node.position, nodes[n].position);
                    }
                }
            }
        }
        
        Handles.color = new Color(0.7f, 0.5f, 0.7f);
        if(drawObstacles)
        {
            foreach (Vector3 v3 in obstaclesDebug)
            {
                Handles.SphereHandleCap(0, v3, Quaternion.identity, nodeSize/2, EventType.Repaint);
            }
        }
    }
    #endif
}
