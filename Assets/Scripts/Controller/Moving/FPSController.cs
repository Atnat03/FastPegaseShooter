using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public struct RequestEnergyEvent
{
    public IEnergyRequest requester;
}

public struct RequestEnergyResponseEvent
{
    public float energy;
}

public class FPSController : NetworkBehaviour, IEnergyRequest
{
    // prévoir une variable de smoothing (acceleration / deceleration) pour le dash si possible en animation curve

    // dans la mesure du possible, faire un jump qui prévoit la montée, la duree a l'apex et la redécente

    public Camera Camera => _camera;
    public bool IsFreeze { get => isFreeze; set=> isFreeze=value; }

    #region public variables

    [SerializeField] Rigidbody rb;
    [SerializeField] Transform cameraParentTransform;
    [SerializeField] Transform cameraSpringTarget;
    [SerializeField] private Camera _camera;
    [SerializeField] Transform playerFeet;
    [SerializeField] Transform playerLeftSide;
    [SerializeField] Transform playerRightSide;
    [SerializeField] float bodyRadius = .6f;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] private GameObject _playerVisual;
    [SerializeField] private PlayerAnimation _playerAnimation;

    [Header("parameters")] [Tooltip("empeche le smoothing de la camera au moment de l'atterissage")] [SerializeField]
    private bool landSnap = true;

    [Tooltip("permet de gérer le dash en fonction de l'orientation de la camera, verticalité comprise")]
    [SerializeField]
    private bool dashVerticality = false;

    [Tooltip("empeche le player de dépasser la maxAirSpeed, le controller ne prend plus en compte le airDrag")]
    [SerializeField]
    private bool clampedMaxAirSpeed = false;

    [Tooltip("est ce que le joueur doit attendre la fin du slide avant de JumpSlide")] [SerializeField]
    private bool jumpSlideOnEndOfSlide = false;

    [Header("UnlockedCapacities")] public bool wallRideUnlocked = true;
    public bool slideUnlocked = true;
    public bool dashUnlocked = true;
    public bool slopeSlideUnlocked = true;

    [Header("Camera")] [SerializeField] float cameraSpringHalfLife = 0.075f;
    [SerializeField] float cameraSpringFrequency = 22.5f;
    [SerializeField] float rollSmoothing = 15f;

    [Header("movement")] [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float verticalLimit = 80f;
    [SerializeField] float moveSpeed;
    [SerializeField] float groundMomentumFactor = 2f;
    [SerializeField] float sideStepImpulseForce;
    [SerializeField] float wallDetectionRange = 0.65f;
    [SerializeField] float walkableSlopeAngle = 45f;
    [SerializeField] float maxStepHeight = .2f;

    [Header("headbob")] [SerializeField] float walkingHeadbobAmplitude = 0.05f;
    [SerializeField] float walkingHeadbobFrequency = 8f;
    [SerializeField] float wallRidingHeadbobAmplitude = 0.1f;
    [SerializeField] float wallRidingHeadbobFrequency = 8f;
    [SerializeField] float headbobStopReturningSpeed = 5f;

    float yaw;
    float pitch;
    float horizontalInput;
    float verticalInput;
    float headbobTimer;

    [Header("jump")] [SerializeField] float jumpForce = 7.5f;
    [SerializeField] float airControlForce = 2f;
    [SerializeField] float maxAirSpeed = 6f;
    [SerializeField] float airDrag = 2f;
    [SerializeField] float bufferJumpTime = 0.2f;
    [SerializeField] float coyoteTimeDuration = 0.2f;
    [SerializeField] float landSnapVelocity = 50f;

    [Header(("Super Jump"))] //new
    [SerializeField] [Tooltip("delai maximum avant le deuxieme trigger de l'input pour que le super jump s'active")] private float superJumpInputMaxDelay;
    [SerializeField] private float superJumpVerticalForce;
    [SerializeField] private float superJumpHorizontalForce;
    [SerializeField] private float superJumpEnergyCost = 20f;


    [Header("wallRide")] [SerializeField] float wallRideDetectionRange = .5f;
    [SerializeField] float wallRidingDuration = 2f;
    [SerializeField] private float wallRideCooldown = .2f;
    [SerializeField] float wallRidingSpeed = 10f;
    [SerializeField] float minSpeedToWallRide = 1f;
    [SerializeField] float wallJumpVerticalForce = 10f;
    [SerializeField] float wallJumpHorizontalForce = 7.5f;
    [SerializeField] float headtiltIntensity = 7f;

    [Header("Crouch")] [SerializeField] float crouchSpeed = 5f;
    [SerializeField] float cameraOffsetWhenCrouching = 1f;
    [SerializeField] GameObject[] bodyStandUpCollider;
    [SerializeField] Transform topHeightStandUpCollider;
    [SerializeField] GameObject[] bodyCrouchedCollider;
    [SerializeField] Transform topHeightCrouchedCollider;

    [Header("Slide")] [SerializeField] float slideSpeed = 5f;
    [SerializeField] float slideTimeDuration = 0.2f;
    [SerializeField] float slideJumpVerticalForce = 6.5f;
    [SerializeField] float slideJumpHorizontalForce = 2f;
    [SerializeField] float slideCooldown = .1f;
    [SerializeField] float coyoteSlideDuration = .1f;
    [SerializeField] private float CameraSlideFOV = 50;

    [Header("Dash")] [SerializeField] float dashSpeed = 5f;
    [SerializeField] float dashTimeDuration = 0.2f;
    [SerializeField] float dashCooldown;
    [SerializeField] private float dashEnergyCost = 20f;

    [Header("Slope Slide")] [SerializeField]
    float minSlopeAngleToSlopeSlide = 11f;

    [SerializeField] float slopeSlideMaxSpeed = 11f;
    [SerializeField] float slopeInfluenceOnRotation = 3f;
    [SerializeField] float slopeInfluenceOnVelocity = .75f;

    [Header("Grapple")] [SerializeField] private float _castWidth = .5f;
    [SerializeField] private float _castMaxDistance = 100f;
    [SerializeField] private float _grapplingSpeed = 15;
    [SerializeField] float _grappleRedirectionSpeed = 8f;
    [SerializeField] private float _endGrappleImpulseForce = 3f;

    #endregion

    #region private variables

    private Transform _camTransform;

    private Transform _currentGrapplePoint;
    private float _cameraDefaultFOV;

    [HideInInspector] public bool grounded;
    [HideInInspector] public bool leftSideAgainstWall;
    [HideInInspector] public bool rightSideAgainstWall;
    RaycastHit leftSideHit;
    RaycastHit rightSideHit;
    RaycastHit groundedHit;
    [HideInInspector] public bool fellOffWallrinding;

    [HideInInspector] public Vector3 horizontalVelocity; //public uniquement pour le debugCanvas

    private bool justJumped;
    bool hasJumped;
    bool bufferJump = false;
    bool coyoteJump = false;
    bool hasDashed = false;
    bool coyoteSlide = false;
    bool enoughtEnegyToDash = false;
    bool enoughtEnegyToDoubleJump = false;
    bool isFreeze = false;
    private readonly SyncVar<bool> isDead = new SyncVar<bool>(false);

    public enum ControlerState
    {
        Idle,
        Moving,
        Falling,
        WallRiding,
        Crouching,
        Sliding,
        Dashing,
        SlopeSliding,
        Grappling
    }

    public StateMachine<ControlerState> stateMachine = new StateMachine<ControlerState>();

    private EventBus _bus;

    #endregion

    public override void OnStartClient()
    {
        base.OnStartClient();

        _bus = EventBusInitialiser.instance.Bus;

        if (IsOwner)
        {
            SetUpLayer();

            _camTransform = _camera.transform;
            _cameraDefaultFOV = _camera.fieldOfView;
            _camTransform.localPosition = Vector3.zero;

            _bus.Subscribe((RequestEnergyResponseEvent data) =>
            {
                enoughtEnegyToDash = data.energy >= dashEnergyCost;
            });
            
            _bus.Subscribe((RequestEnergyResponseEvent data) =>
            {
                enoughtEnegyToDoubleJump = data.energy >= superJumpEnergyCost;
            });

            _bus.Subscribe((OnPlayerDeathEvent data) =>
            {
                if (data.playerN == NetworkObject)
                    SetDeadServerRpc(true);
            });

            _bus.Subscribe((OnPlayerDeathEvent data) =>
            {
                if (data.playerN == NetworkObject)
                    SetDeadServerRpc(true);
            });

            _bus.Subscribe((OnPlayerRespawnEvent data) =>
            {
                if (data.playerN == NetworkObject)
                    SetDeadServerRpc(false);
            });
        }
        else
        {
            _camera.gameObject.SetActive(false);
        }

        //Cursor.lockState = CursorLockMode.Locked;

        yaw = transform.eulerAngles.y;
        pitch = cameraParentTransform.localEulerAngles.x;

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
            onEnter: OnEnterMovingState,
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
            onExit: ExitSlidingState,
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

        stateMachine.Add(new State<ControlerState>(
            iD: ControlerState.Grappling,
            onEnter: EnterGrappleState,
            onUpdate: GrappleUpdate,
            onFixedUpdate: GrappleFixedUpdate,
            onExit: ExitGrappleState,
            onLateUpdate: GrappleLateUpdate
        ));

        stateMachine.ChangeState(ControlerState.Idle);

        //debug

        capsuleTop = topHeightStandUpCollider.position;
        height = Vector3.Distance(capsuleTop, playerFeet.position);
        point1 = playerFeet.position + height * Vector3.up - Vector3.up * bodyRadius;
        point2 = playerFeet.position + Vector3.up * bodyRadius;
    }


    #region Function Calling

    void Update()
    {
        if (!IsOwner) return;

        if (isDead.Value) return;
        if (IsFreeze) return;
        
        UpdateInputs();
        UpdateLdInteractions();
        stateMachine?.Update();
        
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        if (isDead.Value) return;
        if (IsFreeze) return;

        stateMachine?.FixedUpdate();
    }

    void LateUpdate()
    {
        if (!IsOwner) return;

        if (isDead.Value) return;
        if(IsFreeze) return;

        stateMachine?.LateUpdate();
    }


    void UpdateInputs() // appelé en update dans tout les states
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
            ~LayerMask.GetMask("Owner"), QueryTriggerInteraction.Ignore) && !justJumped);


        leftSideAgainstWall = Physics.Raycast(playerLeftSide.position, playerLeftSide.forward,
            out leftSideHit, wallRideDetectionRange, ~LayerMask.GetMask("Owner"), QueryTriggerInteraction.Ignore);
        rightSideAgainstWall = Physics.Raycast(playerRightSide.position, playerRightSide.forward,
            out rightSideHit, wallRideDetectionRange, ~LayerMask.GetMask("Owner"), QueryTriggerInteraction.Ignore);

        horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    }

    private GrapplePoint currentLookedGrapplePoint;

    void UpdateLdInteractions() // appelé en update dans tout les states
    {
        // grapplepoints 
        if (Physics.SphereCast(cameraParentTransform.position, _castWidth, cameraParentTransform.forward,
                out RaycastHit hit, _castMaxDistance, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide))
        {
            currentLookedGrapplePoint = hit.collider.GetComponent<GrapplePoint>();
            if (currentLookedGrapplePoint != null)
            {
                currentLookedGrapplePoint.p_mustShowCanvas = true;
                currentLookedGrapplePoint.p_playerTransform = cameraParentTransform;
            }
        }
        else if (currentLookedGrapplePoint != null)
        {
            currentLookedGrapplePoint.p_mustShowCanvas = false;
            currentLookedGrapplePoint = null;
        }
    }

    #endregion

    #region States

    #region IdleState

    void EnterIdleState()
    {
        if (playerInput.actions["Crouch"].IsPressed())
        {
            if (Vector3.Angle(groundedHit.normal, Vector3.up) > minSlopeAngleToSlopeSlide && slopeSlideUnlocked)
            {
                stateMachine.ChangeState(ControlerState.SlopeSliding);
            }
            else if (!justSlided && slideUnlocked)
            {
                stateMachine.ChangeState(ControlerState.Sliding);
            }
        }

        if (bufferJump && playerInput.actions["Jump"].IsPressed() &&
            stateMachine.previousState == stateMachine.GetState(ControlerState.Falling)) Jump();

        if (stateMachine.previousState != stateMachine.GetState(ControlerState.Grappling))
        {
            if (!(stateMachine.previousState == stateMachine.GetState(ControlerState.Falling) &&
                  (verticalInput != 0f || horizontalInput != 0f)))
            {
                rb.linearVelocity = Vector3.zero;
            }
        }


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

        _bus.InvokeEvent(new RequestEnergyEvent { requester = this });

        if (playerInput.actions["Dash"].WasPressedThisFrame() && !hasDashed && !justDashed && dashUnlocked &&
            enoughtEnegyToDash)
        {
            stateMachine.ChangeState(ControlerState.Dashing);
        }

        if (playerInput.actions["Grapple"].WasPressedThisFrame())
        {
            if (Physics.SphereCast(cameraParentTransform.position, _castWidth, cameraParentTransform.forward,
                    out RaycastHit hit, _castMaxDistance, LayerMask.GetMask("Default"),
                    QueryTriggerInteraction.Collide))
            {
                if (hit.collider.GetComponent<GrapplePoint>() != null)
                {
                    _currentGrapplePoint = hit.collider.transform;
                    stateMachine.ChangeState(ControlerState.Grappling);
                }
            }
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
        defaultSmooting = rollSmoothing;
        rollSmoothing = landSnapVelocity;
        yield return new WaitForSeconds(.2f);
        rollSmoothing = defaultSmooting;
    }

    #endregion

    #region MovingState

    void OnEnterMovingState()
    {
        _playerAnimation.SetMovingAnim(true);
    }

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

        _bus.InvokeEvent(new RequestEnergyEvent { requester = this });

        if (playerInput.actions["Dash"].WasPressedThisFrame() && !hasDashed && !justDashed && dashUnlocked &&
            enoughtEnegyToDash)
        {
            stateMachine.ChangeState(ControlerState.Dashing);
        }

        if (playerInput.actions["Grapple"].WasPressedThisFrame())
        {
            if (Physics.SphereCast(cameraParentTransform.position, _castWidth, cameraParentTransform.forward,
                    out RaycastHit hit, _castMaxDistance, LayerMask.GetMask("Default"),
                    QueryTriggerInteraction.Collide))
            {
                if (hit.collider.GetComponent<GrapplePoint>() != null)
                {
                    stateMachine.ChangeState(ControlerState.Grappling);
                }
            }
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
        /* _playerAnimation.SetFallingAnim(false);
         _playerAnimation.SetGroundedAnim(true);*/

        _playerAnimation.ChangeAirState(false);

        if (!hasJumped)
            StartCoroutine(CoyoteTimeCoroutine());
    }

    void FallingUpdate()
    {
        if (grounded)
        {
            stateMachine.ChangeState(ControlerState.Idle);
        }

        if (playerInput.actions["Jump"].WasPressedThisFrame())
        {
            if (coyoteJump) Jump();
            else StartCoroutine(JumpBufferingCoroutine());
        }

        if (verticalInput > 0.1f && (leftSideAgainstWall || rightSideAgainstWall) &&
            horizontalVelocity.magnitude > minSpeedToWallRide && !grounded && !justWallRided && !fellOffWallrinding &&
            wallRideUnlocked)
        {
            if (rb.linearVelocity.y < 0) stateMachine.ChangeState(ControlerState.WallRiding);
            else mustHeadTilt = true;
        }

        _bus.InvokeEvent(new RequestEnergyEvent { requester = this });

        if (playerInput.actions["Dash"].WasPressedThisFrame() && !hasDashed && !justDashed && dashUnlocked &&
            enoughtEnegyToDash)
        {
            stateMachine.ChangeState(ControlerState.Dashing);
        }

        if (playerInput.actions["Grapple"].WasPressedThisFrame())
        {
            if (Physics.SphereCast(cameraParentTransform.position, _castWidth, cameraParentTransform.forward,
                    out RaycastHit hit, _castMaxDistance, LayerMask.GetMask("Default"),
                    QueryTriggerInteraction.Collide))
            {
                if (hit.collider.GetComponent<GrapplePoint>() != null)
                {
                    stateMachine.ChangeState(ControlerState.Grappling);
                }
            }
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
                            Vector3 newDir = Vector3.Slerp(currentDir, desiredDir,
                                airControlForce * Time.fixedDeltaTime);

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
        /* _playerAnimation.SetFallingAnim(false);
         _playerAnimation.SetGroundedAnim(true);*/

        _playerAnimation.ChangeAirState(true);

        hasJumped = false;
        mustHeadTilt = false;
        _playerAnimation.SetFallingAnim(false);
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
            cameraSpringTarget.rotation =
                cameraParentTransform.rotation = Quaternion.Euler(pitch, yaw, -headtiltIntensity);
        }
        else
        {
            wallRidingDirection = Vector3.Dot(Vector3.Cross(rightSideHit.normal, Vector3.up), rb.linearVelocity) *
                                  Vector3.Cross(rightSideHit.normal, Vector3.up);
            currentWallHit = rightSideHit;
            cameraSpringTarget.rotation =
                cameraParentTransform.rotation = Quaternion.Euler(pitch, yaw, headtiltIntensity);
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

        if (playerInput.actions["Grapple"].WasPressedThisFrame())
        {
            if (Physics.SphereCast(cameraParentTransform.position, _castWidth, cameraParentTransform.forward,
                    out RaycastHit hit, _castMaxDistance, LayerMask.GetMask("Default"),
                    QueryTriggerInteraction.Collide))
            {
                if (hit.collider.GetComponent<GrapplePoint>() != null)
                {
                    stateMachine.ChangeState(ControlerState.Grappling);
                }
            }
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
        cameraSpringTarget.rotation = Quaternion.Euler(pitch, yaw, 0);
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

        if (!playerInput.actions["Crouch"].IsPressed())
        {
            if (!Physics.Raycast(topHeightCrouchedCollider.position, Vector3.up,
                    Vector3.Distance(topHeightCrouchedCollider.position, topHeightStandUpCollider.position)))
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
    }

    void CrouchingFixedUpdate()
    {
        Vector3 move = (transform.forward * verticalInput + transform.right * horizontalInput).normalized;

        horizontalInput = playerInput.actions["Move"].ReadValue<Vector2>().x;
        verticalInput = playerInput.actions["Move"].ReadValue<Vector2>().y;

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
    private bool mustBeGrounded;
    Vector3 landingDirection;

    void EnterSlidingState()
    {
        landingDirection = cameraParentTransform.forward;
        landingDirection.y = 0f;
        landingDirection.Normalize();
        landingDirection *= slideSpeed;

        Crouch();
        StartCoroutine(SlidingCoroutine());
    }

    void SlidingUpdate()
    {
        if (!grounded && mustBeGrounded)
        {
            stateMachine.ChangeState(ControlerState.Falling);
            return;
        }

        if (playerInput.actions["Jump"].WasPressedThisFrame() && !jumpSlideOnEndOfSlide)
        {
            SlideJump();
            stateMachine.ChangeState(ControlerState.Idle);
            return;
        }

        if (!mustSlide)
        {
            if (!Physics.Raycast(topHeightCrouchedCollider.position, Vector3.up,
                    Vector3.Distance(topHeightCrouchedCollider.position, topHeightStandUpCollider.position)))
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
    }

    void SlidingFixedUpdate()
    {
        landingDirection = AlignVelocityToWall(landingDirection, true);
        landingDirection = InterpolateSlope(landingDirection);
        rb.linearVelocity = landingDirection;
    }

    void ExitSlidingState()
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

        mustBeGrounded = false;
        float elapsedTime = 0;
        float startFOV = _camera.fieldOfView;

        while (elapsedTime < slideTimeDuration)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime < 0.1f)
            {
                float t = elapsedTime / 0.1f;
                _camera.fieldOfView = Mathf.Lerp(startFOV, CameraSlideFOV, t);
            }
            else
            {
                mustBeGrounded = true;
            }

            yield return null;
        }

        mustSlide = false;
    }

    IEnumerator JustSlidedCoroutine()
    {
        justSlided = true;

        float elapsedTime = 0;
        float startFOV = _camera.fieldOfView;

        while (elapsedTime < slideCooldown)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime < 0.1f)
            {
                float t = elapsedTime / 0.1f;
                _camera.fieldOfView = Mathf.Lerp(startFOV, _cameraDefaultFOV, t);
            }

            yield return null;
        }

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
            dashingDirection = cameraParentTransform.forward;
        }
        else
        {
            if (verticalInput == 0f && horizontalInput == 0f) dashingDirection = _camera.transform.forward;
            else dashingDirection = (transform.forward * verticalInput + transform.right * horizontalInput).normalized;
        }

        _bus.InvokeEvent(new OnModifyEnergyEvent { value = -dashEnergyCost });

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
            ? Vector3.ProjectOnPlane(rb.linearVelocity, groundedHit.normal).normalized * rb.linearVelocity.magnitude
            : slopeDirection;

        Debug.Log(currentDirection == slopeDirection);
        Debug.Log(slopeDirection);

        Crouch();
    }

    void SlopeSlidingUpdate()
    {
        if (!grounded) stateMachine.ChangeState(ControlerState.Falling);
        if (!Physics.Raycast(topHeightCrouchedCollider.position, Vector3.up,
                Vector3.Distance(topHeightCrouchedCollider.position, topHeightStandUpCollider.position)))
        {
            if (playerInput.actions["Crouch"].WasReleasedThisFrame()) stateMachine.ChangeState(ControlerState.Idle);
            if (playerInput.actions["Jump"].WasPressedThisFrame()) SlideJump(); // au besoin, faire une autre fonction
            if (Vector3.Angle(groundedHit.normal, Vector3.up) < minSlopeAngleToSlopeSlide)
                stateMachine.ChangeState(ControlerState.Idle);
        }
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
            currentDirection *= 1 + (1 / Mathf.Max(Vector3.Angle(slopeDirection, currentDirection), 1f)) *
                slopeInfluenceOnVelocity * Vector3.Angle(groundedHit.normal, Vector3.up);
        }

        Vector3 newVelocity = currentDirection.magnitude > slopeSlideMaxSpeed
            ? currentDirection.normalized * slopeSlideMaxSpeed
            : currentDirection;

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

    #region GrappleState

    Vector3 grappleDirection;

    void EnterGrappleState()
    {
        if (Physics.SphereCast(cameraParentTransform.position, _castWidth, cameraParentTransform.forward,
                out RaycastHit hit, _castMaxDistance, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide))
        {
            if (hit.collider.GetComponent<GrapplePoint>() != null)
            {
                _currentGrapplePoint = hit.collider.transform;
            }
        }
        else //ne devrait pas etre appelé
        {
            stateMachine.ChangeState(ControlerState.Idle);
        }

        grappleDirection = (_currentGrapplePoint.position - transform.position).normalized;
    }

    void GrappleUpdate()
    {
        if (!(Vector3.Distance(transform.position, _currentGrapplePoint.position) > 0.5f &&
              playerInput.actions["Grapple"].IsPressed()))
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(grappleDirection * _endGrappleImpulseForce, ForceMode.Impulse);
            _currentGrapplePoint = null;
            stateMachine.ChangeState(ControlerState.Idle);
        }
    }

    void GrappleFixedUpdate()
    {
        grappleDirection = (_currentGrapplePoint.position - transform.position).normalized;
        Vector3 newDir = Vector3.Slerp(rb.linearVelocity.normalized, grappleDirection,
            _grappleRedirectionSpeed * Time.fixedDeltaTime);

        rb.linearVelocity = newDir * _grapplingSpeed;
    }

    void ExitGrappleState()
    {
    }

    void GrappleLateUpdate()
    {
        UpdateCameraPositionAndRotation();
    }

    #endregion

    #endregion

    #region PlayerActions

    private float currentRoll = 0f;
    private Vector3 camVelocity = Vector3.zero;
    private Vector3 camNextPos;
    private Vector3 bobOffset = Vector3.zero;

    void UpdateCameraPositionAndRotation(bool headbob = false, float headbobAmplitude = 0f, float headbobFrequency = 0f)
    {
        transform.rotation = Quaternion.Euler(0, yaw, 0);

        float targetRoll = cameraSpringTarget.eulerAngles.z;
        currentRoll = Mathf.LerpAngle(currentRoll, targetRoll, rollSmoothing * Time.deltaTime);

        cameraParentTransform.rotation = Quaternion.Euler(pitch, yaw, currentRoll);

        Vector3 targetPos = cameraSpringTarget.position;

        if (headbob)
        {
            headbobTimer += Time.deltaTime * headbobFrequency;

            float bobY = Mathf.Sin(headbobTimer) * headbobAmplitude;
            float bobX = Mathf.Cos(headbobTimer * 0.5f) * headbobAmplitude * 0.5f;
            
            bobOffset = new Vector3(bobX, bobY, 0);
        }
        else
        {
            bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, Time.deltaTime * headbobStopReturningSpeed);
            headbobTimer = 0f;
        }
        targetPos += bobOffset;
        Spring(ref camNextPos, ref camVelocity, targetPos, cameraSpringHalfLife, cameraSpringFrequency, Time.deltaTime);
        cameraParentTransform.position = camNextPos;
    }


    public void Spring(ref Vector3 current, ref Vector3 velocity, Vector3 target, float halfLife, float frequency,
        float timeStep)
    {
        var dampingRatio = -Mathf.Log(0.5f) / (frequency * halfLife);
        var f = 1.0f + 2.0f * timeStep * dampingRatio * frequency;
        var oo = frequency * frequency;
        var hoo = timeStep * oo;
        var hhoo = timeStep * hoo;
        var detInv = 1.0f / (f + hhoo);
        var detX = f * current + timeStep * velocity + hhoo * target;
        var detV = velocity + hoo * (target - current);
        current = detX * detInv;
        velocity = detV * detInv;
    }


    private Vector3 currentHorizontal;
    private Vector3 capsuleTop;
    private float height;
    private Vector3 point1;
    private Vector3 point2;
    private CapsuleCollider capsule;

    Vector3 AlignVelocityToWall(Vector3 velocity, bool crouched = false)
    {
        if (velocity.sqrMagnitude < .1f) return velocity;
        capsuleTop = crouched ? topHeightCrouchedCollider.position : topHeightStandUpCollider.position;
        height = Vector3.Distance(capsuleTop, playerFeet.position);
        point1 = playerFeet.position + height * Vector3.up - Vector3.up * bodyRadius;
        point2 = playerFeet.position + Vector3.up * bodyRadius;

        currentHorizontal = new Vector3(velocity.x, 0, velocity.z);
        if (Physics.CapsuleCast(point1, point2, bodyRadius, currentHorizontal.normalized, out RaycastHit hit,
                wallDetectionRange, ~LayerMask.GetMask("Owner")))
        {
            // Si le contact est bas , pente / sol
            float hitHeight = hit.point.y - playerFeet.position.y;

            if (hitHeight < maxStepHeight)
                return new Vector3(velocity.x, velocity.y + (maxStepHeight - hitHeight), velocity.z);

            float hitSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            // Pente marchable , ne pas bloquer
            if (hitSlopeAngle <= walkableSlopeAngle) return velocity;

            Vector3 aligned = Vector3.ProjectOnPlane(currentHorizontal, hit.normal);

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
        StartCoroutine(SuperJumpCoroutine());
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    IEnumerator JumpAntiLagCoroutine()
    {
        _playerAnimation.SetJumpAnim(true);
        justJumped = true;
        yield return new WaitForSeconds(.1f);
        justJumped = false;
        _playerAnimation.SetJumpAnim(false);
    }

    IEnumerator SuperJumpCoroutine()
    {
        float elapsedTime = 0.1f;
        yield return new WaitForSeconds(0.1f);
        while (elapsedTime < superJumpInputMaxDelay && !grounded)
        {
            elapsedTime += Time.deltaTime;
            if (playerInput.actions["Jump"].WasPressedThisFrame())
            {
                SuperJump();
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void SuperJump()
    {
        if(enoughtEnegyToDoubleJump)
        {
            rb.AddForce(Vector3.up * superJumpVerticalForce + transform.forward * superJumpHorizontalForce, ForceMode.Impulse);
            _bus.InvokeEvent(new OnModifyEnergyEvent { value = -superJumpEnergyCost });
        }
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
        rb.AddForce(Vector3.up * slideJumpVerticalForce + horizontalVelocity * slideJumpHorizontalForce,
            ForceMode.Impulse);
    }

    void Crouch()
    {
        cameraSpringTarget.position =
            new Vector3(cameraSpringTarget.position.x, cameraSpringTarget.position.y - cameraOffsetWhenCrouching,
                cameraSpringTarget.position.z);
        foreach (GameObject col in bodyStandUpCollider) col.SetActive(false);
        foreach (GameObject col in bodyCrouchedCollider) col.SetActive(true);
    }

    void UnCrouch()
    {
        cameraSpringTarget.position =
            new Vector3(cameraSpringTarget.position.x, cameraSpringTarget.position.y + cameraOffsetWhenCrouching,
                cameraSpringTarget.position.z);
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

    public void OnGetEnergy(float energy)
    {
        Debug.Log("OnGetEnergy");
        enoughtEnegyToDash = energy - dashEnergyCost >= 0;
    }

    [ServerRpc]
    private void SetDeadServerRpc(bool value)
    {
        isDead.Value = value;
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(point1, bodyRadius);
        Gizmos.DrawWireSphere(point2, bodyRadius);
    }
}