using UnityEngine;
using UnityEngine.InputSystem;

public class DebugPlayerOptions : MonoBehaviour
{
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] FPSController _fpsController;
    [SerializeField] PlayerHealth _playerHealth;

    void OnEnable()
    {
        _playerInput.actions["DebugLeaveGrapple"].performed += ControllerLeaveState;
        _playerInput.actions["DebugRespawn"].performed += Respawn;
    }

    void OnDisable()
    {
        _playerInput.actions["DebugLeaveGrapple"].performed -= ControllerLeaveState;
        _playerInput.actions["DebugRespawn"].performed -= Respawn;
    }

    private void Respawn(InputAction.CallbackContext obj)
    {
        _playerHealth.Respawn();
    }

    private void ControllerLeaveState(InputAction.CallbackContext obj)
    {
        _fpsController.stateMachine.ChangeState(FPSController.ControlerState.Idle);
    }
}