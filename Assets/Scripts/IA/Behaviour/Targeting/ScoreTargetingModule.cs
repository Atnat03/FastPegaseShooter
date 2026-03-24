using System;
using System.Collections.Generic;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class ScoreTargetingModule : EnemyTargetingModule
{
    [Header("Aggro")]
    [SerializeField] private int _aggroPointWhenInDetectZone;
    [SerializeField] private int _aggroPointPerDamageTaken;
    [SerializeField] private int _aggroPointPerSecond;
    [SerializeField] private int _aggroPointPerDamageDealed;
    
    [Header("Zones")]
    [SerializeField] private float _detectionZoneRadius;
    [SerializeField] private float _aggroZoneRadius;
    [SerializeField] private float _idealDistanceRadius;

    private HashSet<int> players = new HashSet<int>();
    private Dictionary<int, int> _playerAggroValue = new Dictionary<int, int>();
    private Dictionary<int, bool> _playerInDetectionZone = new Dictionary<int, bool>();

    private List<int> _playerToAdd = new List<int>();
    
    public override void OnNetworkTick()
    {
        base.OnNetworkTick();
        string text = "";
        foreach (var newEntry in _playerToAdd)
        {
            players.Add(newEntry);
        }
        _playerToAdd.Clear();
        
        foreach (int playerId in players)
        {
            if(!InstanceFinder.ClientManager.Objects.Spawned.TryGetValue(playerId, out NetworkObject playerObject))
            {
                Debug.LogError("player index wasn't found in dictionary");
            }
            
            float sqrDistance = (playerObject.transform.position-transform.position).sqrMagnitude;
            
            
            //Player in aggro zone
            if(sqrDistance <= _aggroZoneRadius * _aggroZoneRadius)
                    _playerAggroValue[playerId] += _aggroPointPerSecond;
            
            text += $"p{playerId}: {_playerAggroValue[playerId]}";
        }
        
        CustomLogger.HighlightLog(text);
    }

    protected override void OnPlayerPositionUpdate(PlayerPositionUpdateEvent PPUE)
    {
        if(!InstanceFinder.ClientManager.Objects.Spawned.TryGetValue(PPUE.p_networkObjectId, out NetworkObject playerObject))
        {
            Debug.LogError("player index wasn't found in dictionary");
        }
            
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

    public void OnHitPlayer(int playerId, int damages)
    {
        _playerAggroValue[playerId] += damages*_aggroPointPerDamageDealed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _idealDistanceRadius);
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, _aggroZoneRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionZoneRadius);
    }
}
