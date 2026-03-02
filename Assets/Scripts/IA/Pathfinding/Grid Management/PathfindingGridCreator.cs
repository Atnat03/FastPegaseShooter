using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

[ExecuteAlways]
public class PathfindingGridCreator : MonoBehaviour
{
    [Header("Bounding box")]
    [SerializeField] private float boundsHeight = 0.5f;
    [SerializeField] private List<Vector2> boundsVertices = new List<Vector2>();
    
    [Header("Grid Generation Parameters")]
    [SerializeField] private float detectionPrecision = 0.3f;
    [SerializeField] private float maxVerticalDistance = 0.3f;
    [SerializeField] private float agentHeight = 0.5f;

    [HideInInspector] public List<PathfindingNode> nodes = new List<PathfindingNode>();

    private int xRaycastAmount, zRaycastAmount;
    
    //Debug Variables
    List<Vector3> obstaclesDebug =  new List<Vector3>();
    [HideInInspector] public bool drawBounds = true;
    [HideInInspector] public bool drawBoundingBox = true;
    [HideInInspector] public bool drawObstacles = true;
    [HideInInspector] public bool drawNodes = true;
    [HideInInspector] public bool drawNodesConnections = true;
    //
    private void Update()
    {
        if(boundsVertices.Count < 3) return;
        
        InitializeNodes();
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
        
        Vector3 positionOffset = transform.position + new Vector3(-boundingBoxExtent.x*0.5f+xStartOffset, boundsHeight*0.5f, -boundingBoxExtent.y*0.5f+zStartOffset);
        Vector3[] raycastPositions = new Vector3[xRaycastAmount * zRaycastAmount];
        for (int i = 0; i < xRaycastAmount; i++)
        {
            for (int j = 0; j < zRaycastAmount; j++)
            {
                raycastPositions[i * zRaycastAmount + j] = positionOffset + new Vector3(i * detectionPrecision, 0, j * detectionPrecision);
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
                Vector2 pos = new Vector2(raycastPositions[i].x-transform.position.x, raycastPositions[i].z-transform.position.z);
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
                    hits[j].point);
                currentNodeIndex++;
                nodesPerCell[i].Add(node);
                nodes.Add(node);
            }
        }

        for (int i = 0; i < raycastPositions.Length; i++)
        {
            foreach (int neighborIndex in GetNeighborsIndex(i))
            {
                foreach (var node in nodesPerCell[i])
                {
                    foreach (var neighbor in nodesPerCell[neighborIndex])
                    {
                        if(Mathf.Abs(node.position.y - neighbor.position.y) <= maxVerticalDistance) node.neighborsIndex.Add(neighbor.index);
                    }
                }
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

    private void OnDrawGizmos()
    {
        if(boundsVertices.Count < 3) return;
        
        Gizmos.color = Color.yellow;
        Handles.color = Color.yellow;
        if(drawBounds)
        {for (int i = 0; i < boundsVertices.Count; i++)
        {
            Vector2 v1 = boundsVertices[i];
            Vector2 v2 = boundsVertices[(i + 1)%boundsVertices.Count];

            Vector3 pos1 = new Vector3(v1.x,-boundsHeight*0.5f,v1.y) + transform.position;
            Vector3 pos2 = new Vector3(v1.x,boundsHeight*0.5f,v1.y) + transform.position;
            
            Vector3 pos3 = new Vector3(v2.x,-boundsHeight*0.5f,v2.y) + transform.position;
            Vector3 pos4 = new Vector3(v2.x,boundsHeight*0.5f,v2.y) + transform.position;
            
            Gizmos.DrawLine(pos1, pos2);
            Handles.Label(pos2 + Vector3.up*0.1f, $"{i}");
            Gizmos.DrawLine(pos1, pos3);
            Gizmos.DrawLine(pos2, pos4);
        }}
        
        Gizmos.color = new Color(0.9f,0.9f,0.3f);
        if(drawNodes)
        {
            if (drawBoundingBox)
            {
                (Vector2, Vector2) BBMinMax = GetBoundsBoxMinMax();
                Vector2 BBExtent = BBMinMax.Item2 - BBMinMax.Item1;
                Vector2 BBCenter = GetBoundingBoxCenter(BBMinMax.Item1, BBMinMax.Item2);
                Gizmos.DrawWireCube(transform.position + new Vector3(BBCenter.x, 0, BBCenter.y),
                    new Vector3(BBExtent.x, boundsHeight, BBExtent.y));
            }
        }
        
        Gizmos.color = new Color(0.8f,0.2f,0.2f);
        if(drawNodes || drawNodesConnections)
        {
            foreach (PathfindingNode node in nodes)
            {
                if(drawNodes) Gizmos.DrawSphere(node.position, 0.02f);
                if(drawNodesConnections)
                {
                    foreach (int n in node.neighborsIndex)
                    {
                        Gizmos.DrawLine(node.position, nodes[n].position);
                    }
                }
            }
        }
        
        Gizmos.color = new Color(0.7f, 0.5f, 0.7f);
        if(drawObstacles)
        {
            foreach (Vector3 v3 in obstaclesDebug)
            {
                Gizmos.DrawSphere(v3, 0.01f);
            }
        }
    }
}
