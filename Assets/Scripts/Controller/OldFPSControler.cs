using UnityEngine;



public class OldFPSControler : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform cameraTarget;
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] private float verticalLimit = 80f;
    [SerializeField] private float moveSpeed;
    [SerializeField] float followSmoothing = 15f;
    
    float yaw;
    float pitch;
    float horizontalInput;
    float verticalInput;
    
    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = cameraTransform.localEulerAngles.x;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
        float mouseX = Input.GetAxisRaw("Mouse X") *  mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") *  mouseSensitivity;
        
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -verticalLimit, verticalLimit);
    }

    void FixedUpdate()
    {
        Vector3 move = (transform.forward * verticalInput + transform.right * horizontalInput).normalized;
        Vector3 velocity = move * moveSpeed ;
        
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        transform.rotation = Quaternion.Euler(0, yaw, 0);
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0);
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, cameraTarget.position, followSmoothing * Time.deltaTime);
    }
}
