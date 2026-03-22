using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class BasicEnemyMelee : NetworkBehaviour
{
    [SerializeField] private float _maxPlayerDistance = 1.5f;
    [SerializeField] private float _attackDelay = 2f;
    [SerializeField] private int _damage = 10;

    private int _targetedPlayerId;
    Vector3 _lastPlayerPosition;
    private int _playerObjectId;
    private float _waitedTimeSinceAttack;
    
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
        _waitedTimeSinceAttack += (float)InstanceFinder.TimeManager.TickDelta;
        if (_waitedTimeSinceAttack >= _attackDelay && CanAttack())
        {
            _waitedTimeSinceAttack = 0;
            
            //Empty event for now
            if (InstanceFinder.ClientManager.Objects.Spawned.TryGetValue(_playerObjectId, out NetworkObject player))
            {
                EventBusInitialiser.instance.Bus.InvokeEvent(new EnemyMeleeAttack());
                EventBusInitialiser.instance.Bus.InvokeEvent(new PlayerTakeDamageEvent
                {
                    playerN = player,
                    value = _damage
                });
            }
        }
    }

    bool CanAttack()
    {
        if ((transform.position - _lastPlayerPosition).sqrMagnitude > _maxPlayerDistance * _maxPlayerDistance)
            return false;
            
        //only condition is to be close enough from the player
        return true;
    }

    Vector3 PlayerPosition() => InstanceFinder.ClientManager.Objects.Spawned[_playerObjectId].transform.position;

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying || !IsServerInitialized) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _maxPlayerDistance);
        Gizmos.DrawSphere(PlayerPosition(), 0.1f);
    }
}

public struct EnemyMeleeAttack
{
    
}
