#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class PathfindingReaderVisualizer : MonoBehaviour
{
    [HideInInspector] public bool drawNodes = true;
    [HideInInspector] public bool drawNodesConnections = true;
    [HideInInspector] public Gradient walkingCostGradient = new Gradient();
    [HideInInspector] public int upperWalkingCostValue = 1;
    [HideInInspector] public float nodeDrawSize = 0.1f;
    
    [HideInInspector] public bool needToRebuildMatrix = false;

    private Material drawMaterial;
    private List<(Vector3, Color)> lineMatrix = new();
    private List<(Vector3, Color)> nodeMatrix = new();
    
    PathfindingGridReader _gridReader;

    private PathfindingGridSO lastFrameGrid;
    private HashSet<int> computedNodes = new HashSet<int>();
    PathfindingGridReader PGR
    {
        get
        {
            if (_gridReader == null) _gridReader = GetComponent<PathfindingGridReader>();
            return _gridReader;
        }
        set => _gridReader = value;
    }

    
    private void Update()
    {
        if (lastFrameGrid != PGR.pathfindingGridSO)
        {
            lastFrameGrid = PGR.pathfindingGridSO;
            needToRebuildMatrix = true;
        }
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += NodeAndConnectionDrawing;
        PGR.VariableChanged = true;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= NodeAndConnectionDrawing;
    }
    
    void GenerateMatrices(List<PathfindingNode> nodes, float maxCost)
    {
        if (drawMaterial == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            drawMaterial = new Material(shader);
            
            drawMaterial.hideFlags = HideFlags.HideAndDontSave;
            
            drawMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            drawMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstAlpha);
            drawMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            drawMaterial.SetInt("_ZWrite", 0);
        }
        
        lineMatrix.Clear();
        nodeMatrix.Clear();
        computedNodes.Clear();
        
        foreach(PathfindingNode node in nodes)
        {
            PGR._additionalWalkingCost.TryGetValue(node.index, out int addCostA);
            float t = maxCost > 0 ? (node.travelCost + addCostA) / maxCost : 0f;
            Color color = walkingCostGradient.Evaluate(t);
            
            nodeMatrix.Add((node.position, color));
            
            foreach(int n in node.neighborsIndex)
            {
                if(computedNodes.Contains(n)) continue;
                
                lineMatrix.Add((node.position, color));
                lineMatrix.Add((nodes[n].position, color));
            }

            computedNodes.Add(node.index);
        }
    }
    void DrawConnections()
    {
        if(lineMatrix.Count == 0) return;

        drawMaterial.SetPass(0);

        GL.PushMatrix();
        GL.Begin(GL.LINES);

        for(int i = 0; i < lineMatrix.Count; i += 2)
        {
            GL.Color(lineMatrix[i].Item2);
            GL.Vertex(lineMatrix[i].Item1);
            GL.Color(lineMatrix[i+1].Item2);
            GL.Vertex(lineMatrix[i+1].Item1);
        }

        GL.End();
        GL.PopMatrix();
    }
    void DrawNodes(float nodeSize)
    {
        if(nodeMatrix.Count == 0) return;
        
        drawMaterial.SetPass(0);
        
        GL.PushMatrix();
        GL.Begin(GL.QUADS);

        foreach((Vector3, Color) node in nodeMatrix)
        {
            Vector3 pos = node.Item1;
            float s = nodeSize * 0.5f;

            GL.Color(node.Item2);
            GL.Vertex(pos + new Vector3(-s, 0.05f, -s));
            GL.Color(node.Item2);
            GL.Vertex(pos + new Vector3(-s, 0.05f, s));
            GL.Color(node.Item2);
            GL.Vertex(pos + new Vector3(s, 0.05f, s));
            GL.Color(node.Item2);
            GL.Vertex(pos + new Vector3(s, 0.05f, -s));
        }

        GL.End();
        GL.PopMatrix();
    }

    void NodeAndConnectionDrawing(SceneView obj)
    {
        if((PGR.VariableChanged || needToRebuildMatrix) && PGR.pathfindingGridSO != null)
        {
            GenerateMatrices(PGR.pathfindingGridSO.nodes, upperWalkingCostValue);
            PGR.VariableChanged = false;
            needToRebuildMatrix = false;
        }
        
        if(drawNodesConnections)DrawConnections();
        if(drawNodes)DrawNodes(nodeDrawSize);
    }
}
#endif
