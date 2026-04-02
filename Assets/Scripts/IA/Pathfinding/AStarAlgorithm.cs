using System.Collections.Generic;
using UnityEngine;

public class AStarAlgorithm : MonoBehaviour
{
    public List<PathfindingNode> FindPathFromGrid(List<PathfindingNode> grid, PathfindingNode startNode, PathfindingNode targetNode)
    {
        //Creating a SortedSet where elements are ordered by F, then H
        //last return is a security.
        SortedSet<AStarNode> toSearch = new SortedSet<AStarNode>(
            Comparer<AStarNode>.Create((a, b) =>
            {
                int result = a.F.CompareTo(b.F);
                if (result != 0)
                    return result;
                
                result = a.H.CompareTo(b.H);
                if(result != 0)
                    return result;
                
                return a.Node.GetHashCode().CompareTo(b.Node.GetHashCode());
            }));
        Dictionary<PathfindingNode, AStarNode> toSearchHash = new Dictionary<PathfindingNode, AStarNode>();

        AStarNode firstNode = new AStarNode { Node = startNode }; 
        toSearch.Add(firstNode);
        toSearchHash.Add(firstNode.Node, firstNode);
        
        HashSet<PathfindingNode> searched = new HashSet<PathfindingNode>();

        while (toSearch.Count > 0)
        {
            AStarNode currentNode = toSearch.Min; //toSearch[0];
            
            searched.Add(currentNode.Node);
            toSearch.Remove(currentNode);
            toSearchHash.Remove(currentNode.Node);

            if (currentNode.Node == targetNode)
            {
                return ReconstructPath(currentNode, startNode);
            }

            //Check for walkability here by only selecting walkable nodes
            foreach (int neighbor in currentNode.Node.neighborsIndex)
            {
                if(searched.Contains(grid[neighbor])) continue;

                bool inSearch = toSearchHash.ContainsKey(grid[neighbor]);
                int costToNeighbor = currentNode.G + GetDistance(currentNode.Node, grid[neighbor]);
                
                if(!inSearch || (inSearch && costToNeighbor < toSearchHash[grid[neighbor]].G))
                {
                    if (!inSearch)
                    {
                        int distToTarget = GetDistance(grid[neighbor], targetNode);
                        AStarNode newNode = new AStarNode
                        {
                            Node = grid[neighbor],
                            toStart = currentNode,
                            G = costToNeighbor,
                            H = distToTarget,
                            F = costToNeighbor + distToTarget
                        };
                        toSearch.Add(newNode);
                        toSearchHash.Add(newNode.Node, newNode);
                    }
                    else
                    {
                        toSearch.Remove(toSearchHash[grid[neighbor]]);
                        
                        toSearchHash[grid[neighbor]].G = costToNeighbor;
                        toSearchHash[grid[neighbor]].F = toSearchHash[grid[neighbor]].G + toSearchHash[grid[neighbor]].H; 
                        toSearchHash[grid[neighbor]].toStart = currentNode;

                        toSearch.Add(toSearchHash[grid[neighbor]]);
                    }
                }
            }
        }
        return null;
    }

    private List<PathfindingNode> ReconstructPath(AStarNode currentNode, PathfindingNode startNode)
    {
        AStarNode currentPathTile = currentNode;
        List<PathfindingNode> path = new List<PathfindingNode>() {currentPathTile.Node};
        while (currentPathTile.Node != startNode)
        {
            currentPathTile = currentPathTile.toStart;
            path.Add(currentPathTile.Node);
        }
        return path;
    }

    int GetDistance(PathfindingNode nodeA, PathfindingNode nodeB)
    {
        const int diagonalCost = 14;
        const int straightCost = 10;

        int distX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int distY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        return distX > distY
            ? diagonalCost * distY + straightCost * (distX - distY)
            : diagonalCost * distX + straightCost * (distY - distX);
    }

    public class AStarNode
    {
        public PathfindingNode Node;
        public AStarNode toStart;
        public int G; //Distance from start
        public int H; //Estimated distance to target
        public int F; //G + H
    }
}
