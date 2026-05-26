using System;
using CustomConsole.Runtime.Logger;
using FishNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugInputManager : MonoBehaviour
{
    private DebugInput _debugInput;
    private void Awake()
    {
        _debugInput = new DebugInput();
    }

    private void OnEnable()
    {
        _debugInput.Enable();
        _debugInput.Debug.StopZoneSpawning.started += StopZoneSpawningOnStarted;
    }

    private void OnDisable()
    {
        _debugInput.Debug.StopZoneSpawning.started -= StopZoneSpawningOnStarted;
        _debugInput.Disable();
    }

    private void StopZoneSpawningOnStarted(InputAction.CallbackContext obj)
    {
        CustomLogger.CCErrorLog("This inputs needs to be replaced by OnDapEvent");
        
        if(InstanceFinder.IsServerStarted) EventBus.InvokeEvent(new OnDapEvent());
    }
}
