using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

public class DoublePlayerTriggerZoneServerToClient : NetworkBehaviour
{
    [SerializeField] private UnityEvent _events;
    [SerializeField] private bool _activateOnce = true;
    
    private HashSet<int> _enteredPlayers = new HashSet<int>();
    private bool _activated;
    public void OnTriggerEnter(Collider other)
    {
        if(!InstanceFinder.IsServerStarted) return;
        if(_activateOnce && _activated) return;
        
        if (other.CompareTag("Player"))
        {
            PlayerVisuelBridge PVB = other.GetComponent<PlayerVisuelBridge>();
            if (!_enteredPlayers.Contains(PVB.OwnerId)) _enteredPlayers.Add(PVB.OwnerId);
            
            if(_enteredPlayers.Count > 1)
            {
                _activated = true;
                _enteredPlayers.Clear();
                TriggerActionObserverRpc();
            }
        }
    }

    [ObserversRpc]
    void TriggerActionObserverRpc()
    {
        _events?.Invoke();
    }
}
