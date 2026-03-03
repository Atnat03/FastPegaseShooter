using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;

public class PathfindingGridReader : MonoBehaviour
{
    public PathfindingGridSO pathfindingGridSO;

    public ThreeDimensionalTree searchTree;

    [SerializeField] private AStarAlgorithm _aStarAlgorithm;
    
    //Debug Variables
    [SerializeField] private Transform starTransform;
    [HideInInspector] public bool drawNodes = true;
    [HideInInspector] public bool drawNodesConnections = true;
    //

    private void Start()
    {
        searchTree = new ThreeDimensionalTree();
        List<Vector3> values = pathfindingGridSO.nodes.Select(n => n.position).ToList();
        searchTree.Populate(pathfindingGridSO.nodes);

        EventBusInitialiser.instance.Bus.Subscribe((PathRequestEvent PRE) =>
        {
            PRE.p_requester.RequestPath(
                _aStarAlgorithm.FindPathFromGrid(
                    pathfindingGridSO.nodes,
                    searchTree.FindClosest(PRE.p_startPosition).node,
                    searchTree.FindClosest(PRE.p_endPosition).node));
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
    }
}
