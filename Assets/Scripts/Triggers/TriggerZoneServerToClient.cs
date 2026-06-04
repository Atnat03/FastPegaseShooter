using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

public class TriggerZoneServerToClient : NetworkBehaviour
{
    [SerializeField] private UnityEvent _events;
    [SerializeField] private bool _activateOnce = true;

    private bool _activated;
    public void OnTriggerEnter(Collider other)
    {
        if(!InstanceFinder.IsServerStarted) return;
        if(_activateOnce && _activated) return;
        
        if (other.CompareTag("Player"))
        {
            _activated = true;
            TriggerActionObserverRpc();
        }
    }

    [ObserversRpc]
    void TriggerActionObserverRpc()
    {
        _events?.Invoke();
    }
}
