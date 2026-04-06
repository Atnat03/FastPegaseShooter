using System;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerPositionCaster : NetworkBusListener
{
    [SerializeField] private float _castingBeatDelay = 0.7f;
    [SerializeField] private float _castingPhysicalThreshold = 0.5f;
    private Transform _playerTransform;
    private NetworkObject _networkObject;
    private Vector3 _playerPosition;
    
    private float _castingBeatTimer;

    public override void OnStartClient()
    {
        base.OnStartClient();
        _playerTransform = transform;
        _playerPosition = _playerTransform.position;
        _networkObject = GetComponentInParent<NetworkObject>();
        
        _playerPosition = _playerTransform.position;
        PlayerPositionCastingServerRPC(_playerPosition);
    }

    private void FixedUpdate()
    {
        _castingBeatTimer += Time.fixedDeltaTime;
        if (_playerTransform &&
            ((_playerTransform.position - _playerPosition).sqrMagnitude > _castingPhysicalThreshold * _castingPhysicalThreshold) ||
            _castingBeatTimer >= _castingBeatDelay)
        {
            _castingBeatTimer = 0;
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
        InvokeEvent(new PlayerPositionUpdateEvent(playerId, position, _networkObject.ObjectId));
    }

    private void OnDrawGizmos()
    {
        if(Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_playerPosition, _castingPhysicalThreshold);
        }
    }
}

public struct PlayerPositionUpdateEvent
{
    public int p_playerId;
    public Vector3 p_playerPosition;
    public int p_networkObjectId;

    public PlayerPositionUpdateEvent(int playerId, Vector3 playerPos, int networkObjectId)
    {
        p_playerId = playerId;
        p_playerPosition = playerPos;
        p_networkObjectId = networkObjectId;
    }
}
