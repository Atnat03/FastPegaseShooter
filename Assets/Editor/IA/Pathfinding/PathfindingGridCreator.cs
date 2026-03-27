using System;
using System.Collections.Generic;
using System.Linq;
using CustomConsole.Runtime.Logger;
using Unity.VisualScripting;
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
    [SerializeField] private float nodeSize = 0.05f;

    [HideInInspector] public List<PathfindingNode> nodes = new List<PathfindingNode>();

    private int xRaycastAmount, zRaycastAmount;
    
    //Debug Variables
    List<Vector3> obstaclesDebug =  new List<Vector3>();
    [HideInInspector] public bool drawBounds = true;
    [HideInInspector] public bool drawObstacles = false;
    [HideInInspector] public bool drawNodes = false;
    [HideInInspector] public bool drawNodesConnections = true;
    //

    private Color boundsColor;
    private Material nodeMaterial;
    private Material lineMaterial;
    
    bool isWorking = false;
    
    private GUIStyle titleStyle;
    private EditorListDrawerStyle listStyle;

    private bool _boundListOpened = true;


    [MenuItem("Tools/Pathfinding Grid Creator")]
    public static void ShowWindow()
    {
        PathfindingGridCreator window = GetWindow<PathfindingGridCreator>();
        
        window.titleContent = new GUIContent("Pathfinding Grid Creator");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        titleStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            fontSize = 18
        };
        titleStyle.normal.textColor = Color.white;
        listStyle = new EditorListDrawerStyle
        {
            p_titleStyle = titleStyle,
            p_label = (i => $"Index {i}"),
            p_removeColor = new Color(0.8f, 0.3f, 0.3f),
            p_addColor = new Color(0.3f, 0.8f, 0.3f),
            p_verticalMargin = 15
        };
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }


    public void CreateGUI()
    {
        LoadPreferences();
        
        nodeMaterial = new Material(Shader.Find("Unlit/Color"));
        nodeMaterial.color = new Color(0.8f,0.3f,0.3f, 1f);
        nodeMaterial.enableInstancing = true;
        
        lineMaterial = new Material(Shader.Find("Unlit/Color"));
        lineMaterial.color = new Color(1f,0.4f,0.4f, 1f);
        lineMaterial.enableInstancing = true;
        
        GridCreatorGizmosDrawer.ClearLists();
    }

    public void OnGUI()
    {
        DrawGUI();
    }
    private void OnSceneGUI(SceneView obj)
    {
        DrawGizmos();
        SceneView.RepaintAll();
    }

    #region Window Drawing
    void DrawGUI()
    {
        GUIStyle warning = new GUIStyle
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            fontSize = 10
        };
        warning.normal.textColor = Color.yellow;
        GUIStyle bigTitleStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            fontSize = 24
        };
        bigTitleStyle.normal.textColor = Color.white;
        Color backgroundColor = GUI.color;
        
        float windowWidth = position.width;
        
        GUILayout.BeginHorizontal();
        
        GUILayout.BeginVertical(GUILayout.Width(windowWidth/2));
        GUILayout.Label("Generation Parameters", bigTitleStyle);
        GUILayout.Label("Bounding Box", titleStyle);
        boundsHeight = EditorGUILayout.FloatField("Bounds Height", boundsHeight);
        boundsOffset = EditorGUILayout.Vector3Field("Bounds Offset", boundsOffset);
        EditorUtilities.DrawList(boundsVertices, "Bounds Vertices", listStyle, ref _boundListOpened);
        GUILayout.Label("Connection Parameters", titleStyle);
        detectionPrecision = EditorGUILayout.FloatField("Detection Precision", detectionPrecision);
        maxVerticalDistance = EditorGUILayout.FloatField("Max Vertical Distance", maxVerticalDistance);
        agentHeight = EditorGUILayout.FloatField("Agent Height", agentHeight);
        
        //GUILayout.Label("Node Parameters", titleStyle);
        //wallAvoidanceDistance = EditorGUILayout.IntField("Wall Avoidance Distance", wallAvoidanceDistance);
        
        GUILayout.EndVertical();
        
        GUILayout.BeginVertical(GUILayout.Width(windowWidth/2));
        GUILayout.Label("Utilities", bigTitleStyle);
        GUILayout.Label("Preferences", titleStyle);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Preferences"))
        {
            LoadPreferences();
        }
        if (GUILayout.Button("Save Preferences"))
        {
            SavePreferences();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(15);
        
        GUILayout.Label("Grid Management", titleStyle);
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.3f,0.3f,0.8f);
        if (GUILayout.Button("Generate Grid") && !isWorking)
        {
            if(boundsVertices.Count < 3)
            {
                CustomLogger.CCErrorLog("Bounding box size is less than 3");
                GUILayout.EndHorizontal();
                return;
            }
            EditorUtility.DisplayDialog("Wait", "Grid creation will start", "OK");
            isWorking = true;
            InitializeNodes();
            
            GridCreatorGizmosDrawer.GenerateNodeMatrix(nodes);
            GridCreatorGizmosDrawer.GenerateConnectionMatrix(nodes);
            isWorking = false;
            EditorUtility.ClearProgressBar();
        }
        GUI.backgroundColor = new Color(0.5f,0.5f,0.8f);
        if (GUILayout.Button("Clear disconnected Graphs") && !isWorking)
        {
            EditorUtility.DisplayDialog("Wait", "Grid cleaning will start", "OK");
            isWorking = true;
            ClearSubGraphs();

            if (nodes.Count <= 0)
            {
                CustomLogger.CCErrorLog("Error when clearing graph");
                return;
            }
            
            GridCreatorGizmosDrawer.GenerateNodeMatrix(nodes);
            GridCreatorGizmosDrawer.GenerateConnectionMatrix(nodes);
            isWorking = false;
            EditorUtility.ClearProgressBar();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUI.backgroundColor = backgroundColor;
        if (GUILayout.Button("Clear Grid"))
        {
            nodes.Clear();
            GridCreatorGizmosDrawer.ClearLists();
        }
        GUILayout.Space(5);
        GUI.backgroundColor = new Color(0.8f,0.7f,0.1f);
        if (GUILayout.Button("Save Grid"))
        {
            SaveGrid(nodes);
        }
        GUI.backgroundColor = backgroundColor;

        GUILayout.Space(15);
        
        GUILayout.Label("Debug", titleStyle);
        GUILayout.Label("Warning:\nBounds and Obstacles are drawn on CPU,\n consider disabling if the editor is lagging", warning);
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = drawBounds ? Color.green : Color.red;
        if(GUILayout.Button("Draw Bounds")) drawBounds = !drawBounds;
        GUI.backgroundColor = drawObstacles ? Color.green : Color.red;
        if(GUILayout.Button("Draw Obstacles")) drawObstacles = !drawObstacles;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = drawNodes ? Color.green : Color.red;
        if(GUILayout.Button("Draw Nodes")) drawNodes = !drawNodes;
        GUI.backgroundColor = drawNodesConnections ? Color.green : Color.red;
        if(GUILayout.Button("Draw Connections")) drawNodesConnections = !drawNodesConnections;
        GUILayout.EndHorizontal();
        
        GUI.backgroundColor = backgroundColor;
        nodeSize = EditorGUILayout.FloatField("Node Size", nodeSize);
        boundsColor = EditorGUILayout.ColorField("Bounds Color", boundsColor);
        
        
        GUI.backgroundColor = backgroundColor;
        GUILayout.EndVertical();
        
        GUILayout.EndHorizontal();
    }
    
    /*void DrawList<T>(List<T> list, Func<int, string> label, string title, GUIStyle titleStyle =  null)
    {
        Color oldColor = GUI.color;
        Color red = new Color(0.8f, 0.3f, 0.3f);
        Color green = new Color(0.3f, 0.8f, 0.3f);
        
        GUILayout.Space(15);
        
        GUILayout.BeginVertical("box");
        GUILayout.Label(title, titleStyle);
        
        for (int i = list.Count-1; i >= 0; i--)
        {
            object value = list[i];
            EditorGUILayout.BeginHorizontal();
            if(typeof(T) == typeof(Vector2))
            {
                GUILayout.Label(label?.Invoke(i), GUILayout.Width(80));
                value = EditorGUILayout.Vector2Field("", (Vector2)value);
            }
            else if (typeof(Object).IsAssignableFrom(typeof(T)))
            {
                GUILayout.Label(label?.Invoke(i), GUILayout.Width(80));
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
        GUILayout.Space(15);
    }*/
    #endregion

    void SaveGrid(List<PathfindingNode> grid)
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
            foreach (PathfindingNode node in grid)
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

    #region Preferences
    void LoadPreferences()
    {
        GridCreatorPreferences GCP = new GridCreatorPreferences().GetFromJSon();
        
        boundsOffset = GCP.boundsOffset;
        boundsHeight = GCP.boundsHeight;
        boundsVertices =  GCP.boundsVertices;
        detectionPrecision  = GCP.detectionPrecision;
        maxVerticalDistance =  GCP.maxVerticalDistance;
        agentHeight =  GCP.agentHeight;
        //wallAvoidanceDistance =   GCP.wallAvoidanceDistance;
        nodeSize =  GCP.nodeSize;
        //wallAvoidanceGradient = GCP.wallAvoidanceGradient;
        drawBounds = GCP.drawBounds;
        drawObstacles = GCP.drawObstacles;
        drawNodes = GCP.drawNodes;
        drawNodesConnections  = GCP.drawNodesConnections;
        boundsColor = GCP.boundsColor;
    }
    void SavePreferences()
    {
        new GridCreatorPreferences(
        boundsOffset, boundsHeight, boundsVertices,
        detectionPrecision, maxVerticalDistance, agentHeight,
        wallAvoidanceDistance, nodeSize,
        drawBounds, drawObstacles, drawNodes, drawNodesConnections, 
        boundsColor).SaveToJson();
    }
    #endregion

    #region Graph Calculation

    #region Bounding Box
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
    #endregion

    #region Basic Grid
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

    void InitializeNodes()
    {
        nodes.Clear();
        obstaclesDebug.Clear();
        
        Vector3[] raycastPositions = GetRaycastPositions();
        Dictionary<int, List<PathfindingNode>> nodesPerCell = new();
        int currentNodeIndex = 0;
        
        for(int i = 0; i < raycastPositions.Length; i++)
        {
            RaycastHit[] hits = new RaycastHit[10];
            int hitCount = Physics.RaycastNonAlloc(raycastPositions[i], Vector3.down, hits, boundsHeight);

            nodesPerCell[i] = new List<PathfindingNode>();
            
            List<(float,float)> obstacles = new List<(float,float)>();
            for (int j = hitCount-1; j >= 0; j--)
            {
                if (hits[j].collider.gameObject.layer == 8 &&
                    hits[j].collider.gameObject.CompareTag("PathFindingObstacle"))
                {
                    float lenght = hits[j].collider.bounds.max.y - hits[j].collider.bounds.min.y;
                    RaycastHit[] backHits = new RaycastHit[10];
                        int backHitCount = Physics.RaycastNonAlloc(
                        new Vector3(raycastPositions[i].x,hits[j].point.y-lenght-0.1f,raycastPositions[i].z),
                        Vector3.up, backHits, lenght);
                    for(int k = 0; k < backHitCount; k++ )
                    {
                        if (backHits[k].collider == hits[j].collider)
                        {
                            obstaclesDebug.Add(hits[j].point);
                            obstaclesDebug.Add(backHits[k].point);
                            obstacles.Add((backHits[k].point.y, hits[j].point.y));
                        }
                    }
                    
                }
            }
            
            for(int j = 0; j < hitCount; j++)
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
        
        //WallAvoidance
        for(int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].neighborsIndex.Count < 8)
            {
                nodes[i].wallAvoidance = wallAvoidanceDistance;
                borderNodes.Add(i);
            }
        }
        foreach (int borderNode in borderNodes)
        {
            UpdateNeighboringNodesWallValue(borderNode, wallAvoidanceDistance);
        }
    }

    void ClearSubGraphs()
    {
        List<List<PathfindingNode>> subGraphs = new List<List<PathfindingNode>>();
        
        Queue<int> nodesToCheck = new Queue<int>();
        HashSet<int> visitedNodes = new HashSet<int>();
        while (visitedNodes.Count < nodes.Count || nodesToCheck.Count > 0)
        {
            if (nodesToCheck.Count <= 0)
            {
                PathfindingNode newNode = GetFirstNoneVisitedNode(visitedNodes);
                
                subGraphs.Add(new List<PathfindingNode>());
                nodesToCheck.Enqueue(newNode.index);
            }
            
            int index = nodesToCheck.Dequeue();
            
            subGraphs[^1].Add(nodes[index]);
            visitedNodes.Add(nodes[index].index);
            foreach (int neighborIndex in subGraphs[^1][^1].neighborsIndex)
            {
                if(!visitedNodes.Contains(neighborIndex))
                {
                    nodesToCheck.Enqueue(neighborIndex);
                    visitedNodes.Add(nodes[neighborIndex].index);
                }
            }
        }

        string txt = "";
        for(int i = 0; i < subGraphs.Count; i++)
        {
            txt += $"graph {i}:{subGraphs[i].Count} ||";
        }
        
        List<PathfindingNode> biggestSubGraph = new List<PathfindingNode>();

        //----- INDEX REMAPPING -----
        
        //old index => new index
        Dictionary<int, int> newIndexMapping = new Dictionary<int, int>();
        foreach (PathfindingNode node in subGraphs.OrderByDescending(g => g.Count).ToList().First())
        {
            biggestSubGraph.Add(node.Copy());
            newIndexMapping.Add(node.index, biggestSubGraph.Count-1);
        }

        foreach (PathfindingNode node in biggestSubGraph)
        {
            int oldIndex = node.index;
            node.index = newIndexMapping[oldIndex];
            for (int i = node.neighborsIndex.Count-1; i >= 0; i--)
            {
                if (newIndexMapping.ContainsKey(node.neighborsIndex[i]))
                {
                    node.neighborsIndex[i] = newIndexMapping[node.neighborsIndex[i]];
                }
                else //neighbor wasn't in the same subgraph (should'nt happen) we delete the neighbor
                {
                    node.neighborsIndex.RemoveAt(i);
                }
            }
        }
        
        nodes = biggestSubGraph;
    }

    PathfindingNode GetFirstNoneVisitedNode(HashSet<int> visitedNodes)
    {
        foreach (PathfindingNode node in nodes)
        {
            if(!visitedNodes.Contains(node.index)) return node;
        }
        return null;
    }
    #endregion
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
    #endregion

    #region Drawing
    private void DrawGizmos()
    {
        Handles.color = new Color(0.7f, 0.5f, 0.7f);
        if(drawObstacles)
        {
            foreach (Vector3 v3 in obstaclesDebug)
            {
                Handles.SphereHandleCap(0, v3, Quaternion.identity, nodeSize, EventType.Repaint);
            }
        }
        
        if(boundsVertices.Count < 3) return;
        
        if(drawNodes)
        {
            GridCreatorGizmosDrawer.DrawNodes(nodeMaterial, nodeSize);
        }
        if(drawNodesConnections)
        {
            GridCreatorGizmosDrawer.DrawConnections(lineMaterial);
        }
        
        Handles.color = boundsColor;
        if(drawBounds)
        {
            for (int i = 0; i < boundsVertices.Count; i++)
            {
                Vector2 v1 = boundsVertices[i];
                Vector2 v2 = boundsVertices[(i + 1)%boundsVertices.Count];
                
                Vector3 pos1 = new Vector3(v1.x,-boundsHeight*0.5f,v1.y) + boundsOffset;
                Vector3 pos2 = new Vector3(v1.x,boundsHeight*0.5f,v1.y) + boundsOffset;
                
                Vector3 pos3 = new Vector3(v2.x,-boundsHeight*0.5f,v2.y) + boundsOffset;
                Vector3 pos4 = new Vector3(v2.x,boundsHeight*0.5f,v2.y) + boundsOffset;
                        
                GUIStyle style = new GUIStyle();
                style.normal.textColor = boundsColor;
                Handles.Label(pos2 + Vector3.up, $"{i}", style);
                Handles.DrawLine(pos1, pos2);
                Handles.DrawLine(pos1, pos3);
                Handles.DrawLine(pos2, pos4);
            }
        }
    }
    #endregion
}
