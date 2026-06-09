using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AStarAlgorithm))]
public class PathfindingGridReader : MonoBehaviour
{
    public Guid p_id;
    public PathfindingGridSO pathfindingGridSO;
    
    private ThreeDimensionalTree _searchTree;
    private AStarAlgorithm _aStarAlgorithm;
    
    //public Dictionary<int, int> _additionalWalkingCost = new Dictionary<int, int>();
    //[HideInInspector] public bool VariableChanged = true;

    private void Start()
    {
        _searchTree = new ThreeDimensionalTree();
        _searchTree.Populate(pathfindingGridSO.nodes);

        _aStarAlgorithm = GetComponent<AStarAlgorithm>();
        
        _aStarAlgorithm.Init();
        
        p_id = GenerateNewGuid();
        
        
    }

    public void GetPath(Vector3 start, Vector3 end, out List<PathfindingNode> path)
    {
        path = _aStarAlgorithm.FindPathFromGrid(pathfindingGridSO.nodes, _searchTree.FindClosest(start).node, _searchTree.FindClosest(end).node);
    }

    Guid GenerateNewGuid()
    {
        Guid guid = Guid.NewGuid();
        while (IsGuidInScene(this, guid))
        {
            guid = Guid.NewGuid();
        }
        return guid;
    }

    bool IsGuidInScene(PathfindingGridReader self, Guid id)
    {
        PathfindingGridReader[] spawnZones = FindObjectsByType<PathfindingGridReader>(FindObjectsSortMode.None);
        foreach (PathfindingGridReader gridReader in spawnZones)
        {
            if(gridReader != self && gridReader.p_id == id) return true;
        }
        return false;
    }
}

public struct PathReservation
{
    public int p_reservationId;
    public List<int> p_path;
    public int p_traceWeight;
    public int p_traceSpread;

    public PathReservation(int id, List<int> path, int weight, int spread)
    {
        p_reservationId = id;
        p_path = path;
        p_traceWeight = weight;
        p_traceSpread = spread;
    }
}