using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathfindingNode
{
    public int index;
    public Vector2Int gridPosition;
    public Vector3 position;
    public List<int> neighborsIndex = new List<int>();

    public PathfindingNode(int index, Vector2Int gridPos, Vector3 position)
    {
        this.index = index;
        this.gridPosition = gridPos;
        this.position = position;
    }
}
