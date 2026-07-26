using FishNet;
using FishNet.Object;
using UnityEngine.InputSystem;

public class DebugInputManager : NetworkBusListener
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
        _debugInput.Debug.GetInvincible.performed += GetInvincible;
        _debugInput.Debug.SkipLevel.performed += SkipLevel;
    }

    private void OnDisable()
    {
        _debugInput.Debug.StopZoneSpawning.started -= StopZoneSpawningOnStarted;
        _debugInput.Debug.GetInvincible.performed -= GetInvincible;
        _debugInput.Debug.SkipLevel.performed -= SkipLevel;

        _debugInput.Disable();
    }

    private void StopZoneSpawningOnStarted(InputAction.CallbackContext obj)
    {
        if(InstanceFinder.IsServerStarted)
        {
            EventBus.InvokeEvent(new OnDapEvent());
            EventBus.InvokeEvent(new AfterDapVideoEvent());
            DapObserver();
        }
    }

    [ObserversRpc]
    void DapObserver()
    {
        InvokeEvent(new OnDappEventObserveurs());
    }

    private void GetInvincible(InputAction.CallbackContext obj)
    {
        if(InstanceFinder.IsServerStarted) EventBus.InvokeEvent(new GetInvincibleEvent());
    }

    void SkipLevel(InputAction.CallbackContext obj)
    {
        FindAnyObjectByType<TriggerZoneServerToClient>().DebugSkipLevel();
    }
}
