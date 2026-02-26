using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class FloorDetection : MonoBehaviour
{
    [SerializeField] private Vector3 detectionExtent;
    [SerializeField] private float detectionPrecision = 0.3f;
    [SerializeField] private float maxVerticalDistance = 0.3f;
    [SerializeField] private float agentHeight = 0.5f;
    
    List<PathfindingNode> nodes = new List<PathfindingNode>();

    private int xRaycastAmount, zRaycastAmount;
    private void Update()
    {
        //GetRaycastPositions();
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
        Vector3[] raycastPositions = GetRaycastPositions();
        Dictionary<int, List<PathfindingNode>> nodesPerCell = new();
        
        for(int i = 0; i < raycastPositions.Length; i++)
        {
            RaycastHit[] hits = Physics.RaycastAll(raycastPositions[i], Vector3.down, detectionExtent.y);

            nodesPerCell[i] = new List<PathfindingNode>();

            for(int j = 0; j < hits.Length; j++)
            {
                if(j != hits.Length-1 && hits[j+1].point.y - hits[j].point.y < agentHeight)
                {
                    Debug.Log(hits[j].point.y - hits[j + 1].point.y);
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

        int currentX = index % xRaycastAmount;
        int currentY = index / xRaycastAmount;

        for (int offsetY = -1; offsetY <= 1; offsetY++)
        {
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                if (offsetX == 0 && offsetY == 0) continue;

                int newX = currentX + offsetX;
                int newY = currentY + offsetY;

                if (newX < 0 || newX >= xRaycastAmount) continue;
                if (newY < 0 || newY >= zRaycastAmount) continue;

                int newIndex = newY * xRaycastAmount + newX;

                neighbors.Add(newIndex);
            }
        }

        return neighbors;
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
    }
}
