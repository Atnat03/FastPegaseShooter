using System.Collections.Generic;
using UnityEngine;

public class AStarAlgorithm : MonoBehaviour
{
    private MinHeap<AStarNode> _toSearch;
    private readonly Stack<AStarNode> _nodePool = new Stack<AStarNode>(2000);
    private readonly Stack<AStarNode> _nodeToRelease = new();
    public void Init()
    {
        _toSearch = new MinHeap<AStarNode>(2000, (a, b) =>
        {
            int result = a.F.CompareTo(b.F);//Sorting by total Cost
            if (result != 0)
                return result;
                
            result = a.H.CompareTo(b.H);//Sorting by estimated distance to target
            if(result != 0)
                return result;
                
            return a.Node.GetHashCode().CompareTo(b.Node.GetHashCode());//Security sorting
        });
    }
    
    public List<PathfindingNode> FindPathFromGrid(List<PathfindingNode> grid, PathfindingNode startNode, PathfindingNode targetNode)
    {
        //Creating a SortedSet where elements are ordered by F, then H
        //last return is a security.
        
        Dictionary<PathfindingNode, AStarNode> toSearchHash = new Dictionary<PathfindingNode, AStarNode>();

        AStarNode firstNode = new AStarNode { Node = startNode }; 
        _toSearch.Add(firstNode);
        toSearchHash.Add(firstNode.Node, firstNode);
        
        HashSet<PathfindingNode> searched = new HashSet<PathfindingNode>();

        while (_toSearch.Count > 0)
        {
            AStarNode currentNode = _toSearch.RemoveMin(); 
            
            //check for old nodes version
            if(!toSearchHash.ContainsKey(currentNode.Node) ||
               currentNode != toSearchHash[currentNode.Node]) continue;
            
            searched.Add(currentNode.Node);
            

            if (currentNode.Node == targetNode)
            {
                List<PathfindingNode> path = new List<PathfindingNode>();
                ReconstructPath(currentNode, startNode, path);
                
                //Heap and Pool clearing
                _toSearch.Clear();
                while (_nodeToRelease.Count > 0)
                {
                    ReleaseNode(_nodeToRelease.Pop());
                }
                //_nodeToRelease.Clear();
                
                return path;
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

                        AStarNode newNode = PoolNode();
                        newNode.Node = grid[neighbor];
                        newNode.toStart = currentNode;
                        newNode.G = costToNeighbor;
                        newNode.H = distToTarget;
                        newNode.F = costToNeighbor + distToTarget;
                        
                            
                        _toSearch.Add(newNode);
                        toSearchHash.Add(newNode.Node, newNode);
                    }
                    else
                    {
                        toSearchHash[grid[neighbor]].G = costToNeighbor;
                        toSearchHash[grid[neighbor]].F = toSearchHash[grid[neighbor]].G + toSearchHash[grid[neighbor]].H; 
                        toSearchHash[grid[neighbor]].toStart = currentNode;

                        //inserting a better node 
                        _toSearch.Add(toSearchHash[grid[neighbor]]);
                    }
                }
            }
            
            toSearchHash.Remove(currentNode.Node);
            _nodeToRelease.Push(currentNode);
        }
        return null;
    }

    private void ReconstructPath(AStarNode currentNode, PathfindingNode startNode, List<PathfindingNode> path)
    {
        AStarNode currentPathTile = currentNode;
        path.Add(currentPathTile.Node); //= new List<PathfindingNode>() {currentPathTile.Node};
        while (currentPathTile.Node != startNode)
        {
            currentPathTile = currentPathTile.toStart;
            path.Add(currentPathTile.Node);
        }
        //return path;
    }

    int GetDistance(PathfindingNode nodeA, PathfindingNode nodeB)
    {
        const int diagonalCost = 14;
        const int straightCost = 10;

        int distX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int distY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        int baseCost = distX > distY
            ? diagonalCost * distY + straightCost * (distX - distY)
            : diagonalCost * distX + straightCost * (distY - distX);

        return baseCost + nodeB.travelCost;
    }

    public class AStarNode
    {
        public PathfindingNode Node;
        public AStarNode toStart;
        public int G; //Distance from start
        public int H; //Estimated distance to target
        public int F; //G + H + Cost
    }

    AStarNode PoolNode()
    {
        if(_nodePool.Count > 0)
            return  _nodePool.Pop();
        
        return new AStarNode();
    }

    private void ReleaseNode(AStarNode node)
    {
        node.Node = null;
        node.toStart = null;
        node.G = 0;
        node.H = 0;
        node.F = 0;
        
        _nodePool.Push(node);
    }
}
