using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathfindingNode
{
    public int index;
    public Vector2Int gridPosition;
    public Vector3 position;
    public int travelCost;
    public List<int> neighborsIndex = new List<int>();

    public PathfindingNode(int index, Vector2Int gridPos, Vector3 position, int travelCost)
    {
        this.index = index;
        this.gridPosition = gridPos;
        this.position = position;
        this.travelCost = travelCost;
    }

    public PathfindingNode Copy()
    {
        PathfindingNode node = new PathfindingNode(index, gridPosition, position, travelCost);
        node.neighborsIndex = new List<int>(neighborsIndex);
        /*foreach (int neighborIndex in neighborsIndex)
        {
            node.neighborsIndex.Add(neighborIndex);
        }*/
        return node;
    }
}
