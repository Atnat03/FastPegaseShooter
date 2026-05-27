using System;
using System.Collections.Generic;
using System.Linq;
using CustomConsole.Runtime.Logger;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(AStarAlgorithm))]
public class PathfindingGridReader : MonoBehaviour
{
    public Guid p_id;
    public PathfindingGridSO pathfindingGridSO;
    
    private ThreeDimensionalTree _searchTree;
    private AStarAlgorithm _aStarAlgorithm;
    
    public Dictionary<int, int> _additionalWalkingCost = new Dictionary<int, int>();
    [HideInInspector] public bool VariableChanged = true;
    
    private Dictionary<int, PathReservation> _reservations = new Dictionary<int, PathReservation>();
    private int _freeReservationId;

    /*[SerializeField] private int _playerPositionWeight = 50;
    [SerializeField] private int _playerPositionSpread = 3;
    private int playerRequestReservationId;*/

    private void Start()
    {
        _searchTree = new ThreeDimensionalTree();
        _searchTree.Populate(pathfindingGridSO.nodes);

        _aStarAlgorithm = GetComponent<AStarAlgorithm>();
        
        _aStarAlgorithm.Init();
        
        p_id = GenerateNewGuid();
        
        /*ListenToEvent((PlayerPositionUpdateEvent PPUE) =>
        {
            PathfindingNode node = _searchTree.FindClosest(PPUE.p_playerPosition).node;
            
            if(_reservations.ContainsKey(playerRequestReservationId))
            {
                if (node.position !=
                    pathfindingGridSO.nodes[_reservations[playerRequestReservationId].p_path[0]].position)
                {
                    ClearPathReservation(playerRequestReservationId);

                    playerRequestReservationId = RegisterPath(
                        new List<PathfindingNode> { node },
                        _playerPositionWeight, _playerPositionSpread);
                }
            }
            else
            {
                playerRequestReservationId = RegisterPath(
                    new List<PathfindingNode> { node },
                    _playerPositionWeight, _playerPositionSpread);
            }
        });*/
    }

    public void GetAndRegisterPath(Vector3 start, Vector3 end, int traceWeight, int traceSpread, out List<PathfindingNode> path, out int reservationId)
    {
        path = _aStarAlgorithm.FindPathFromGrid(pathfindingGridSO.nodes, _additionalWalkingCost, _searchTree.FindClosest(start).node, _searchTree.FindClosest(end).node);
        reservationId = RegisterPath(path, traceWeight, traceSpread);
    }

    private Queue<(int index, int dist)> _registerQueue = new Queue<(int, int)>();
    private HashSet<int> _registerVisitedNodes = new HashSet<int>();
    int RegisterPath(List<PathfindingNode> path, int traceWeight, int traceSpread)
    {
        _registerQueue.Clear();
        _registerVisitedNodes.Clear();
        List<int> _registerPathIndexs = new List<int>();

        foreach (PathfindingNode node in path)
        {
            _registerQueue.Enqueue((node.index, 0));
            _registerVisitedNodes.Add(node.index);
            _registerPathIndexs.Add(node.index);
        }

        while (_registerQueue.Count > 0)
        {
            (int current, int dist) = _registerQueue.Dequeue();

            if (dist > traceSpread) continue;
            
            int weight = Mathf.RoundToInt(traceWeight * (1f - (float)dist / traceSpread));

            if (!_additionalWalkingCost.ContainsKey(current) || _additionalWalkingCost[current] < weight)
            {
                _additionalWalkingCost[current] = weight;
            }

            foreach (int n in pathfindingGridSO.nodes[current].neighborsIndex)
            {
                if (_registerVisitedNodes.Contains(n)) continue;

                _registerVisitedNodes.Add(n);
                _registerQueue.Enqueue((n, dist + 1));
            }
        }

        PathReservation newReservation = new PathReservation(
            _freeReservationId,
            _registerPathIndexs,
            traceWeight,
            traceSpread);
        _freeReservationId ++;
        _reservations.Add(newReservation.p_reservationId, newReservation);
        VariableChanged = true;
        
        return newReservation.p_reservationId;
    }

    private Queue<(int index, int dist)> _clearQueue = new Queue<(int, int)>();
    private HashSet<int> _clearVisitedNodes = new HashSet<int>();
    public void ClearPathReservation(int id)
    {
        if(!_reservations.ContainsKey(id)) return;
        
        _clearQueue.Clear();
        _clearVisitedNodes.Clear();
        
        PathReservation reservation = _reservations[id];

        foreach (int node in reservation.p_path)
        {
            _clearQueue.Enqueue((node, 0));
            _clearVisitedNodes.Add(node);
        }

        while (_clearQueue.Count > 0)
        {
            (int current, int dist) = _clearQueue.Dequeue();

            if (dist > reservation.p_traceSpread) continue;
            
            int weight = Mathf.RoundToInt(reservation.p_traceWeight * (1f - (float)dist / reservation.p_traceSpread));

            if (_additionalWalkingCost.TryGetValue(current, out int existing))
            {
                if (existing <= weight)
                {
                    _additionalWalkingCost.Remove(current);
                }
                else
                {
                    //CustomLogger.CCErrorLog($"NO MATCH {current} existing:{existing} vs weight:{weight}");
                }
            }

            foreach (int n in pathfindingGridSO.nodes[current].neighborsIndex)
            {
                if (_clearVisitedNodes.Contains(n)) continue;

                _clearVisitedNodes.Add(n);
                _clearQueue.Enqueue((n, dist + 1));
            }
        }
        _reservations.Remove(id);
        VariableChanged = true;
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