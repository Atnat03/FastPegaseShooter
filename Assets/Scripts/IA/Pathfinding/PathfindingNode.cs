using System.Collections.Generic;
using UnityEngine;

public class PathfindingNode
{
    public Vector3 position;
    public List<PathfindingNode> neighbors = new List<PathfindingNode>();
}
