using System.Collections.Generic;
using UnityEngine;

public class AStarAlgorithm : MonoBehaviour
{
    public List<PathfindingNode> FindPathFromGrid(List<PathfindingNode> grid, PathfindingNode startNode, PathfindingNode targetNode)
    {
        List<AStarNode> toSearch = new List<AStarNode>() { new AStarNode
        {
            Node = startNode
        } };
        List<AStarNode> searched = new List<AStarNode>();

        while (toSearch.Count > 0)
        {
            AStarNode currentNode = toSearch[0];
            foreach (AStarNode node in toSearch)
            {
                if (node.F < currentNode.F || (node.F == currentNode.F && node.H < currentNode.H))
                {
                    currentNode = node;
                }
            }
            
            searched.Add(currentNode);
            toSearch.Remove(currentNode);

            if (currentNode.Node == targetNode)
            {
                AStarNode currentPathTile = currentNode;
                List<PathfindingNode> path = new List<PathfindingNode>();
                while (currentPathTile.Node != startNode)
                {
                    path.Add(currentPathTile.Node);
                    currentPathTile = currentPathTile.toStart;
                }
                return path;
            }

            //Check for walkability here by only selecting walkable nodes
            foreach (int neighbor in currentNode.Node.neighborsIndex)
            {
                bool inSearch = DoContainNode(toSearch, grid[neighbor]).Item1;
                int costToNeighbor = currentNode.G + GetDistance(currentNode.Node, targetNode);
                
                (bool, AStarNode) doContains = DoContainNode(toSearch, grid[neighbor]);
                if (!inSearch || (doContains.Item1 && costToNeighbor < doContains.Item2.G))
                {
                    if (!inSearch)
                    {
                        AStarNode newNode = new AStarNode
                        {
                            Node = grid[neighbor],
                            toStart = currentNode,
                            G = currentNode.G + costToNeighbor,
                            H = GetDistance(grid[neighbor], targetNode)
                        };
                        toSearch.Add(newNode);
                    }
                    else
                    {
                        doContains.Item2.G = currentNode.G + costToNeighbor;
                        doContains.Item2.toStart = currentNode;
                    }
                }
            }
        }
        return null;
    }

    (bool,AStarNode) DoContainNode(List<AStarNode> list, PathfindingNode node)
    {
        foreach (AStarNode n in list)
            if (n.Node == node) return (true,n);
        
        return (false, new AStarNode());
    }

    int GetDistance(PathfindingNode a, PathfindingNode b) => Mathf.FloorToInt((a.position-b.position).sqrMagnitude);

    public class AStarNode
    {
        public PathfindingNode Node;
        public AStarNode toStart;
        public int G; //Distance from start
        public int H; //Estimated distance to target
        public int F; //G + H
    }
}
