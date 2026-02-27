using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[ExecuteAlways]
public class FloorDetection : MonoBehaviour
{
    [SerializeField] private Vector3 detectionExtent;
    [SerializeField] private float detectionPrecision = 0.3f;
    [SerializeField] private float maxVerticalDistance = 0.3f;
    [SerializeField] private float agentHeight = 0.5f;

    List<PathfindingNode> nodes = new List<PathfindingNode>();
    List<Vector3> obstaclesDebug =  new List<Vector3>();

    private int xRaycastAmount, zRaycastAmount;
    private void Update()
    {
        InitializeNodes();
    }

    Vector3[] GetRaycastPositions()
    {
        xRaycastAmount = (int)(detectionExtent.x / detectionPrecision);
        zRaycastAmount = (int)(detectionExtent.z / detectionPrecision);
        
        float xStartOffset = (detectionExtent.x - detectionPrecision * (xRaycastAmount-1))*0.5f;
        float zStartOffset = (detectionExtent.z - detectionPrecision * (zRaycastAmount-1))*0.5f;
        
        Vector3 positionOffset = transform.position + new Vector3(-detectionExtent.x*0.5f+xStartOffset, detectionExtent.y*0.5f, -detectionExtent.z*0.5f+zStartOffset);
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

    void InitializeNodes()
    {
        nodes.Clear();
        obstaclesDebug.Clear();
        
        Vector3[] raycastPositions = GetRaycastPositions();
        Dictionary<int, List<PathfindingNode>> nodesPerCell = new();

        for(int i = 0; i < raycastPositions.Length; i++)
        {
            RaycastHit[] hits = Physics.RaycastAll(raycastPositions[i], Vector3.down, detectionExtent.y);
            //hits.OrderBy(ray => ray.point.y);

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
                            
                            //hits.RemoveAt(j);
                        }
                    }
                    
                }
            }
            
            for(int j = 0; j < hits.Length; j++)
            {
                if(!CanAgentWalkUnder(hits[j], hits, obstacles) ||
                   hits[j].collider.gameObject.layer != 8 ||
                   IsInsideObstacle(hits[j], obstacles) ||
                   hits[j].collider.gameObject.CompareTag("PathFindingObstacle"))
                {
                    continue;
                }
                
                PathfindingNode node = new PathfindingNode { position = hits[j].point };
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
                        if(Mathf.Abs(node.position.y - neighbor.position.y) <= maxVerticalDistance) node.neighbors.Add(neighbor);
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, detectionExtent);
        
        

        Gizmos.color = new Color(0.8f,0.2f,0.2f);
        foreach (PathfindingNode node in nodes)
        {
            Gizmos.DrawSphere(node.position, 0.02f);
            foreach (PathfindingNode n in node.neighbors)
            {
                Gizmos.DrawLine(node.position, n.position);
            }
        }
        
        Gizmos.color = new Color(0.9f,0.9f,0.3f);
        foreach (Vector3 v3 in obstaclesDebug)
        {
            Gizmos.DrawSphere(v3, 0.01f);
        }
    }
}
