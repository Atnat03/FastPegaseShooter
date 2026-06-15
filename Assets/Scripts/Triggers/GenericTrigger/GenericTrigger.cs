using CustomConsole.Runtime.Logger;
using FishNet.Object;
using UnityEngine;

public class GenericTrigger : NetworkBusListener
{
    [SerializeField] private int _triggerId;

    private bool _activated;

    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if(!IsServerInitialized) return;

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
