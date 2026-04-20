using System;
using UnityEngine;
using UnityEngine.InputSystem;

public struct OnPauseEvent
{
    public int p_playerId;
    public bool p_isPause;
}

public class PlayerPause : NetworkBusListener
{
    [SerializeField] private GameObject _pauseUI;
    [SerializeField] private PlayerInput _playerInput;
    
    public Action<bool> OnPause;

    private bool _isPause;

    
    private void UpdatePause(InputAction.CallbackContext obj)
    {
        UpdatePause();
    }
    
    public void UpdatePause()
    {
        if (!IsOwner) return;
        
        _isPause = !_isPause;
        
        _pauseUI.SetActive(_isPause);
        
        Cursor.lockState = _isPause ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = _isPause;
        
        OnPause?.Invoke(_isPause);
        InvokeEvent(new OnPauseEvent{p_isPause = _isPause});
    }
    
    
    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        _playerInput.actions["Escape"].performed += UpdatePause;
    }
    
    private void OnDisable()
    {
        _playerInput.actions["Escape"].performed -= UpdatePause;
    }

}
