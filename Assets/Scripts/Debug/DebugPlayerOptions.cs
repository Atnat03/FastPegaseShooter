using UnityEngine;
using UnityEngine.InputSystem;

public class DebugPlayerOptions : MonoBehaviour
{
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] FPSController _fpsController;

    void Start()
    {
        _playerInput.actions["DebugLeaveGrapple"].performed += ControllerLeaveState;
    }

    private void ControllerLeaveState(InputAction.CallbackContext obj)
    {
        _fpsController.stateMachine.ChangeState(FPSController.ControlerState.Idle);
    }
}