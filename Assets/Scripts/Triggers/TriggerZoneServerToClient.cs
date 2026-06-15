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
        if (!other.CompareTag("Player")) return;

        if (IsServerInitialized)
        {
            TriggerOnServer();
        }
        else if (IsClientInitialized)
        {
            RequestTriggerServerRpc();
        }
    }

    public void DebugSkipLevel()
    {
        if (IsServerInitialized)
        {
            TriggerOnServer();
        }
        else if (IsClientInitialized)
        {
            RequestTriggerServerRpc();
        }
    }
    

    [ServerRpc(RequireOwnership = false)]
    private void RequestTriggerServerRpc()
    {
        TriggerOnServer();
    }

    [Server]
    private void TriggerOnServer()
    {
        if (_activateOnce && _activated) return;

        _activated = true;
        _serverEvents?.Invoke();
        TriggerActionObserverRpc();
    }

    [ObserversRpc]
    private void TriggerActionObserverRpc()
    {
        _clientEvents?.Invoke();
    }
}