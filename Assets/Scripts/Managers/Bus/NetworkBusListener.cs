using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public abstract class NetworkBusListener : NetworkBehaviour
{
    protected List<Action> _unsubscribeActions = new List<Action>();


    protected void ListenToEvent<T>(Action<T> listeningAction) where T : struct
    {
        _unsubscribeActions.Add(EventBus.Subscribe(listeningAction));
    }

    protected void InvokeEvent<T>(T newEvent) where T : struct
    {
        EventBus.InvokeEvent(newEvent);
    }

    public override void OnStopServer()
    {
        UnsubscribeAll();
        base.OnStopServer();
    }

    protected virtual void OnDestroy()
    {
        UnsubscribeAll();
    }

    private void UnsubscribeAll()
    {
        foreach (Action unsubscribeAction in _unsubscribeActions)
            unsubscribeAction?.Invoke();
    }
}
