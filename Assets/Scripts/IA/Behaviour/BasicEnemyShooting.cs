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
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _maxAmmoLifeTime = 10f;

    private int _targetedPlayerId;
    Vector3 _lastPlayerPosition;
    private int _playerObjectId;
    private float _waitedTimeSinceShoot;
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        EventBusInitialiser.instance.Bus.Subscribe((PlayerPositionUpdateEvent PPUE) =>
        {
            if (IsTargetPlayer(PPUE.p_playerId) || IsPlayerCloser(PPUE.p_playerPosition))
            {
                _lastPlayerPosition = PPUE.p_playerPosition;
                _playerObjectId = PPUE.p_networkObjectId;
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

            Vector3 delta = PlayerPosition() - transform.position;
            float length = delta.magnitude;
            Vector3 dir = delta / length;
            
            EventBusInitialiser.instance.Bus.InvokeEvent(new EnemyShootingEvent
            {
                p_startPos = transform.position + dir * 0.1f + Vector3.up * 0.5f,
                p_direction = dir,
                p_speed = _ammoSpeed,
                p_damage = _damage,
                p_aliveTime = _maxAmmoLifeTime
            });
        }
    }

    bool CanShoot()
    {
        if ((transform.position - _lastPlayerPosition).sqrMagnitude > _maxPlayerDistance * _maxPlayerDistance)
            return false;
        
        
        Vector3 delta = PlayerPosition() - transform.position;
        float length = delta.magnitude;
        Vector3 dir = delta / length;

        Vector3 origin = transform.position + dir * 0.1f + Vector3.up * 0.5f;
        Debug.DrawLine(origin,origin + dir * length, Color.red, _shootingDelay);
        if (Physics.Raycast(origin, dir, out RaycastHit hit, length, LayerMask.GetMask("Owner", "Other"), QueryTriggerInteraction.Ignore))
        {
            CustomLogger.ImportantLog($"{hit.collider.name}");
            if (hit.collider.CompareTag("Player"))
            {
                CustomLogger.ImportantLog($"Can shoot third check");
                return true;
            }
        }
        
        return false;
    }

    Vector3 PlayerPosition() => InstanceFinder.ClientManager.Objects.Spawned[_playerObjectId].transform.position;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _maxPlayerDistance);
        Gizmos.DrawSphere(PlayerPosition(), 0.1f);
    }
}

public struct EnemyShootingEvent
{
    public Vector3 p_startPos;
    public Vector3 p_direction;
    public float p_speed;
    public int p_damage;
    public float p_aliveTime;
}
