using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class GenericTriggerDouble : NetworkBusListener
{
    [SerializeField] private int _triggerId;
    [SerializeField] private bool _activateOnce = true;
    
    private HashSet<int> _enteredPlayers = new HashSet<int>();

    private bool _activated;

    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !IsServerInitialized || (_activated && _activateOnce)) return;

        PlayerVisuelBridge PVB = other.GetComponent<PlayerVisuelBridge>();
        if (PVB == null) return;
        if (!_enteredPlayers.Contains(PVB.OwnerId)) _enteredPlayers.Add(PVB.OwnerId);
            
        if(_enteredPlayers.Count > 1)
        {
            _activated = true;
            _enteredPlayers.Clear();
            TriggerActionObserverRpc();
        }
    }

    [ObserversRpc]
    private void TriggerActionObserverRpc()
    {
        InvokeEvent(new GenericTriggerEvent(_triggerId));
    }
}
