using System.Collections;
using FishNet;
using FishNet.Connection;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;
using FishNet.Object;

public class FPSController : NetworkBehaviour
{ 
    // prévoir une variable de smoothing (acceleration / deceleration) pour le dash si possible en animation curve
    
    // dans la mesure du possible, faire un jump qui prévoit la montée, la duree a l'apex et la redécente
    
    #region public variables
    
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform cameraTarget;
    [SerializeField] Transform playerFeet;
    [SerializeField] Transform playerLeftSide;
    [SerializeField] Transform playerRightSide;
    [SerializeField] float bodyRadius = .6f;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] private GameObject _playerVisual;
    [SerializeField] private PlayerAnimation _playerAnimation;

    [Header("parameters")]
    [Tooltip("empeche le smoothing de la camera au moment de l'atterissage")][SerializeField] private bool landSnap = true;
    [Tooltip("permet de gérer le dash en fonction de l'orientation de la camera, verticalité comprise")][SerializeField] private bool dashVerticality = false;
    [Tooltip("empeche le player de dépasser la maxAirSpeed, le controller ne prend plus en compte le airDrag")][SerializeField] private bool clampedMaxAirSpeed = false; 

    [Header("UnlockedCapacities")] 
    public bool wallRideUnlocked = true;
    public bool slideUnlocked = true; 
    public bool dashUnlocked = true;
    public bool slopeSlideUnlocked = true;
    
    [Header("movement")] 
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float verticalLimit = 80f;
    [SerializeField] float moveSpeed;
    [SerializeField] float groundMomentumFactor = 2f; 
    [SerializeField] float sideStepImpulseForce; 
    [SerializeField] float followSmoothing = 15f;
    [SerializeField] float wallDetectionRange = 0.65f;
    [SerializeField]float walkableSlopeAngle = 45f; 
    [SerializeField]float maxStepHeight = .2f;

    [Header("headbob")] 
    [SerializeField] float walkingHeadbobAmplitude = 0.05f;
    [SerializeField] float walkingHeadbobFrequency = 8f;
    [SerializeField] float wallRidingHeadbobAmplitude = 0.1f;
    [SerializeField] float wallRidingHeadbobFrequency = 8f;

    float yaw;
    float pitch;
    float horizontalInput;
    float verticalInput;
    float headbobTimer;

    [Header("jump")] 
    [SerializeField] float jumpForce = 7.5f;
    [SerializeField] float airControlForce = 2f;
    [SerializeField] float maxAirSpeed = 6f;
    [SerializeField] float airDrag = 2f; 
    [SerializeField] float bufferJumpTime = 0.2f;
    [SerializeField] float coyoteTimeDuration = 0.2f;
    [SerializeField] float landSnapVelocity = 50f;


    [Header("wallRide")] 
    [SerializeField] float wallRideDetectionRange = .5f;
    [SerializeField] float wallRidingDuration = 2f;
    [SerializeField] private float wallRideCooldown = .2f;
    [SerializeField] float wallRidingSpeed = 10f;
    [SerializeField] float minSpeedToWallRide = 1f;
    [SerializeField] float wallJumpVerticalForce = 10f;
    [SerializeField] float wallJumpHorizontalForce = 7.5f;
    [SerializeField] float headtiltIntensity = 7f;

    [Header("Crouch")] 
    [SerializeField] float crouchSpeed = 5f;
    [SerializeField] float cameraOffsetWhenCrouching = 1f;
    [SerializeField] GameObject[] bodyStandUpCollider;
    [SerializeField] Transform topHeightStandUpCollider;
    [SerializeField] GameObject[] bodyCrouchedCollider;
    [SerializeField] Transform topHeightCrouchedCollider;

    [Header("Slide")] 
    [SerializeField] float slideSpeed = 5f;
    [SerializeField] float slideTimeDuration = 0.2f;
    [SerializeField] float slideJumpVerticalForce = 6.5f;
    [SerializeField] float slideJumpHorizontalForce = 2f;
    [SerializeField] float slideCooldown = .1f; 
    [SerializeField] float coyoteSlideDuration = .1f;

    [Header("Dash")] 
    [SerializeField] float dashSpeed = 5f;
    [SerializeField] float dashTimeDuration = 0.2f;
    [SerializeField] float dashCooldown;

    [Header("Slope Slide")]
    [SerializeField] float minSlopeAngleToSlopeSlide = 11f; 
    [SerializeField] float slopeSlideMaxSpeed = 11f;
    [SerializeField] float slopeInfluenceOnRotation = 3f;
    [SerializeField] float slopeInfluenceOnVelocity = .75f;
    
    #endregion

    #region private variables
    
    [HideInInspector] public bool grounded;
    [HideInInspector] public bool leftSideAgainstWall;
    [HideInInspector] public bool rightSideAgainstWall;
    RaycastHit leftSideHit;
    RaycastHit rightSideHit;
    RaycastHit groundedHit;
    [HideInInspector] public bool fellOffWallrinding;

    private Vector3 horizontalVelocity;

    private bool justJumped;
    bool hasJumped;
    bool bufferJump = false;
    bool coyoteJump = false;
    bool hasDashed = false;
    bool coyoteSlide = false;


    public enum ControlerState
    {
        Idle,
        Moving,
        Falling,
        WallRiding,
        Crouching,
        Sliding,
        Dashing,
        SlopeSliding
    }

    public readonly StateMachine<ControlerState> stateMachine = new StateMachine<ControlerState>();
    private InputActionMap actionMap;
    
    #endregion

    public override void OnStartClient()
    {
        base.OnStartClient();

        if(IsOwner)
        {
            SetUpLayer();
        }
        
        cameraTransform = Camera.main.transform;
        
        actionMap = playerInput.currentActionMap;
        
        Cursor.lockState = CursorLockMode.Locked;

        yaw = transform.eulerAngles.y;
        pitch = cameraTransform.localEulerAngles.x;

        foreach (GameObject col in bodyStandUpCollider) col.SetActive(true);
        foreach (GameObject col in bodyCrouchedCollider) col.SetActive(false);

        stateMachine.Add(new State<ControlerState>(
            ControlerState.Idle,
            onEnter: EnterIdleState,
            onUpdate: IdleUpdate,
            onFixedUpdate: IdleFixedUpdate,
            onLateUpdate: IdleLateUpdate
        ));

        stateMachine.Add(new State<ControlerState>(
            ControlerState.Moving,
            onFixedUpdate: MovingFixedUpdate,
            onUpdate: MovingUpdate,
            onLateUpdate: MovingLateUpdate
        ));

        stateMachine.Add(new State<ControlerState>(
            ControlerState.Falling,
            onEnter: EnterFallingState,
            onUpdate: FallingUpdate,
            onFixedUpdate: FallingFixedUpdate,
            onExit: ExitFallingState,
            onLateUpdate: FallingLateUpdate
        ));

        stateMachine.Add(new State<ControlerState>(
            ControlerState.WallRiding,
            onEnter: EnterWallRidingState,
            onUpdate: WallRidingUpdate,
            onFixedUpdate: WallRidingFixedUpdate,
            onExit: ExitWallRidingState,
            onLateUpdate: WallRidingLateUpdate
        ));

        stateMachine.Add(new State<ControlerState>(
            ControlerState.Crouching,
            onEnter: EnterCrouchingState,
            onUpdate: CrouchingUpdate,
            onFixedUpdate: CrouchingFixedUpdate,
            onExit: CrouchingExitState,
            onLateUpdate: CrouchingLateUpdate
        ));

        stateMachine.Add(new State<ControlerState>(
            ControlerState.Sliding,
            onEnter: EnterSlidingState,
            onUpdate: SlidingUpdate,
            onFixedUpdate: SlidingFixedUpdate,
            onExit: SlidingExitState,
            onLateUpdate: SlidingLateUpdate
        ));

        stateMachine.Add(new State<ControlerState>(
            iD: ControlerState.Dashing,
            onEnter: EnterDashingState,
            onUpdate: DashingUpdate,
            onFixedUpdate: DashingFixedUpdate,
            onExit: DashingExitState,
            onLateUpdate: DashingLateUpdate
        ));
        
        stateMachine.Add(new State<ControlerState>(
            iD: ControlerState.SlopeSliding,
            onEnter: EnterSlopeSlidingState,
            onUpdate: SlopeSlidingUpdate,
            onFixedUpdate: SlopeSlidingFixedUpdate,
            onExit: ExitSlopeSlidingState,
            onLateUpdate: SlopeSlidingLateUpdate
            ));

        stateMachine.ChangeState(ControlerState.Idle);
    }


    #region Function Calling

    void Update()
    {
        if (!IsOwner) return;
        
        UpdateInputs();
        stateMachine?.Update();
    }

    void FixedUpdate()
    { 
        if (!IsOwner) return;
        
        stateMachine?.FixedUpdate();
    }

    void LateUpdate()
    {
        if (!IsOwner) return;
        
        stateMachine?.LateUpdate();
    }


    void UpdateInputs()
    {
        //inputs
        horizontalInput = playerInput.actions["Move"].ReadValue<Vector2>().x;
        verticalInput = playerInput.actions["Move"].ReadValue<Vector2>().y;

        float mouseX = playerInput.actions["Look"].ReadValue<Vector2>().x * mouseSensitivity;
        float mouseY = playerInput.actions["Look"].ReadValue<Vector2>().y * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -verticalLimit, verticalLimit);

        //situation de jeu

        grounded = (Physics.Raycast(playerFeet.position, Vector3.down, out groundedHit, 0.25f,
            ~LayerMask.GetMask("Player")) && !justJumped);

        leftSideAgainstWall = Physics.Raycast(playerLeftSide.position, playerLeftSide.forward,
            out leftSideHit, wallRideDetectionRange, ~LayerMask.GetMask("Player"));
        rightSideAgainstWall = Physics.Raycast(playerRightSide.position, playerRightSide.forward,
            out rightSideHit, wallRideDetectionRange, ~LayerMask.GetMask("Player"));

        horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    }

    #endregion

    #region states

    #region IdleState

    void EnterIdleState()
    {
        if (playerInput.actions["Crouch"].IsPressed())
        {
            if (Vector3.Angle(groundedHit.normal, Vector3.up) > minSlopeAngleToSlopeSlide && slopeSlideUnlocked)
            {
                stateMachine.ChangeState(ControlerState.SlopeSliding);
            }
            else if(!justSlided && slideUnlocked)
            {
                stateMachine.ChangeState(ControlerState.Sliding);
            }
        }
        
        if (bufferJump && playerInput.actions["Jump"].IsPressed()) Jump();
        
        rb.linearVelocity = Vector3.zero;

        _playerAnimation.SetMovingAnim(false);

        if (grounded)
        {
            fellOffWallrinding = false;
            hasDashed = false;
        }

        if (stateMachine.previousState == stateMachine.GetState(ControlerState.Falling) && landSnap)
            StartCoroutine(FollowSmoothingOnLandingCoroutine());
    }

    void IdleUpdate()
    {
        if (verticalInput != 0f || horizontalInput != 0f)
        {
            if (verticalInput == 0f) SideStep();
            _playerAnimation.SetMovingAnim(true);
            stateMachine.ChangeState(ControlerState.Moving);
        }

        if (!grounded)
        {
            stateMachine.ChangeState(ControlerState.Falling);
        }

        if (playerInput.actions["Jump"].WasPressedThisFrame())
        {
            Jump();
        }

        if (playerInput.actions["Crouch"].WasPressedThisFrame())
        {
            if (coyoteSlide && !justSlided && slideUnlocked) stateMachine.ChangeState(ControlerState.Sliding); 
            else stateMachine.ChangeState(ControlerState.Crouching);
        }

        if (playerInput.actions["Dash"].WasPressedThisFrame() && !hasDashed && !justDashed && dashUnlocked)
        {
            stateMachine.ChangeState(ControlerState.Dashing);
        }
    }

    void IdleFixedUpdate()
    {
    }

    void IdleLateUpdate()
    {
        UpdateCameraPositionAndRotation();
    }

    float defaultSmooting;
    IEnumerator FollowSmoothingOnLandingCoroutine()
    {
        defaultSmooting = followSmoothing;
        followSmoothing = landSnapVelocity;
        yield return new WaitForSeconds(.2f);
        followSmoothing = defaultSmooting;
    }

    #endregion

    #region MovingState

    void MovingUpdate()
    {
        if (verticalInput == 0f && horizontalInput == 0f)
        {
            stateMachine.ChangeState(ControlerState.Idle);
        }

        if (!grounded)
        {
            stateMachine.ChangeState(ControlerState.Falling);
        }

        if (playerInput.actions["Jump"].WasPressedThisFrame())
        {
            Jump();
        }
        
        if (playerInput.actions["Crouch"].WasPressedThisFrame())
        {
            if (Vector3.Angle(groundedHit.normal, Vector3.up) > minSlopeAngleToSlopeSlide && slopeSlideUnlocked)
                stateMachine.ChangeState(ControlerState.SlopeSliding);
            else if (coyoteSlide && slideUnlocked && !justSlided) stateMachine.ChangeState(ControlerState.Sliding);
            else stateMachine.ChangeState(ControlerState.Crouching);
        }
        

        if (playerInput.actions["Dash"].WasPressedThisFrame() && !hasDashed && !justDashed && dashUnlocked)
        {
            stateMachine.ChangeState(ControlerState.Dashing);
        }
    }

    void MovingFixedUpdate()
    {
        Vector3 move = (transform.forward * verticalInput + transform.right * horizontalInput).normalized;

        Vector3 velocity = move * moveSpeed;
        velocity.y = rb.linearVelocity.y;
        
        if (horizontalVelocity.magnitude > moveSpeed)
        {
            velocity = Vector3.MoveTowards(horizontalVelocity, velocity, groundMomentumFactor * Time.deltaTime);
        }

        velocity = AlignVelocityToWall(velocity);

        rb.linearVelocity = InterpolateSlope(velocity);
    }


    void MovingLateUpdate()
    {
        UpdateCameraPositionAndRotation(true, walkingHeadbobAmplitude, walkingHeadbobFrequency);
    }

    #endregion

    #region FallingState

    private bool mustHeadTilt;

    void EnterFallingState()
    {
        _playerAnimation.PlayLandingAnim(true);
        
        if (!hasJumped) 
            StartCoroutine(CoyoteTimeCoroutine());
    }

    void FallingUpdate()
    {
        if (grounded)
        {
            _playerAnimation.PlayLandingAnim(false);
            stateMachine.ChangeState(ControlerState.Idle);
        }

        if (playerInput.actions["Jump"].WasPressedThisFrame())
        {
            if (coyoteJump) Jump();
            else StartCoroutine(JumpBufferingCoroutine());
        }

        if (verticalInput > 0.1f && (leftSideAgainstWall || rightSideAgainstWall) && horizontalVelocity.magnitude > minSpeedToWallRide && !grounded && !justWallRided && !fellOffWallrinding && wallRideUnlocked)
        {
            _playerAnimation.PlayLandingAnim(false);
            if (rb.linearVelocity.y < 0) stateMachine.ChangeState(ControlerState.WallRiding);
            else mustHeadTilt = true;
        }

        if (playerInput.actions["Dash"].WasPressedThisFrame() && !hasDashed && !justDashed && dashUnlocked)
        {
            _playerAnimation.PlayLandingAnim(false);
            stateMachine.ChangeState(ControlerState.Dashing);
        }
    }

    void FallingFixedUpdate() // on peut tres facilecement diviser air controle force en deux floats, un de rediraction et un de force d'arret puisque la redirection se fait avec une methode d'ifferente de l'arret qui n'utilisent pas les memes ordres de grandeurs
    {
        Vector3 velocity = rb.linearVelocity;
        
        Vector3 move = (transform.forward * verticalInput + transform.right * horizontalInput).normalized;

        Vector3 desiredHorizontal = horizontalVelocity;

        if (clampedMaxAirSpeed)
        {
            desiredHorizontal = (horizontalVelocity + (move * (airControlForce * 10 * Time.fixedDeltaTime)));
            if (desiredHorizontal.magnitude > maxAirSpeed)
            {
                desiredHorizontal = desiredHorizontal.normalized * maxAirSpeed;
            }
        }
        else
        {
            if ((horizontalVelocity + (move * (airControlForce * Time.fixedDeltaTime))).magnitude > maxAirSpeed)
            {
                Vector3 drag = desiredHorizontal.normalized * ((desiredHorizontal.magnitude - maxAirSpeed) * airDrag);

                desiredHorizontal -= drag * Time.fixedDeltaTime;
                
                if (horizontalVelocity != Vector3.zero && desiredHorizontal != Vector3.zero)
                {
                    Vector3 currentDir = horizontalVelocity.normalized;
                    Vector3 desiredDir = move.normalized;

                    if (Vector3.Dot(currentDir, desiredDir) > 0)
                    {
                        if (Vector3.Angle(currentDir, desiredDir) > 1f)
                        {
                            Vector3 newDir = Vector3.Slerp(currentDir, desiredDir, airControlForce * Time.fixedDeltaTime);
                            
                            desiredHorizontal = newDir.normalized * desiredHorizontal.magnitude;
                        }
                    }
                    else
                    {
                        desiredHorizontal = horizontalVelocity + (move * (airControlForce * 10 * Time.fixedDeltaTime));
                    }
                }
            }
            else
            {
                desiredHorizontal = horizontalVelocity + (move * (airControlForce * 10 * Time.fixedDeltaTime));
            }
        }

        velocity.x = desiredHorizontal.x;
        velocity.z = desiredHorizontal.z;
        
        velocity = AlignVelocityToWall(velocity);

        rb.linearVelocity = velocity;
    }


    void ExitFallingState()
    {
        hasJumped = false;
        mustHeadTilt = false;
        _playerAnimation.PlayLandingAnim(false);
        StartCoroutine(CoyoteSlideCoroutine());
    }

    void FallingLateUpdate()
    {
        if (mustHeadTilt) UpdateCameraPositionAndRotation(true, wallRidingHeadbobAmplitude, wallRidingHeadbobFrequency);
        else UpdateCameraPositionAndRotation();
    }

    IEnumerator CoyoteTimeCoroutine()
    {
        coyoteJump = true;
        yield return new WaitForSeconds(coyoteTimeDuration);
        coyoteJump = false;
    }

    IEnumerator JumpBufferingCoroutine()
    {
        _playerAnimation.PlayJumpAnim();
        bufferJump = true;
        yield return new WaitForSeconds(bufferJumpTime);
        bufferJump = false;
    }

    IEnumerator CoyoteSlideCoroutine()
    {
        coyoteSlide = true; 
        yield return new WaitForSeconds(coyoteSlideDuration); 
        coyoteSlide = false;
    }

    #endregion

    #region WallRidingState

    private float wallRidingHeight;
    private Vector3 wallRidingDirection;

    RaycastHit currentWallHit;

    Coroutine wallRidingCoroutine;
    bool wallRidingCoroutineRunning;

    [HideInInspector] public bool justWallRided;

    void EnterWallRidingState()
    {
        hasDashed = false; // ligne a retirer si on veut que le joueur doive toucher le sol avant de redasher
        
        wallRidingCoroutineRunning = true;
        wallRidingHeight = transform.position.y;

        if (leftSideAgainstWall)
        {
            wallRidingDirection = Vector3.Dot(Vector3.Cross(leftSideHit.normal, Vector3.up), rb.linearVelocity) *
                                  Vector3.Cross(leftSideHit.normal, Vector3.up);
            currentWallHit = leftSideHit;
            cameraTarget.rotation = cameraTransform.rotation = Quaternion.Euler(pitch, yaw, -headtiltIntensity);
        }
        else
        {
            wallRidingDirection = Vector3.Dot(Vector3.Cross(rightSideHit.normal, Vector3.up), rb.linearVelocity) *
                                  Vector3.Cross(rightSideHit.normal, Vector3.up);
            currentWallHit = rightSideHit;
            cameraTarget.rotation = cameraTransform.rotation = Quaternion.Euler(pitch, yaw, headtiltIntensity);
        }

        wallRidingCoroutine = StartCoroutine(WallRidingDurationCoroutine());
    }

    void WallRidingUpdate()
    {
        if (verticalInput == 0f || (!leftSideAgainstWall && !rightSideAgainstWall) || !wallRidingCoroutineRunning)
        {
            stateMachine.ChangeState(ControlerState.Falling);
        }

        if (playerInput.actions["Jump"].WasPressedThisFrame())
        {
            WallJump(currentWallHit.normal);
        }

        if (grounded)
        {
            stateMachine.ChangeState(ControlerState.Idle);
        }
    }

    void WallRidingFixedUpdate()
    {
        transform.position = new Vector3(transform.position.x, wallRidingHeight, transform.position.z);

        Vector3 move = (wallRidingDirection * verticalInput).normalized;
        Vector3 velocity = move * wallRidingSpeed;

        rb.linearVelocity = velocity;
    }

    void ExitWallRidingState()
    {
        cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0);
        StopCoroutine(wallRidingCoroutine);
    }

    void WallRidingLateUpdate()
    {
        UpdateCameraPositionAndRotation(true, wallRidingHeadbobAmplitude, wallRidingHeadbobFrequency);
    }

    IEnumerator WallRidingDurationCoroutine()
    {
        yield return new WaitForSeconds(wallRidingDuration);
        wallRidingCoroutineRunning = false;
        fellOffWallrinding = true;
        StartCoroutine(WallRidingCooldownCoroutine());
    }

    IEnumerator WallRidingCooldownCoroutine()
    {
        justWallRided = true;
        yield return new WaitForSeconds(wallRideCooldown);
        justWallRided = false;
    }

    #endregion

    #region CrouchingState

    void EnterCrouchingState()
    {
        Crouch();
    }

    void CrouchingUpdate()
    {
        if (!grounded)
        {
            stateMachine.ChangeState(ControlerState.Falling);
        }

        if (playerInput.actions["Crouch"].WasReleasedThisFrame())
        {
            if (verticalInput != 0f || horizontalInput != 0f)
            {
                stateMachine.ChangeState(ControlerState.Moving);
            }

            else
            {
                stateMachine.ChangeState(ControlerState.Idle);
            }
        }
    }

    void CrouchingFixedUpdate()
    {
        Vector3 move = (transform.forward * verticalInput + transform.right * horizontalInput).normalized;

        Vector3 velocity = move * crouchSpeed;
        velocity.y = rb.linearVelocity.y;

        velocity = AlignVelocityToWall(velocity, true);
        velocity = InterpolateSlope(velocity);

        rb.linearVelocity = velocity;
    }

    void CrouchingExitState()
    {
        UnCrouch();
    }

    void CrouchingLateUpdate()
    {
        UpdateCameraPositionAndRotation();
    }

    #endregion

    #region SlidingState

    private bool mustSlide;
    private bool justSlided;
    Vector3 landingDirection;

    void EnterSlidingState()
    {
        landingDirection = (transform.forward * verticalInput + transform.right * horizontalInput).normalized;
        landingDirection *= slideSpeed;
        landingDirection.y = rb.linearVelocity.y;

        Crouch();
        StartCoroutine(SlidingCoroutine());
    }

    void SlidingUpdate()
    {
        if (!grounded)
        {
            stateMachine.ChangeState(ControlerState.Falling);
        }

        if (!mustSlide)
        {
            if (playerInput.actions["Jump"].IsPressed())
            {
                SlideJump();
                stateMachine.ChangeState(ControlerState.Idle);
            }

            else if (playerInput.actions["Crouch"].IsPressed())
            {
                stateMachine.ChangeState(ControlerState.Crouching);
            }

            else if (verticalInput != 0f || horizontalInput != 0f)
            {
                stateMachine.ChangeState(ControlerState.Moving);
            }

            else
            {
                stateMachine.ChangeState(ControlerState.Idle);
            }
        }
    }

    void SlidingFixedUpdate()
    {
        landingDirection = AlignVelocityToWall(landingDirection, true);
        landingDirection = InterpolateSlope(landingDirection);
        rb.linearVelocity = landingDirection;
    }

    void SlidingExitState()
    {
        UnCrouch();
        StartCoroutine(JustSlidedCoroutine());
    }

    void SlidingLateUpdate()
    {
        UpdateCameraPositionAndRotation();
    }

    IEnumerator SlidingCoroutine()
    {
        mustSlide = true;
        yield return new WaitForSeconds(slideTimeDuration);
        mustSlide = false;
    }

    IEnumerator JustSlidedCoroutine()
    {
        justSlided = true; 
        yield return new WaitForSeconds(slideCooldown); 
        justSlided = false;
    }

    #endregion

    #region DashingState

    private bool isDashing;
    private bool justDashed;
    Vector3 dashingDirection;

    void EnterDashingState()
    {
        if (dashVerticality)
        {
            dashingDirection = cameraTransform.forward;
        }
        else
        {
            if (verticalInput == 0f && horizontalInput == 0f) dashingDirection = transform.forward;
            else dashingDirection = (transform.forward * verticalInput + transform.right * horizontalInput).normalized;
        }
        dashingDirection *= dashSpeed;
        StartCoroutine(DashingCoroutine());
    }

    void DashingUpdate()
    {
        if (!isDashing)
        {
            stateMachine.ChangeState(ControlerState.Idle);
        }
    }


    void DashingFixedUpdate()
    {
        dashingDirection = AlignVelocityToWall(dashingDirection);
        dashingDirection = InterpolateSlope(dashingDirection);
        rb.linearVelocity += dashingDirection;
    }

    void DashingExitState()
    {
        hasDashed = true;
        coyoteJump = false; 
        bufferJump = false; 
        StartCoroutine(DashCooldownCoroutine());
    }

    void DashingLateUpdate()
    {
        UpdateCameraPositionAndRotation();
    }

    IEnumerator DashingCoroutine()
    {
        isDashing = true;
        yield return new WaitForSeconds(dashTimeDuration);
        isDashing = false;
    }

    IEnumerator DashCooldownCoroutine()
    {
        justDashed = true; 
        yield return new WaitForSeconds(dashCooldown);
        justDashed = false;
    }

    #endregion

    #region SlopeSlidingState

    Vector3 slopeDirection;
    Vector3 currentDirection;
    
    void EnterSlopeSlidingState()
    {
        slopeDirection = Vector3.ProjectOnPlane(Vector3.down, groundedHit.normal);
        
        currentDirection = horizontalVelocity.magnitude > 0.1f 
            ? Vector3.ProjectOnPlane( rb.linearVelocity, groundedHit.normal).normalized * rb.linearVelocity.magnitude
            : slopeDirection;
        
        Debug.Log(currentDirection == slopeDirection);
        Debug.Log(slopeDirection);
        
        Crouch();
    }

    void SlopeSlidingUpdate()
    {
        if(!grounded) stateMachine.ChangeState(ControlerState.Falling);
        if (playerInput.actions["Crouch"].WasReleasedThisFrame()) stateMachine.ChangeState(ControlerState.Idle);
        if (playerInput.actions["Jump"].WasPressedThisFrame()) SlideJump(); // au besoin, faire une autre fonction
        if(Vector3.Angle(groundedHit.normal, Vector3.up) < minSlopeAngleToSlopeSlide) stateMachine.ChangeState(ControlerState.Idle);
    }

    void SlopeSlidingFixedUpdate()
    {
        slopeDirection = Vector3.ProjectOnPlane(Vector3.down, groundedHit.normal);
        
        float slopeAngle = Vector3.Angle(groundedHit.normal, Vector3.up);
        
        float slopeFactor = slopeAngle / 70; // à clamp si besoin
        slopeFactor = Mathf.Clamp01(slopeFactor);
        
        slopeFactor = slopeInfluenceOnRotation * slopeFactor * Time.fixedDeltaTime;
        
        currentDirection = Vector3.Slerp(currentDirection, slopeDirection, slopeFactor);
        
        if (Vector3.Angle(slopeDirection, currentDirection) < 90)
        {
            currentDirection *= 1 + (1 / Mathf.Max(Vector3.Angle(slopeDirection, currentDirection), 1f)) * slopeInfluenceOnVelocity * Vector3.Angle(groundedHit.normal, Vector3.up);
        }
        
        Vector3 newVelocity = currentDirection.magnitude > slopeSlideMaxSpeed ? currentDirection.normalized * slopeSlideMaxSpeed : currentDirection;
        
        if (float.IsNaN(newVelocity.x) || float.IsNaN(newVelocity.y) || float.IsNaN(newVelocity.z))
        {
            Debug.LogError("NaN velocity detected");
            return;
        }
        
        rb.linearVelocity = newVelocity;
    }


    void ExitSlopeSlidingState()
    {
        UnCrouch();
    }

    void SlopeSlidingLateUpdate()
    {
        UpdateCameraPositionAndRotation();
    }

    #endregion
    
    #endregion

    #region PlayerActions

    private float currentRoll = 0f;

    void UpdateCameraPositionAndRotation(bool headbob = false, float headbobAmplitude = 0f, float headbobFrequency = 0f)
    {
        transform.rotation = Quaternion.Euler(0, yaw, 0);

        float targetRoll = cameraTarget.eulerAngles.z;
        currentRoll = Mathf.LerpAngle(currentRoll, targetRoll, followSmoothing * Time.deltaTime);

        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, currentRoll);

        Vector3 targetPos = cameraTarget.position;

        if (headbob)
        {
            headbobTimer += Time.deltaTime * headbobFrequency;

            float bobY = Mathf.Sin(headbobTimer) * headbobAmplitude;
            float bobX = Mathf.Cos(headbobTimer * 0.5f) * headbobAmplitude * 0.5f;

            targetPos += cameraTransform.up * bobY;
            targetPos += cameraTransform.right * bobX;
        }
        else
        {
            headbobTimer = 0f;
        }

        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, followSmoothing * Time.deltaTime);
    }


    private Vector3 currenHorizontal;
    private Vector3 capsuleTop;
    Vector3 AlignVelocityToWall(Vector3 velocity, bool crouched = false)
    {
        currenHorizontal = new Vector3(velocity.x, 0f, velocity.z);
        if (currenHorizontal.sqrMagnitude < 0.0001f) return velocity;

         capsuleTop = crouched ? topHeightCrouchedCollider.position : topHeightStandUpCollider.position;

        if (Physics.CapsuleCast(playerFeet.position, capsuleTop, bodyRadius, currenHorizontal.normalized, out RaycastHit hit, wallDetectionRange, ~LayerMask.GetMask("Player")))
        {
            // Si le contact est bas , pente / sol
            float hitHeight = hit.point.y - playerFeet.position.y;
            if (hitHeight < maxStepHeight)
            {
                Vector3 temp = new Vector3(velocity.x, velocity.y + maxStepHeight, velocity.z);
                return temp;
            }

            float hitSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            // Pente marchable , ne pas bloquer
            if (hitSlopeAngle <= walkableSlopeAngle) return velocity;

            //  Vérifier que c’est bien face au mouvement
            if (Vector3.Dot(currenHorizontal.normalized, -hit.normal) < 0.5f) return velocity;
            
            Vector3 aligned = Vector3.ProjectOnPlane(currenHorizontal, hit.normal);
            return new Vector3(aligned.x, velocity.y, aligned.z);
        }

        return velocity;
    }
    
    Vector3 InterpolateSlope(Vector3 velocity)
    {
        if (!grounded) return velocity;

        Vector3 normal = groundedHit.normal;
        float slopeAngle = Vector3.Angle(normal, Vector3.up);

        if (slopeAngle < 1f) return velocity;
        if (slopeAngle > walkableSlopeAngle) return velocity;
        
        Vector3 slopeVelocity = Vector3.ProjectOnPlane(velocity, normal);
        
        if (velocity.y > 0.1f) // seuil pour le saut
        {
            slopeVelocity.y = velocity.y;
        }

        return slopeVelocity;
    }
    
    private void SideStep()
    {
        Vector3 move = (transform.right * horizontalInput).normalized * sideStepImpulseForce; 
        rb.AddForce(move, ForceMode.Impulse);
    }

    
    void Jump()
    {
        hasJumped = true;
        coyoteJump = false;
        bufferJump = false;
        StartCoroutine(JumpAntiLagCoroutine());
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    IEnumerator JumpAntiLagCoroutine()
    {
        justJumped = true;
        yield return new WaitForSeconds(.1f);
        justJumped = false;
    }

    void WallJump(Vector3 wallNormal)
    {
        hasJumped = true;
        coyoteJump = false;
        bufferJump = false;
        Vector3 wallJumpDirection =
            wallNormal.normalized * wallJumpHorizontalForce + Vector3.up * wallJumpVerticalForce;
        rb.AddForce(wallJumpDirection, ForceMode.Impulse);
        StartCoroutine(WallRidingCooldownCoroutine());
        stateMachine.ChangeState(ControlerState.Falling);
    }

    void SlideJump()
    {
        hasJumped = true;
        coyoteJump = false;
        bufferJump = false;
        StartCoroutine(JumpAntiLagCoroutine());
        rb.AddForce(Vector3.up * slideJumpVerticalForce + horizontalVelocity * slideJumpHorizontalForce, ForceMode.Impulse);
    }

    void Crouch()
    {
        cameraTarget.position =
            new Vector3(cameraTarget.position.x, cameraTarget.position.y - cameraOffsetWhenCrouching,
                cameraTarget.position.z);
        foreach (GameObject col in bodyStandUpCollider) col.SetActive(false);
        foreach (GameObject col in bodyCrouchedCollider) col.SetActive(true);
    }

    void UnCrouch()
    {
        cameraTarget.position =
            new Vector3(cameraTarget.position.x, cameraTarget.position.y + cameraOffsetWhenCrouching,
                cameraTarget.position.z);
        foreach (GameObject col in bodyStandUpCollider) col.SetActive(true);
        foreach (GameObject col in bodyCrouchedCollider) col.SetActive(false);
    }

    #endregion

    #region Other Fonctions
        
    public void SetUpLayer()
    {
        print("Change layer");
        
        SetLayerRecursively(_playerVisual, LayerMask.NameToLayer("Owner"));
    }
    
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    #endregion

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(playerFeet.position, playerFeet.position + Vector3.down * 0.1f);
        Gizmos.DrawLine(playerLeftSide.position,
            playerLeftSide.position + playerLeftSide.forward * wallRideDetectionRange);
        Gizmos.DrawLine(playerRightSide.position,
            playerRightSide.position + playerRightSide.forward * wallRideDetectionRange);
    }
}
