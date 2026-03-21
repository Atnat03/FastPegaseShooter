using UnityEngine;
using UnityEngine.InputSystem;

public class GunsSway : MonoBehaviour
{
    [Header("Sway")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] float _swayAmount = 0.02f;
    [SerializeField] float _maxSwayAmount = 0.06f;
    [SerializeField] float _smooth = 6f;
    [SerializeField] float _idleCoef = 0.01f;
    
    [Header("HeadBob")]
    [SerializeField] float _bobSpeed = 8f;
    [SerializeField] float _bobAmount = 0.02f;

    [Header("Vertical Bump")]
    [SerializeField] float _verticalBumpForce = 0.015f;
    [SerializeField] float _smoothTimeVerticalBump = 0.01f;

    private float _bumpPosition;
    private float _bumpVelocity;
    private float _lastVerticalVelocity;

    private Rigidbody _playerRB;
    
    private Vector3 _initialPosition;
    private float _bobTimer;
    
    
    void Start()
    {
        _initialPosition = transform.localPosition;
        _playerRB = _playerInput.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        #region Sway
        
        float mouseX = _playerInput.actions["Look"].ReadValue<Vector2>().x;
        float mouseY = _playerInput.actions["Look"].ReadValue<Vector2>().y;
        
        float moveX = Mathf.Clamp(-mouseX * _swayAmount, -_maxSwayAmount, _maxSwayAmount);
        float moveY = Mathf.Clamp(-mouseY * _swayAmount, -_maxSwayAmount, _maxSwayAmount);

        Vector3 finalPosition = new Vector3(_initialPosition.x + moveX, _initialPosition.y + moveY, _initialPosition.z);
        
        #endregion

        #region Headbob
        
        float idleY = Mathf.Cos(Time.time * 1.5f) * _idleCoef;

        finalPosition += new Vector3(0, idleY, 0);
        
        Vector2 moveInput = _playerInput.actions["Move"].ReadValue<Vector2>();

        if (moveInput.magnitude > 0.1f && _playerRB.linearVelocity.y < 0.2f) 
        {
            _bobTimer += Time.deltaTime * _bobSpeed;

            float bobY = Mathf.Sin(_bobTimer) * _bobAmount;
            finalPosition += new Vector3(0, bobY, 0);
        }
        else
        {
            _bobTimer = 0;
        }
        
        #endregion
        
        #region Vertical Bump (Jump/Landing)
        
        float verticalVelocity = _playerRB.linearVelocity.y;

        float targetBump = -verticalVelocity * (_verticalBumpForce);

        _bumpPosition = Mathf.SmoothDamp(_bumpPosition, targetBump, ref _bumpVelocity, _smoothTimeVerticalBump);

        finalPosition.y += _bumpPosition;
        #endregion

        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition, Time.deltaTime * _smooth);
    }
}
