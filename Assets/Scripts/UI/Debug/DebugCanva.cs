using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugCanva : MonoBehaviour
{
    [Header("Canvas objects")] [SerializeField]
    TextMeshProUGUI controllerCurrentState;

    [SerializeField] TextMeshProUGUI controllerGrounded;
    [SerializeField] TextMeshProUGUI controllerSidesDetection;
    [SerializeField] TextMeshProUGUI canWallRide;
    [SerializeField] TextMeshProUGUI CurrentVelocity;
    [SerializeField] TextMeshProUGUI maxHeight;

    [Header("Ingame objects")] [SerializeField]
    FPSController mainFPSController;

    [SerializeField] PlayerInput playerInput;

    [Header("TPS Camera Metrix")] [SerializeField]
    GameObject TPSCamera;

    [SerializeField] float horizontalCameraOffset;
    [SerializeField] float verticalCameraOffset;

    private float currentHeightMarker;
    private float maxReachedHeight;

    void Start()
    {
        ResetHeightMarker();
    }

    void Update()
    {
        controllerCurrentState.text = mainFPSController.stateMachine.currentState?.iD.ToString();
        controllerGrounded.text = mainFPSController.grounded ? "Grounded" : "Not grounded";
        if (mainFPSController.leftSideAgainstWall && mainFPSController.rightSideAgainstWall)
        {
            controllerSidesDetection.text = "Both side against wall";
        }
        else if (mainFPSController.leftSideAgainstWall && !mainFPSController.rightSideAgainstWall)
        {
            controllerSidesDetection.text = "Left side against wall";
        }
        else if (!mainFPSController.leftSideAgainstWall && mainFPSController.rightSideAgainstWall)
        {
            controllerSidesDetection.text = "Right side against wall";
        }
        else
        {
            controllerSidesDetection.text = "No wall detected";
        }

        canWallRide.text = (mainFPSController.fellOffWallrinding || mainFPSController.justWallridedOtherSide)
            ? "Cannot wallride"
            : "Can wallride";

        CurrentVelocity.text = "Current velocity :" + mainFPSController.horizontalVelocity.magnitude;

        if (mainFPSController.transform.position.y > maxReachedHeight)
            maxReachedHeight = mainFPSController.transform.position.y;
        maxHeight.text = "Max Height Reached :" + (maxReachedHeight - currentHeightMarker);
        if (playerInput.actions["DebugResetHeight"].WasPressedThisFrame())
        {
            ResetHeightMarker();
        }
    }

    void LateUpdate()
    {
        if (TPSCamera.activeSelf)
        {
            TPSCamera.transform.position = new Vector3(
                mainFPSController.transform.position.x,
                mainFPSController.transform.position.y + verticalCameraOffset,
                mainFPSController.transform.position.z + horizontalCameraOffset
            );

            TPSCamera.transform.LookAt(mainFPSController.transform.position);
        }
    }

    void ResetHeightMarker()
    {
        currentHeightMarker = mainFPSController.transform.position.y;
        maxReachedHeight = currentHeightMarker;
    }
}