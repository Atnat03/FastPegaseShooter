using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PathfindingGridReader : MonoBehaviour, IPlayerPositionListener
{
    public PathfindingGridSO pathfindingGridSO;

    public ThreeDimensionalTree searchTree;
    
    //Debug Variables
    [HideInInspector] public bool drawNodes = true;
    [HideInInspector] public bool drawNodesConnections = true;
    Vector3 playerPosition;
    static ProfilerMarker findClosestNodeMarker = new ProfilerMarker("FindClosestNode");
    //

    private void Start()
    {
        searchTree = new ThreeDimensionalTree();
        List<Vector3> values = pathfindingGridSO.nodes.Select(n => n.position).ToList();
        searchTree.Populate(values);
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
            Gizmos.color = Color.blue;
            Vector3 position = Vector3.zero;
            using (findClosestNodeMarker.Auto())
            {
                position = searchTree.FindClosest(playerPosition).value;
            }
            Gizmos.DrawSphere(position, 0.025f);
        }
    }

    public void OnPlayerMoving(Vector3 playerPos)
    {
        playerPosition = playerPos;
    }
}
