using System;
using System.Collections.Generic;
using FishNet;
using UnityEngine;

public class PathfindingRequestManager : MonoBehaviour
{
    [SerializeField] private int _maxRequestsPerFrame = 2;
    
    private Queue<PathRequest> _requests = new Queue<PathRequest>();
    
    /// <summary>
    /// Add a pathRequest, previous check needs to be done to prevent registering multiple time the same request
    /// </summary>
    public void RegisterPathRequest(PathRequest pathRequest)
    {
        _requests.Enqueue(pathRequest);
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
            _requests.Dequeue().p_AuthorizePathRequest?.Invoke();
        }
    }
}

public struct PathRequest
{
    public Action p_AuthorizePathRequest;
}
