using System;
using System.Collections.Generic;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;

[AddComponentMenu("EnemyBehaviour/Target/ScoreTargetModule")]
public class ScoreTargetModule : EnemyTargetModule
{
    [Header("Aggro")]
    [SerializeField] private int _aggroPointWhenInDetectZone;
    [SerializeField] private int _aggroPointPerDamageTaken;
    [SerializeField] private int _aggroPointPerSecond;
    [SerializeField] private int _aggroPointPerDamageDealt;
    [SerializeField] private List<int> _aggroPointsThreshold = new List<int>(){0,100,200};
    
    [Header("Zones")]
    [SerializeField] private float _detectionZoneRadius;
    [SerializeField] private float _aggroZoneRadius;
    

    private HashSet<int> _players = new HashSet<int>();
    private Dictionary<int, int> _playerAggroValue = new Dictionary<int, int>();
    private Dictionary<int, bool> _playerInDetectionZone = new Dictionary<int, bool>();

    private List<int> _playerToAdd = new List<int>();
    
    private float _timeSincePointAdded;
    private int _currentThreshold;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _currentThreshold = 0;
    }

    public override void OnNetworkTick(float tickDelta)
    {
        base.OnNetworkTick(tickDelta);
        
        _timeSincePointAdded += (float)InstanceFinder.TimeManager.TickDelta;
        
        foreach (var newEntry in _playerToAdd)
        {
            _players.Add(newEntry);
        }
        _playerToAdd.Clear();
        
        foreach (int playerId in _players)
        {
            if(!InstanceFinder.ClientManager.Objects.Spawned.TryGetValue(playerId, out NetworkObject playerObject))
            {
                Debug.LogError("player index wasn't found in dictionary");
            }
            
            float sqrDistance = (playerObject.transform.position-transform.position).sqrMagnitude;
            
            
            //Player in aggro zone
            if(sqrDistance <= _aggroZoneRadius * _aggroZoneRadius && _timeSincePointAdded > 1)
                    _playerAggroValue[playerId] += _aggroPointPerSecond;
            
            //switching aggroCheck
            if (_currentThreshold < _aggroPointsThreshold.Count &&
                _playerAggroValue[playerId] > _aggroPointsThreshold[_currentThreshold])
            {
                p_targetId = playerId;
                _currentThreshold++;
                if (InstanceFinder.ClientManager.Objects.Spawned.TryGetValue(playerId, out NetworkObject networkObject))
                {
                    p_lastTargetPosition = networkObject.transform.position;
                }
            }
            if (p_targetId == playerId && _playerAggroValue[playerId] == 0)
            {
                p_targetId = -1;
            }
            
            //text += $"p{playerId}: {_playerAggroValue[playerId]}";
        }

        if (p_targetId == -1) _currentThreshold = 0;

        if (_timeSincePointAdded > 1) _timeSincePointAdded = 0;
        //CustomLogger.HighlightLog(text);
    }

    protected override void OnPlayerPositionUpdate(PlayerPositionUpdateEvent PPUE)
    {
        if(!InstanceFinder.ClientManager.Objects.Spawned.TryGetValue(PPUE.p_networkObjectId, out NetworkObject playerObject))
        {
            Debug.LogError("player index wasn't found in dictionary");
        }
        
        p_onTargetPositionUpdate?.Invoke();
            
        float sqrDistance = (playerObject.transform.position-transform.position).sqrMagnitude;

        if (!_playerAggroValue.ContainsKey(PPUE.p_networkObjectId))
        {
            int aggroValue = sqrDistance > _detectionZoneRadius * _detectionZoneRadius
                ? 0
                : _aggroPointWhenInDetectZone;
            
            _playerAggroValue.Add(PPUE.p_networkObjectId, aggroValue);
            _playerInDetectionZone.Add(PPUE.p_networkObjectId, sqrDistance <= _detectionZoneRadius*_detectionZoneRadius);
            
            _playerToAdd.Add(PPUE.p_networkObjectId);
        }
        
        if (_playerInDetectionZone[PPUE.p_networkObjectId] && sqrDistance > _detectionZoneRadius*_detectionZoneRadius)
        {
            _playerAggroValue[PPUE.p_networkObjectId] -= _aggroPointWhenInDetectZone;
            _playerAggroValue[PPUE.p_networkObjectId] = Mathf.Max(_playerAggroValue[PPUE.p_networkObjectId], 0);
            _playerInDetectionZone[PPUE.p_networkObjectId] = false;
        }
        else if(!_playerInDetectionZone[PPUE.p_networkObjectId] && sqrDistance <= _detectionZoneRadius*_detectionZoneRadius)
        {
            _playerAggroValue[PPUE.p_networkObjectId] += _aggroPointWhenInDetectZone;
            _playerInDetectionZone[PPUE.p_networkObjectId] = true;
        }
    }

    public override bool HasTarget()
    {
        return p_targetId >= 0;
    }

    public void OnHitPlayer(int playerId, int damages)
    {
        if(_playerAggroValue.ContainsKey(playerId))
            _playerAggroValue[playerId] += damages*_aggroPointPerDamageDealt;
        else 
            _playerAggroValue.Add(playerId, damages*_aggroPointPerDamageDealt);
    }
    public void OnDamageTaken(int playerId, int damages)
    {
        if(_playerAggroValue.ContainsKey(playerId))
            _playerAggroValue[playerId] += damages*_aggroPointPerDamageTaken;
        else 
            _playerAggroValue.Add(playerId, damages*_aggroPointPerDamageTaken);
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, _aggroZoneRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionZoneRadius);
    }*/
}
