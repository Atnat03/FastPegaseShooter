using FishNet.Object;
using UnityEngine;

public class GenericTrigger : NetworkBusListener
{
    [SerializeField] private int _triggerId;
    [SerializeField] private bool _activateOnce = true;
    
    private bool _activated;
    
    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !IsServerInitialized || (_activated && _activateOnce)) return;

        _activated = true;
        TriggerActionObserverRpc();
    }

    [ObserversRpc]
    private void TriggerActionObserverRpc()
    {
        InvokeEvent(new GenericTriggerEvent(_triggerId));
    }
}

public struct GenericTriggerEvent
{
    public int p_Id;

    public GenericTriggerEvent(int id)
    {
        p_Id = id;
    }
}
