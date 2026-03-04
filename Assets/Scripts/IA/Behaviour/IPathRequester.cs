using System.Collections.Generic;
using UnityEngine;

public interface IPathRequester
{
    public void RequestPath(List<PathfindingNode> path);
}
