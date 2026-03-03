using System;
using System.Collections.Generic;
using CustomConsole.Runtime.Logger;
using UnityEngine;

public class BasicEnemy : MonoBehaviour, IPathRequester
{
    [SerializeField] private float _speed;
    
    private int _targetedPlayerId;
    private Vector3 _lastPlayerPosition;
    private EventBus _bus;
    
    private Transform _transform;
    
    //path related variables
    private List<PathfindingNode> _path = new List<PathfindingNode>();
    private Vector3 _lastPos;
    private float _t;
    void Start()
    {
        _transform = transform;
        
        _bus = EventBusInitialiser.instance.Bus;
        _bus.Subscribe((PlayerPositionUpdate PPU) =>
        {
            OnPlayerMoving(PPU.p_playerId, PPU.p_playerPosition);
        });
    }

    private void FixedUpdate()
    {
        if(_path.Count > 1) FollowPath();
    }

    void OnPlayerMoving(int playerId, Vector3 playerPosition)
    {
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
