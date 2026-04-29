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
    [SerializeField] private PausePanel[] _pauseUIPanels;
    [SerializeField] private PlayerInput _playerInput;
    
    private bool _isPause = false;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner) return;

        foreach (PausePanel panel in _pauseUIPanels)
        {
            panel.Init();
        }
    }
    
    private void UpdatePause(InputAction.CallbackContext obj)
    {
        UpdatePause();
    }
    
    public void UpdatePause()
    {
        if (!IsOwner) return;
        
        _isPause = !_isPause;
        
        foreach (PausePanel panel in _pauseUIPanels)
        {
            panel.OnPause(_isPause);
        }
        
        if(_isPause && Cursor.visible == false)
        {
            Cursor.lockState = _isPause ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = _isPause;
        }
        
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
