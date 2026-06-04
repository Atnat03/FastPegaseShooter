using System;
using System.Collections.Generic;
using UnityEngine;

public enum CursorState
{
    Gameplay,
    UI
}

public class CursorManager : MonoBehaviour
{
    public static CursorManager instance;

    private void Awake()
    {
        instance = this;
    }

    private Stack<CursorState> _states = new Stack<CursorState>();
    
    public void PushState(CursorState state, FPSController fps)
    {
        _states.Push(state);
        
        ApplyState(fps);
    }

    public  void PopState(FPSController fps)
    {
        if (_states.Count > 0)
            _states.Pop();

        ApplyState(fps);
    }

    private void ApplyState(FPSController fps)
    {
        CursorState state = _states.Count > 0 ? _states.Peek() : CursorState.Gameplay;

        switch (state)
        {
            case CursorState.Gameplay:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                fps.SetFreeze(false);
                break;

            case CursorState.UI:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                fps.SetFreeze(true);
                break;
        }
    }
}