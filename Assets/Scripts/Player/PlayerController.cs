using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private CameraController cameraController;
    private Animator animator;

    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeedOutsideCombat = 5f;
    [SerializeField] private float runSpeedInCombat = 8f;
    [SerializeField] private float rotationSharpness = 10f;
    [SerializeField] private float animatorLocomotionSmoothTime = 0.12f;
    [SerializeField][Range(0.01f, 1f)] private float animatorSpeedSmoothTime = 0.18f;

    [Header("Move speed multipliers")]
    [SerializeField][Range(0.05f, 1f)] private float aimMoveSpeedMultiplier = 0.3f;
    [SerializeField][Range(0.05f, 1f)] private float crouchMoveSpeedMultiplier = 0.5f;
    [SerializeField][Range(0.05f, 1f)] private float crouchAimMoveSpeedMultiplier = 0.2f;

    [Header("Animator locomotion Speed")]
    [SerializeField][Range(0f, 1f)] private float animatorSpeedWalk = 0.3f;
    [SerializeField][Range(0f, 1f)] private float animatorSpeedRunExploration = 0.6f;
    [SerializeField][Range(0f, 1f)] private float animatorSpeedRunCombat = 1f;

    private Vector2 currentInput;
    private bool sprintHeld;
    private float locomotionSpeedTarget;
    private float smoothedAnimatorSpeed;
    private float animatorSpeedSmoothVelocity;
    private Vector2 smoothedAnimatorInput;
    private Vector2 animatorSmoothVelocity;
    private Vector3 movement;
    private float gravity = -9.81f;
    private float yVelocity;
    private PlayerStatus playerStatus;
    private GameStatus gameStatus;

    private const float GroundedStickForce = -2f;
    private const float MinMoveSqrMagnitude = 0.01f;

    private static bool IsAimingStatus(PlayerStatus s) =>
        s == PlayerStatus.Aiming || s == PlayerStatus.CrounchAiming;

    private static bool IsCrouchedPose(PlayerStatus s) =>
        s == PlayerStatus.Crounched || s == PlayerStatus.CrounchAiming;

    public PlayerStatus PlayerStatus { get { return playerStatus; } set { playerStatus = value; } }

    public void SetMovementInput(Vector2 input)
    {
        currentInput = input;
    }

    public void SetSprint(bool sprintHeld)
    {
        this.sprintHeld = sprintHeld;
    }

    void SyncLocomotionFromInput()
    {
        if (IsAimingStatus(playerStatus) || playerStatus == PlayerStatus.Inventory)
        {
            locomotionSpeedTarget = smoothedAnimatorSpeed;
            return;
        }

        if (playerStatus == PlayerStatus.Crounched)
        {
            locomotionSpeedTarget = smoothedAnimatorSpeed;
            return;
        }

        bool hasMovement = currentInput.sqrMagnitude > MinMoveSqrMagnitude;

        if (!hasMovement)
        {
            playerStatus = PlayerStatus.Idle;
            locomotionSpeedTarget = 0f;
            return;
        }

        if (sprintHeld)
        {
            playerStatus = PlayerStatus.Running;
            locomotionSpeedTarget = gameStatus == GameStatus.Combat ? animatorSpeedRunCombat : animatorSpeedRunExploration;
        }
        else
        {
            playerStatus = PlayerStatus.Walking;
            locomotionSpeedTarget = animatorSpeedWalk;
        }
    }

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        cameraController = GetComponentInChildren<CameraController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        PromoteCrouchAimToStandingAimIfMoving();
        SyncLocomotionFromInput();
        CalculateMovement();
        UpdateSmoothedAnimatorParameters();
        ApplyMovement();
    }

    void PromoteCrouchAimToStandingAimIfMoving()
    {
        if (playerStatus != PlayerStatus.CrounchAiming)
            return;
        if (currentInput.sqrMagnitude <= MinMoveSqrMagnitude)
            return;

        playerStatus = PlayerStatus.Aiming;
    }

    private void UpdateSmoothedAnimatorParameters()
    {
        if (animator == null)
            return;

        Vector2 target = IsAimingStatus(PlayerStatus)
            ? Vector2.zero
            : (currentInput.sqrMagnitude > MinMoveSqrMagnitude ? currentInput : Vector2.zero);

        if (PlayerStatus == PlayerStatus.CrounchAiming)
        {
            smoothedAnimatorInput = Vector2.zero;
            animatorSmoothVelocity = Vector2.zero;
        }
        else
        {
            float smooth = Mathf.Max(0.0001f, animatorLocomotionSmoothTime);
            smoothedAnimatorInput = Vector2.SmoothDamp(
                smoothedAnimatorInput,
                target,
                ref animatorSmoothVelocity,
                smooth);
        }

        animator.SetBool("IsCrouching", IsCrouchedPose(playerStatus));
        animator.SetFloat("MoveX", smoothedAnimatorInput.x);
        animator.SetFloat("MoveY", smoothedAnimatorInput.y);

        float speedSmooth = Mathf.Max(0.0001f, animatorSpeedSmoothTime);
        smoothedAnimatorSpeed = Mathf.SmoothDamp(
            smoothedAnimatorSpeed,
            locomotionSpeedTarget,
            ref animatorSpeedSmoothVelocity,
            speedSmooth);
        animator.SetFloat("Speed", smoothedAnimatorSpeed);
    }

    private void CalculateMovement()
    {
        GetCameraPlanarAxes(out Vector3 forward, out Vector3 right);
        movement = forward * currentInput.y + right * currentInput.x;
        movement = Vector3.ClampMagnitude(movement, 1f);
    }

    private void ApplyMovement()
    {
        ApplyGravity();
        HandleRotation();

        float speedMul = playerStatus switch
        {
            PlayerStatus.Aiming => aimMoveSpeedMultiplier,
            PlayerStatus.Crounched => crouchMoveSpeedMultiplier,
            PlayerStatus.CrounchAiming => crouchAimMoveSpeedMultiplier,
            _ => 1f,
        };

        float runSpeed = gameStatus == GameStatus.Combat ? runSpeedInCombat : runSpeedOutsideCombat;
        float moveSpeed = playerStatus == PlayerStatus.Running ? runSpeed : walkSpeed;


        Vector3 finalMove = movement * moveSpeed * speedMul;
        finalMove.y = yVelocity;
        characterController.Move(finalMove * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && yVelocity < 0)
        {
            yVelocity = GroundedStickForce;
        }

        yVelocity += gravity * Time.deltaTime;
    }

    private void HandleRotation()
    {
        if (IsAimingStatus(PlayerStatus))
        {
            HandleAimingRotation();
            return;
        }

        HandleDefaultRotation();
    }

    private void HandleAimingRotation()
    {
        Vector3 aimDirection = GetPlanarForwardFromCameraYaw();
        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude < MinMoveSqrMagnitude)
            return;

        RotateTowards(aimDirection);
    }

    private void HandleDefaultRotation()
    {
        if (movement.sqrMagnitude > MinMoveSqrMagnitude)
        {
            Vector3 faceDirection = GetPlanarForwardFromCameraYaw();
            faceDirection.y = 0f;

            if (faceDirection.sqrMagnitude < MinMoveSqrMagnitude)
                return;

            RotateTowards(faceDirection);
        }
        else if (playerStatus == PlayerStatus.Walking || playerStatus == PlayerStatus.Running)
        {
            PlayerStatus = PlayerStatus.Idle;
        }
    }

    private void GetCameraPlanarAxes(out Vector3 forward, out Vector3 right)
    {
        forward = GetPlanarForwardFromCameraYaw();
        right = Vector3.Cross(Vector3.up, forward).normalized;
    }

    private Vector3 GetPlanarForwardFromCameraYaw()
    {
        if (cameraController != null)
        {
            Quaternion yawOnly = Quaternion.AngleAxis(cameraController.HorizontalYaw, Vector3.up);
            Vector3 f = yawOnly * Vector3.forward;
            f.y = 0f;
            return f.sqrMagnitude > MinMoveSqrMagnitude ? f.normalized : Vector3.forward;
        }

        Transform rig = transform.childCount > 0 ? transform.GetChild(0) : transform;
        Vector3 forward = rig.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > MinMoveSqrMagnitude ? forward.normalized : Vector3.forward;
    }

    private void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            1 - Mathf.Exp(-rotationSharpness * Time.deltaTime)
        );

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

}

