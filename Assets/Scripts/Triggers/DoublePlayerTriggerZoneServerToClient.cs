using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

public class DoublePlayerTriggerZoneServerToClient : NetworkBehaviour
{
    [SerializeField] private UnityEvent _clientEvents;
    [SerializeField] private UnityEvent _serverEvents;
    [SerializeField] private bool _activateOnce = true;
    
    [Header("Debug")]
    [SerializeField] private bool _forceActivationWithSinglePlayer;
    
    private HashSet<int> _enteredPlayers = new HashSet<int>();
    private bool _activated;
    
    
    #if !UNITY_EDITOR
    void OnServerInitialized()
    {
       _forceActivationWithSinglePlayer = false;
    }   
    #endif
    
    public void OnTriggerEnter(Collider other)
    {
        if(!InstanceFinder.IsServerStarted) return;
        if(_activateOnce && _activated) return;
        
        if (other.CompareTag("Player"))
        {
            PlayerVisuelBridge PVB = other.GetComponent<PlayerVisuelBridge>();
            if (PVB == null) return;
            if (!_enteredPlayers.Contains(PVB.OwnerId)) _enteredPlayers.Add(PVB.OwnerId);
            
            if(_forceActivationWithSinglePlayer || _enteredPlayers.Count > 1)
            {
                _activated = true;
                _enteredPlayers.Clear();
                _serverEvents?.Invoke();
                TriggerActionObserverRpc();
            }
        }
    }

    [ObserversRpc]
    void TriggerActionObserverRpc()
    {
        _clientEvents?.Invoke();
    }
}
