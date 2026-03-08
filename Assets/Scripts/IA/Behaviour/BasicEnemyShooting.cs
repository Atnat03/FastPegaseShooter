using System;
using CustomConsole.Runtime.Logger;
using FishNet;
using UnityEngine;
using FishNet.Object;


public class BasicEnemyShooting : NetworkBehaviour
{
    [SerializeField] private float _maxPlayerDistance = 10f;
    [SerializeField] private float _shootingDelay = 2f;
    [SerializeField] private float _ammoSpeed;

    private int _targetedPlayerId;
    Vector3 _lastPlayerPosition;
    private float _waitedTimeSinceShoot;
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        EventBusInitialiser.instance.Bus.Subscribe((PlayerPositionUpdate PPU) =>
        {
            if (IsTargetPlayer(PPU.p_playerId) || IsPlayerCloser(PPU.p_playerPosition))
            {
                _lastPlayerPosition = PPU.p_playerPosition;
            }
        });
        InstanceFinder.TimeManager.OnTick += OnNetworkTick;
    }
    bool IsTargetPlayer(int playerId) => playerId == _targetedPlayerId;
    bool IsPlayerCloser(Vector3 playerPosition) => (transform.position - playerPosition).sqrMagnitude < (transform.position - _lastPlayerPosition).sqrMagnitude;

    public override void OnStopServer()
    {
        InstanceFinder.TimeManager.OnTick -= OnNetworkTick;
        
        base.OnStopServer();
    }

    private void OnNetworkTick()
    {
        _waitedTimeSinceShoot += (float)InstanceFinder.TimeManager.TickDelta;
        if (_waitedTimeSinceShoot >= _shootingDelay && CanShoot())
        {
            _waitedTimeSinceShoot = 0;

            Vector3 delta = _lastPlayerPosition - transform.position;
            float length = delta.magnitude;
            Vector3 dir = delta / length;
            
            EventBusInitialiser.instance.Bus.InvokeEvent(new EnemyShootingEvent
            {
                p_startPos = transform.position + dir * 0.1f + Vector3.up * 0.5f,
                p_direction = dir,
                p_speed = _ammoSpeed
            });
        }
    }

    bool CanShoot()
    {
        if ((transform.position - _lastPlayerPosition).sqrMagnitude > _maxPlayerDistance * _maxPlayerDistance)
            return false;
        
        Vector3 delta = _lastPlayerPosition - transform.position;
        float length = delta.magnitude;
        Vector3 dir = delta / length;

        Debug.DrawLine(transform.position + dir * 0.1f  + Vector3.up * 0.5f, dir * length, Color.red, _shootingDelay);
        if (Physics.Raycast(transform.position + dir * 0.1f  + Vector3.up * 0.5f, dir, out RaycastHit hit, length))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_lastPlayerPosition, 0.3f);
    }
}

public struct EnemyShootingEvent
{
    public Vector3 p_startPos;
    public Vector3 p_direction;
    public float p_speed;
}
