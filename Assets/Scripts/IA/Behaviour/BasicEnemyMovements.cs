using System;
using System.Collections.Generic;
using CustomConsole.Runtime.Logger;
using FishNet.Object;
using UnityEngine;

public class BasicEnemyMovements : NetworkBehaviour
{
    [SerializeField] private float _speed;
    
    private int _targetedPlayerId;
    private Vector3 _lastPlayerPosition;
    private EventBus _bus;
    
    private Transform _transform;


    //server side variables
    //path related variables
    private Guid _gridReaderId;
    private List<PathfindingNode> _path = new List<PathfindingNode>();
    private Vector3 _lastPos;
    private float _t;

    //private Action _unsubscribePPU;
    public override void OnStartClient()
    {
    }

    public override void OnStartServer()
    {
        _transform = transform;
        _bus = EventBusInitialiser.instance.Bus;
        /*_unsubscribePPU = _bus.Subscribe((PlayerPositionUpdateEvent PPUE) =>
        {
            OnPlayerMoving(PPUE.p_playerId, PPUE.p_playerPosition);
        });*/
    }

    /*public override void OnStopServer()
    {
        _unsubscribePPU?.Invoke();
    }*/
    
    public void SetGridReaderGuid(Guid gridReaderId) => _gridReaderId = gridReaderId;

    private void FixedUpdate()
    {
        if(IsServerInitialized)
        {
            
            
            //only the server can determine the enemies positions
            if (_path.Count > 1)
            {
                FollowPath();
            }
        }
    }

    public void OnPlayerMoving(int playerId, Vector3 playerPosition, PathfindingGridReader gridReader)
    {
        if (IsTargetPlayer(playerId) || IsPlayerCloser(playerPosition))
        {
            _targetedPlayerId = playerId;
            _lastPlayerPosition = playerPosition;
            //_bus.InvokeEvent(new PathRequestEvent(this, _gridReaderId, _transform.position, _lastPlayerPosition));
            
            //Updating Pathfinding
            _path = gridReader.GetPath(transform.position, playerPosition);
            _lastPos = transform.position;
            _t = 0;
        }
    }

    bool IsTargetPlayer(int playerId) => playerId == _targetedPlayerId;
    bool IsPlayerCloser(Vector3 playerPosition) => (transform.position - playerPosition).sqrMagnitude < (transform.position - _lastPlayerPosition).sqrMagnitude;

    /*public void OnPathAnswer(List<PathfindingNode> path)
    {
        _path = path;
        _lastPos = _transform.position;
        _t = 0;
    }*/
    
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

    /*private void OnDrawGizmos()
    {
        if (IsServerInitialized)
        {
            Gizmos.color = Color.green;
            foreach (PathfindingNode node in _path)
            {
                Gizmos.DrawWireSphere(node.position, 1f);
            }
        }
    }*/
}
/*public struct PathRequestEvent
{
    public IPathRequester p_requester;
    public Guid p_gridReaderId;
    public Vector3 p_startPosition;
    public Vector3 p_endPosition;

    public PathRequestEvent(IPathRequester requester, Guid gridReaderId, Vector3 startPosition, Vector3 endPosition)
    {
        p_requester = requester;
        p_gridReaderId = gridReaderId;
        p_startPosition = startPosition;
        p_endPosition = endPosition;
    }
}*/
