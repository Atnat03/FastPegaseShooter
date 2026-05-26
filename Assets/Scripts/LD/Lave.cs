using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class Lave : NetworkBusListener
{
    [SerializeField] private float _damage = 10000000;
    [SerializeField] private float _timeTickDamage = 1;
    
    private Dictionary<NetworkObject, float> _playerTimers = new Dictionary<NetworkObject, float>();
    
    private void Update()
    {
        if (!IsServerInitialized) return;

        var keys = new List<NetworkObject>(_playerTimers.Keys);
        foreach (var key in keys)
        {
            if (_playerTimers[key] > 0)
                _playerTimers[key] -= Time.deltaTime;
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (!IsServerInitialized) return;

        if (other.TryGetComponent(out PlayerVisuelBridge player))
        {
            ApplyDamage(player.NetworkObject);
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        if (!IsServerInitialized) return;
        
        if (other.TryGetComponent(out PlayerVisuelBridge player))
        {
            _playerTimers.Remove(player.NetworkObject);
        }
    }

    void ApplyDamage(NetworkObject playerCollision)
    {
        if (!_playerTimers.ContainsKey(playerCollision))
            _playerTimers[playerCollision] = 0;

        if (_playerTimers[playerCollision] > 0) return;

        _playerTimers[playerCollision] = _timeTickDamage;

        InvokeEvent(new PlayerTakeDamageEvent
        {
            p_playerN = playerCollision,
            p_value = _damage,
            p_attacker = gameObject.GetComponent<NetworkObject>()
        });
    }
}