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
    [SerializeField] private FPSController _fpsController;
    
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
        
        if(_isPause)
            CursorManager.instance.PushState(CursorState.UI, _fpsController);
        else
        {
            CursorManager.instance.PopState(_fpsController);
            Cursor.lockState = CursorLockMode.Locked; // jsp ce que fait la ligne du dessus mais elle marche pas bien
            Cursor.visible = false;
        }
        
        //InvokeEvent(new OnPauseEvent{p_isPause = _isPause});
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
