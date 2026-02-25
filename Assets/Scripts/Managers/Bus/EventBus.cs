using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus
{
    private readonly Dictionary<Type, List<Delegate>>  _handlers = new Dictionary<Type, List<Delegate>>();

    public Action Subscribe<T>(Action<T> handler) where T : struct
    {
        Type type = typeof(T);
        if (!_handlers.ContainsKey(type))
        {
            _handlers[type] = new List<Delegate>();
        }
        
        _handlers[type].Add(handler);
        return () => Unsubscribe(handler);
    }
    
    public void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        if (_handlers.ContainsKey(typeof(T)))
        {
            _handlers[typeof(T)].Remove(handler);
        }
    }
    
    public void InvokeEvent<T>(T invokedEvent) where T : struct
    {
        if(!_handlers.TryGetValue(typeof(T), out List<Delegate> subscribers)) return;

        foreach (Delegate subscriber in subscribers)
            ((Action<T>)subscriber).Invoke(invokedEvent);
    }
}
/*public struct ShootingEventData
{
    public bool test;
    public int test2;
}

public class ShootingService
{
    Action unsubscribingAction;

    private EventBus _bus;
    public ShootingService(EventBus bus)
    {
        _bus = bus;
    }

    void Start()
    {

        Action<ShootingEventData> onShoot = ShootingFeedBack;
        unsubscribingAction = _bus.Subscribe(onShoot);
        
        _bus.Unsubscribe(onShoot);
    }
    public void ShootingFeedBack(ShootingEventData data)
    {
        
        unsubscribingAction?.Invoke();
    }
    
    
    public void Shooting()
    {

        ShootingEventData shootingEvent = new ShootingEventData
        {
            test = true,
            test2 = 2
        };
       _bus.InvokeEvent(shootingEvent);
    }
}*/
