using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

public class Arena2TriggerAnimation : NetworkBusListener
{
    [SerializeField] private Arena2TriggerListener.TriggerType _eventType;

    private bool _activated;

    public void OnTriggerEnter(Collider other)
    {
        CustomLogger.ImportantLog("arena2 0");
        if (!other.CompareTag("Player")) return;
        CustomLogger.ImportantLog("arena2 0.5");
        if(!IsServerInitialized) return;
        CustomLogger.ImportantLog("arena2 0.75");

        TriggerActionObserverRpc();
    }

    [ObserversRpc]
    private void TriggerActionObserverRpc()
    {
        CustomLogger.ImportantLog("arena2 1");
        
        switch (_eventType)
        {
            case Arena2TriggerListener.TriggerType.Arena1:
                InvokeEvent(new OnArena2FirstEvent());
                break;
            case Arena2TriggerListener.TriggerType.Arena2:
                InvokeEvent(new OnArena2SecondEvent());
                break;
            case Arena2TriggerListener.TriggerType.Arena3:
                InvokeEvent(new OnArena2ThirdEvent());
                break;
        }
    }
}