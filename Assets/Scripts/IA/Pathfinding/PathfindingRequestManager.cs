using System;
using System.Collections.Generic;
using FishNet;
using UnityEngine;

public class PathfindingRequestManager : MonoBehaviour
{
    [SerializeField] private int _maxRequestsPerFrame = 2;
    [SerializeField] private float _tolerableDistanceThreshold = 0.5f;
    
    private Queue<PathRequest> _requests = new Queue<PathRequest>();
    
    /// <summary>
    /// Add a pathRequest, previous check needs to be done to prevent registering multiple time the same request
    /// </summary>
    public bool TryRegisterPathRequest(PathRequest pathRequest)
    {
        if(pathRequest._sqrDistanceEndNodeToTarget > _tolerableDistanceThreshold * _tolerableDistanceThreshold)
        {
            _requests.Enqueue(pathRequest);
            return true;
        }
        
        return false;
    }

    void Update()
    {
        if(!InstanceFinder.IsServerStarted)return;
        ProcessRequests();
    }

    void ProcessRequests()
    {
        for (int i = 0; i < _maxRequestsPerFrame && _requests.Count > 0; i++)
        {
            _requests.Dequeue().p_authorizePathRequest?.Invoke();
        }
    }
}

public struct PathRequest
{
    public float _sqrDistanceEndNodeToTarget;
    public Action p_authorizePathRequest;

    public PathRequest(float sqrDistanceEndNodeToTarget, Action authorizePathRequest)
    {
        _sqrDistanceEndNodeToTarget = sqrDistanceEndNodeToTarget;
        p_authorizePathRequest = authorizePathRequest;
    }
}
