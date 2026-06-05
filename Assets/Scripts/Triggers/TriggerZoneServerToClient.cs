using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

public class TriggerZoneServerToClient : NetworkBehaviour
{
    [SerializeField] private UnityEvent _clientEvents;
    [SerializeField] private UnityEvent _serverEvents;
    [SerializeField] private bool _activateOnce = true;

    private bool _activated;
    public void OnTriggerEnter(Collider other)
    {
        if(!InstanceFinder.IsServerStarted) return;
        if(_activateOnce && _activated) return;
        
        if (other.CompareTag("Player"))
        {
            CustomLogger.HighlightLog($"[SubArena]Trigger on server={InstanceFinder.IsServerStarted} client={InstanceFinder.IsClientStarted}");
            _activated = true;
            _serverEvents?.Invoke();
            TriggerActionObserverRpc();
        }
    }

    [ObserversRpc]
    void TriggerActionObserverRpc()
    {
        _clientEvents?.Invoke();
    }
}
