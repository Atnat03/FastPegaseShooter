using System;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class PlayerPositionCaster : NetworkBehaviour
{
    [SerializeField] private float _movementCastingThreshold = 0.5f;
    private Transform _playerTransform;
    private Vector3 _playerPosition;
    
    

    public override void OnStartClient()
    {
        base.OnStartClient();
        _playerTransform = transform;
        _playerPosition = _playerTransform.position;
        
        _playerPosition = _playerTransform.position;
        PlayerPositionCastingServerRPC(_playerPosition);
    }

    private void FixedUpdate()
    {
        if (_playerTransform && (_playerTransform.position - _playerPosition).sqrMagnitude >
            _movementCastingThreshold * _movementCastingThreshold)
        {
            _playerPosition = _playerTransform.position;
            PlayerPositionCastingServerRPC(_playerPosition);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PlayerPositionCastingServerRPC(Vector3 position, NetworkConnection conn = null)
    {
        PlayerPositionCastingObserverRPC(position, conn.ClientId);
    }
    [ObserversRpc]
    void PlayerPositionCastingObserverRPC(Vector3 position, int playerId)
    {
        EventBusInitialiser.instance.Bus.InvokeEvent(new PlayerPositionUpdate(playerId, position));
    }

    private void OnDrawGizmos()
    {
        if(Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_playerPosition, _movementCastingThreshold);
        }
    }
}

public struct PlayerPositionUpdate
{
    public int p_playerId;
    public Vector3 p_playerPosition;

    public PlayerPositionUpdate(int playerId, Vector3 playerPos)
    {
        p_playerId = playerId;
        p_playerPosition = playerPos;
    }
}
