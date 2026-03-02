using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;

public class PathfindingGridReader : MonoBehaviour, IPlayerPositionListener
{
    public PathfindingGridSO pathfindingGridSO;

    public ThreeDimensionalTree searchTree;

    [SerializeField] private AStarAlgorithm _aStarAlgorithm;
    
    //Debug Variables
    [SerializeField] private Transform starTransform;
    [HideInInspector] public bool drawNodes = true;
    [HideInInspector] public bool drawNodesConnections = true;
    Vector3 playerPosition;
    static ProfilerMarker findClosestNodeMarker = new ProfilerMarker("FindClosestNode");
    //

    private void Start()
    {
        searchTree = new ThreeDimensionalTree();
        List<Vector3> values = pathfindingGridSO.nodes.Select(n => n.position).ToList();
        searchTree.Populate(pathfindingGridSO.nodes);
    }

    private void FixedUpdate()
    {
        EventBusInitialiser.instance.Bus.InvokeEvent(new PlayerPosRequestEvent
        {
            positionListener = this
        });
    }

    private void OnDrawGizmos()
    {
        if(!pathfindingGridSO) return;
        
        Gizmos.color = new Color(0.8f,0.2f,0.2f);
        if(drawNodes || drawNodesConnections)
        {
            foreach (PathfindingNode node in pathfindingGridSO.nodes)
            {
                if(drawNodes) Gizmos.DrawSphere(node.position, 0.02f);
                if(drawNodesConnections)
                {
                    foreach (int n in node.neighborsIndex)
                    {
                        Gizmos.DrawLine(node.position, pathfindingGridSO.nodes[n].position);
                    }
                }
            }
        }

        if (searchTree != null)
        {
            PathfindingNode playerNode = null;
            PathfindingNode startingNode = null;
            using (findClosestNodeMarker.Auto())
            {
                playerNode = searchTree.FindClosest(playerPosition).node;
                startingNode = searchTree.FindClosest(starTransform.position).node;
            }
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(playerNode.position, 0.025f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(startingNode.position, 0.025f);
            
            List<PathfindingNode> path = _aStarAlgorithm.FindPathFromGrid(pathfindingGridSO.nodes, startingNode,  playerNode);
            Gizmos.color = Color.cyan;
            for(int i = 0; i < path.Count-1; i++)
            {
                Gizmos.DrawLine(path[i].position, path[i+1].position);
            }
        }
        
    }

    public void OnPlayerMoving(Vector3 playerPos)
    {
        playerPosition = playerPos;
    }
}
