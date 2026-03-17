using System.Collections.Generic;
using UnityEngine;

public class GridCreatorPreferences : ISavable<GridCreatorPreferences>
{
    [Header("Bounding box")]
    public Vector3 boundsOffset =  Vector3.zero;
    public float boundsHeight = 0.5f;
    public List<Vector2> boundsVertices = new List<Vector2>();
    
    [Header("Grid Generation Parameters")]
    public float detectionPrecision = 0.3f;
    public float maxVerticalDistance = 0.3f;
    public float agentHeight = 0.5f;
    
    [Header("Node Parameters")]
    public int wallAvoidanceDistance = 3;
    
    [Header("Debug")]
    public float nodeSize = 0.05f;
    //public Gradient wallAvoidanceGradient = new Gradient();
    
    public bool drawBounds = true;
    public bool drawObstacles = false;
    public bool drawNodes = false;
    public bool drawNodesConnections = true;
    
    public GridCreatorPreferences(){}

    public GridCreatorPreferences(Vector3 boundsOffset, float boundsHeight, List<Vector2> boundsVertices,
        float detectionPrecision, float maxVerticalDistance, float agentHeight,
        int wallAvoidanceDistance, float nodeSize,
        bool drawBounds, bool drawObstacles, bool drawNodes, bool drawNodesConnections)
    {
        this.boundsOffset = boundsOffset;
        this.boundsHeight = boundsHeight;
        this.boundsVertices = boundsVertices;
        this.detectionPrecision = detectionPrecision;
        this.maxVerticalDistance = maxVerticalDistance;
        this.agentHeight = agentHeight;
        this.wallAvoidanceDistance = wallAvoidanceDistance;
        this.nodeSize = nodeSize;
        this.drawBounds = drawBounds;
        this.drawObstacles = drawObstacles;
        this.drawNodes = drawNodes;
        this.drawNodesConnections = drawNodesConnections;
    }
    
    
    public GridCreatorPreferences GetFromJSon()
    {
        return SaveManager.Load<GridCreatorPreferences>();
    }

    public void SaveToJson()
    {
        SaveManager.Save(this);
    }
}
