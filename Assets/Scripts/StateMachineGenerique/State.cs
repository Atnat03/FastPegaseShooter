using System;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class State<T>
{
    public T iD;

    [CanBeNull]public Action OnEnter;
    public Action OnExit;
    public Action OnUpdate;
    public Action OnFixedUpdate;
    public Action OnLateUpdate;

    public State(T iD, Action onEnter = null, Action onExit = null, Action onUpdate = null, Action onFixedUpdate = null, Action onLateUpdate = null)
    {
        this.iD = iD;
        this.OnEnter = onEnter;
        this.OnExit = onExit;
        this.OnUpdate = onUpdate;
        this.OnFixedUpdate = onFixedUpdate;
        this.OnLateUpdate = onLateUpdate;
    }

    public void Enter()
    {
        this.OnEnter?.Invoke();
    }

    public void Exit()
    {
        this.OnExit?.Invoke();
    }

    public void FixedUpdate()
    {
        this.OnFixedUpdate?.Invoke();
    }

    public void Update()
    {
        this.OnUpdate?.Invoke();
    }

    public void LateUpdate()
    {
        this.OnLateUpdate?.Invoke();
    }
}
