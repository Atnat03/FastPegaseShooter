using TMPro;
using UnityEngine;

public class DebugCanva : MonoBehaviour
{
    [Header("Canvas objects")] 
    [SerializeField] TextMeshProUGUI controllerCurrentState;
    [SerializeField] TextMeshProUGUI controllerGrounded;
    [SerializeField] TextMeshProUGUI controllerSidesDetection;
    [SerializeField] TextMeshProUGUI canWallRide;
    [SerializeField] TextMeshProUGUI CurrentVelocity;
    
    [Header("Ingame objects")]
    [SerializeField] FPSController mainFPSController;
    
    [Header("TPS Camera Metrix")]
    [SerializeField] GameObject TPSCamera;
    [SerializeField] float horizontalCameraOffset;
    [SerializeField] float verticalCameraOffset;
    
    void Update()
    {
        controllerCurrentState.text = mainFPSController.stateMachine.currentState.iD.ToString();
        controllerGrounded.text = mainFPSController.grounded? "Grounded" : "Not grounded";
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

        canWallRide.text = (mainFPSController.fellOffWallrinding || mainFPSController.justWallRided) ? "Cannot wallride" : "Can wallride";

        CurrentVelocity.text = "Current velocity :" + mainFPSController.horizontalVelocity.magnitude;
    }

    /*void LateUpdate()
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
        
    }*/
}
