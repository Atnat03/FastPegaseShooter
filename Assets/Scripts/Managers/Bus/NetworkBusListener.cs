using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public abstract class NetworkBusListener : NetworkBehaviour
{
    protected List<Action> _unsubscribeActions = new List<Action>();

    private EventBus _bus;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _bus = EventBusInitialiser.instance.Bus;
    }
    

    protected void ListenToEvent<T>(Action<T> listeningAction) where T : struct
    {
        _unsubscribeActions.Add(_bus.Subscribe(listeningAction));
    }

    protected void InvokeEvent<T>(T newEvent) where T : struct
    {
        _bus.InvokeEvent(newEvent);
    }

    public override void OnStopServer()
    {
        UnsubscribeAll();
        base.OnStopServer();
    }

    protected void OnDestroy()
    {
        UnsubscribeAll();
    }

    private void UnsubscribeAll()
    {
        foreach (Action unsubscribeAction in _unsubscribeActions)
            unsubscribeAction?.Invoke();
    }
}
