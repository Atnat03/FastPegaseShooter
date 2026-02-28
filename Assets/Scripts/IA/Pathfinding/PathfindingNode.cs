using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathfindingNode
{
    public int index;
    public Vector3 position;
    public List<int> neighborsIndex = new List<int>();

    public PathfindingNode(int index, Vector3 position)
    {
        this.index = index;
        this.position = position;
    }
}
