using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerPositionCaster : NetworkBusListener
{
    [SerializeField] private float _castingBeatDelay = 0.7f;
    [SerializeField] private float _castingPhysicalThreshold = 0.5f;
    private NetworkObject _networkObject;
    private Vector3 _playerPosition;
    
    [Header("Fake Target")]
    [SerializeField] private int _fakeTargetAmount = 5;
    [SerializeField, Range(0, 360)] private float _fakeSpreadAngle = 40;
    [SerializeField] private float _fakeTargetMaxDistance = 5;
    [SerializeField] private LayerMask _fakeTargetMask;
    
    
    private float _castingBeatTimer;
    private Dictionary<int, Vector3> _fakeTargetPositions = new Dictionary<int, Vector3>();
    private int askedIndexsAmount = -1;

    public override void OnStartClient()
    {
        base.OnStartClient();
        _playerPosition = transform.position;
        _networkObject = GetComponentInParent<NetworkObject>();
        
        _playerPosition = transform.position;
        PlayerPositionCastingServerRPC(_playerPosition, false);
        UpdateFakePositions();
    }

    private void FixedUpdate()
    {
        _castingBeatTimer += Time.fixedDeltaTime;
        if (transform &&
            ((transform.position - _playerPosition).sqrMagnitude > _castingPhysicalThreshold * _castingPhysicalThreshold) ||
            _castingBeatTimer >= _castingBeatDelay)
        {
            _playerPosition = transform.position;
            PlayerPositionCastingServerRPC(_playerPosition, _castingBeatTimer >= _castingBeatDelay);
            _castingBeatTimer = 0;
            UpdateFakePositions();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PlayerPositionCastingServerRPC(Vector3 position, bool isHeartBeat)
    {
        PlayerPositionCastingObserverRPC(position,isHeartBeat);
    }
    
    [ObserversRpc]
    void PlayerPositionCastingObserverRPC(Vector3 position, bool isHeartBeat)
    {
        InvokeEvent(new PlayerPositionUpdateEvent(_networkObject.OwnerId, position, _networkObject.ObjectId, isHeartBeat));
    }

    void UpdateFakePositions()
    {
        RaycastHit hit;
        for (int i = 0; i < _fakeTargetAmount; i++)
        {
            Vector3 dir = GetDirection(i);
            if (Physics.Raycast(transform.position, dir, out hit, _fakeTargetMaxDistance, _fakeTargetMask))
            {
                _fakeTargetPositions[i] = transform.position + dir * hit.distance;
            }
            else
            {
                _fakeTargetPositions[i] = transform.position + dir * _fakeTargetMaxDistance;
            }
        }
    }

    public int GetTargetIndex()
    {
        askedIndexsAmount++;
        return askedIndexsAmount % _fakeTargetAmount;
    }
    public Vector3 GetFakeTargetPosition(int targetIndex) => _fakeTargetPositions[targetIndex];
    
    public Vector3 GetDirection(int index)
    {
        Vector3 flatForward = Vector3.forward;
        flatForward.x = transform.forward.x;
        flatForward.z = transform.forward.z;
        flatForward.Normalize();
        
        float angle = 0f;

        if (_fakeTargetAmount > 1)
        {
            float divisor = Mathf.Approximately(_fakeSpreadAngle, 360f)
                ? _fakeTargetAmount
                : (_fakeTargetAmount - 1);

            angle = (
                -_fakeSpreadAngle * 0.5f
                + index * (_fakeSpreadAngle / divisor)
            ) * Mathf.Deg2Rad;
        }
        
        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);
        
        float x = flatForward.x * cosAngle - flatForward.z * sinAngle;
        float z = flatForward.x * sinAngle + flatForward.z * cosAngle;
        
        return new Vector3(x, 0, z).normalized;
    }

    
    private void OnDrawGizmos()
    {
        if(Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_playerPosition, _castingPhysicalThreshold);

            Gizmos.color = Color.orange;
            foreach (var fakeTarget in _fakeTargetPositions)
            {
                Gizmos.DrawLine(transform.position, fakeTarget.Value);
                Gizmos.DrawSphere(fakeTarget.Value, 0.1f);
            }
        }
        else
        {
            Gizmos.color = Color.orange;
            for (int i = 0; i < _fakeTargetAmount; i++)
            {
                Gizmos.DrawRay(transform.position, GetDirection(i) * _fakeTargetMaxDistance);
            }
        }
    }
}

public struct PlayerPositionUpdateEvent
{
    public int p_playerId;
    public Vector3 p_playerPosition;
    public int p_networkObjectId;
    public bool p_isHeartBeat;

    public PlayerPositionUpdateEvent(int playerId, Vector3 playerPos, int networkObjectId, bool isHeartBeat)
    {
        p_playerId = playerId;
        p_playerPosition = playerPos;
        p_networkObjectId = networkObjectId;
        p_isHeartBeat = isHeartBeat;
    }
}
