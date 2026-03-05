using System.Collections.Generic;
using UnityEngine;

public interface IPathRequester
{
    public void OnPathAnswer(List<PathfindingNode> path);
}
