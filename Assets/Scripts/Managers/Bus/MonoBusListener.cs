using System;
using System.Collections.Generic;
using UnityEngine;

public class MonoBusListener : MonoBehaviour
{
    protected List<Action> _unsubscribeActions = new List<Action>();

    private EventBus _bus;

    public virtual void Awake()
    {
        _bus = EventBusInitialiser.instance.Bus;
    }

    protected void ListenToEvent<T>(Action<T> listeningAction) where T : struct
    {
        _unsubscribeActions.Add(_bus.Subscribe(listeningAction));
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
