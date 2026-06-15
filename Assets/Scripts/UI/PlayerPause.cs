using System;
using MyPrint;
using TMPro;
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
    [SerializeField] private TextMeshProUGUI _gameCode;

    private PausePanel _currentPausePanel;

    private bool _isPause = false;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner) return;

        if (IsServerInitialized)
        {
            _gameCode.text = ConnectionWithCode.GameCode;
        }
        else
        {
            _gameCode.transform.parent.gameObject.SetActive(false);
        }

        foreach (PausePanel panel in _pauseUIPanels)
        {
            panel.Init();
        }
    }

    private void UpdatePause(InputAction.CallbackContext obj)
    {
        if (!_isPause && !CursorManager.CanPause())
            return;

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

        if (_isPause)
            CursorManager.instance.PushState(CursorState.UI, _fpsController);
        else
        {
            CursorManager.instance.PopState(_fpsController);
            Cursor.lockState = CursorLockMode.Locked; // jsp ce que fait la ligne du dessus mais elle marche pas bien
            Cursor.visible = false;
        }

        //InvokeEvent(new OnPauseEvent{p_isPause = _isPause});
    }

    public void ChangePanel(PausePanel panel)
    {
        if (_currentPausePanel != null)
        {
            _currentPausePanel.OnPanelDeselected();
            _currentPausePanel.gameObject.SetActive(false);
        }

        _currentPausePanel = panel;
        panel.gameObject.SetActive(true);
        panel.OnPanelSelected();
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