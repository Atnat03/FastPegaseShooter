using System;
using System.Collections.Generic;
using CustomConsole.Runtime.Logger;
using UnityEngine;

//##########
//Script Broken by removal of "IPlayerPositionListener" interface
//##########
public class DumbEnemy : MonoBehaviour, /*IPlayerPositionListener,*/ IPathRequester
{
    [SerializeField] private float playerPositionUpdateThreshold;
    [SerializeField] private float _speed;
    
    Vector3 _playerPosition;
    private List<PathfindingNode> _path;
    private EventBus _bus;

    private Vector3 _lastPos;
    private float _t;

    private void Start()
    {
        _bus = EventBusInitialiser.instance.Bus;
    }

    //##########
    //Function Broken by removal of "PlayerPosRequestEvent" struct
    //##########
    private void FixedUpdate()
    {
        /*_bus.InvokeEvent(new PlayerPosRequestEvent{positionListener = this});
        if(_path.Count > 1) FollowPlayer();*/
        CustomLogger.CCErrorLog($"The script {typeof(DumbEnemy)} attached to {transform.name} isn't valid anymore. No behaviour remaining");
    }

    public void OnPlayerMoving(Vector3 playerPosition)
    {
        //##########
        //Script Broken by changement in Struct "PathRequestEvent" signature
        //##########
        
        /*if((playerPosition - _playerPosition).sqrMagnitude > playerPositionUpdateThreshold * playerPositionUpdateThreshold)
        {
            _playerPosition = playerPosition;
            //requesting for new path
            _bus.InvokeEvent(new PathRequestEvent(this, transform.position));
        }*/
    }

    void FollowPlayer()
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

    public void OnPathAnswer(List<PathfindingNode> path)
    {
        _path = path;
        _lastPos = transform.position;
        _t = 0;
    }
}
