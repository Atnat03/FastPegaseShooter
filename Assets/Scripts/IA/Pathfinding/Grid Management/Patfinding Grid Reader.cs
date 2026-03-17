using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;

[RequireComponent(typeof(AStarAlgorithm))]
public class PathfindingGridReader : MonoBehaviour
{
    public Guid p_id;
    public PathfindingGridSO pathfindingGridSO;

    public ThreeDimensionalTree searchTree;

    private AStarAlgorithm _aStarAlgorithm;
    
    //Debug Variables
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
            if(PRE.p_gridReaderId != p_id) return;
            PRE.p_requester.OnPathAnswer(
                _aStarAlgorithm.FindPathFromGrid(
                    pathfindingGridSO.nodes,
                    searchTree.FindClosest(PRE.p_startPosition).node,
                    searchTree.FindClosest(PRE.p_endPosition).node));
        });

        _aStarAlgorithm = GetComponent<AStarAlgorithm>();
    }

    private void OnDrawGizmos()
    {
        if(!pathfindingGridSO) return;
        
        Gizmos.color = new Color(0.8f,0.2f,0.2f);
        if(drawNodes || drawNodesConnections)
        {
            foreach (PathfindingNode node in pathfindingGridSO.nodes)
            {
                if(drawNodes) Gizmos.DrawSphere(node.position, 0.2f);
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
    
    #region Id
    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        if (p_id == Guid.Empty || IsGuidInScene(this))
        {
            p_id = GenerateNewGuid();
        }
    }
    
    Guid GenerateNewGuid() => Guid.NewGuid();

    bool IsGuidInScene(PathfindingGridReader self)
    {
        PathfindingGridReader[] spawnZones = FindObjectsByType<PathfindingGridReader>(FindObjectsSortMode.None);
        foreach (PathfindingGridReader gridReader in spawnZones)
        {
            if(gridReader != self && gridReader.p_id == self.p_id) return true;
        }
        return false;
    }
    #endif
    #endregion
}
