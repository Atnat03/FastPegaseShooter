using System;
using System.Collections.Generic;
using CustomConsole.Runtime.Logger;
using FishNet.Object;
using UnityEngine;

public class BasicEnemyMovements : NetworkBehaviour, IPathRequester
{
    [SerializeField] private float _speed;
    
    private int _targetedPlayerId;
    private Vector3 _lastPlayerPosition;
    private EventBus _bus;
    
    private Transform _transform;


    //server side variables
    [SerializeField] private float _syncRate = 0.5f;
    private float _visualSyncingTimer;
        //path related variables 
    private List<PathfindingNode> _path = new List<PathfindingNode>();
    private Vector3 _lastPos;
    private float _t;
    
    //movement related variables (client side)
    private Vector3 _targetPosition;
    
    #region Server Side

    public override void OnStartClient()
    {
        _transform = transform;
    }

    public override void OnStartServer()
    {
        _bus = EventBusInitialiser.instance.Bus;
        _bus.Subscribe((PlayerPositionUpdate PPU) =>
        {
            OnPlayerMoving(PPU.p_playerId, PPU.p_playerPosition);
        });
    }

    private void FixedUpdate()
    {
        if(IsServerInitialized)
        {
            //only the server can determine the enemies positions
            if (_path.Count > 1)
            {
                FollowPath();
                
                _visualSyncingTimer += Time.deltaTime;
                if(_visualSyncingTimer >= _syncRate)
                {
                    Debug.Log("Syncing visuals");
                    _visualSyncingTimer = 0;
                    UpdatePositionObserverRPC(transform.position);
                }
            }
        }
        else
        {
            //smoothing movements on client side
            transform.position = Vector3.Lerp(transform.position, _targetPosition, 10f * Time.deltaTime);
        }
    }

    void OnPlayerMoving(int playerId, Vector3 playerPosition)
    {
        Debug.Log("test");
        if (IsTargetPlayer(playerId) || IsPlayerCloser(playerPosition))
        {
            _targetedPlayerId = playerId;
            _lastPlayerPosition = playerPosition;
            
            //Updating Pathfinding
            _bus.InvokeEvent(new PathRequestEvent(this, _transform.position, _lastPlayerPosition));
        }
    }

    bool IsTargetPlayer(int playerId) => playerId == _targetedPlayerId;
    bool IsPlayerCloser(Vector3 playerPosition) => (transform.position - playerPosition).sqrMagnitude < (transform.position - _lastPlayerPosition).sqrMagnitude;

    public void RequestPath(List<PathfindingNode> path)
    {
        _path = path;
        _lastPos = _transform.position;
        _t = 0;
    }
    
    void FollowPath()
    {
        transform.position = Vector3.Lerp(_lastPos, _path[^2].position, _t);
        _t += Time.deltaTime * _speed;
        if (_t >= 1)
        {
            _t = 0;
            _path.RemoveAt(_path.Count - 1);
            if(_path.Count > 0) _lastPos = _path[^1].position;
        }
    }
    #endregion

    #region Client Side
    [ObserversRpc]
    public void UpdatePositionObserverRPC(Vector3 serverPosition)
    {
        UpdatePosition(serverPosition);
    }

    void UpdatePosition(Vector3 newPosition)
    {
        if (!IsServerInitialized)
        {
            _targetPosition = newPosition;
        }
    }
    #endregion
}
public struct PathRequestEvent
{
    public IPathRequester p_requester;
    public Vector3 p_startPosition;
    public Vector3 p_endPosition;

    public PathRequestEvent(IPathRequester requester, Vector3 startPosition, Vector3 endPosition)
    {
        p_requester = requester;
        p_startPosition = startPosition;
        p_endPosition = endPosition;
    }
}
