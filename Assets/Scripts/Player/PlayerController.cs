using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private CameraController cameraController;
    private Animator animator;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSharpness = 10f;
    [SerializeField] private float animatorLocomotionSmoothTime = 0.12f;
    private Vector2 currentInput;
    private Vector2 smoothedAnimatorInput;
    private Vector2 animatorSmoothVelocity;
    private Vector3 movement;
    private float gravity = -9.81f;
    private float yVelocity;
    private PlayerStatus playerStatus;

    private const float GroundedStickForce = -2f;
    private const float MinMoveSqrMagnitude = 0.01f;

    public PlayerStatus PlayerStatus { get { return playerStatus; } set { playerStatus = value; } }

    public void SetMovement(Vector2 input)
    {
        currentInput = input;
        if (input.sqrMagnitude > MinMoveSqrMagnitude)
            PlayerStatus = PlayerStatus.Walking;
        else
            PlayerStatus = PlayerStatus.Idle;
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
        CalculateMovement();
        UpdateSmoothedAnimatorParameters();
        ApplyMovement();
    }

    private void UpdateSmoothedAnimatorParameters()
    {
        if (animator == null)
            return;

        Vector2 target = currentInput.sqrMagnitude > MinMoveSqrMagnitude ? currentInput : Vector2.zero;
        float smooth = Mathf.Max(0.0001f, animatorLocomotionSmoothTime);
        smoothedAnimatorInput = Vector2.SmoothDamp(
            smoothedAnimatorInput,
            target,
            ref animatorSmoothVelocity,
            smooth);

        animator.SetFloat("Horizontal", smoothedAnimatorInput.x);
        animator.SetFloat("Vertical", smoothedAnimatorInput.y);
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

        Vector3 finalMove = movement * moveSpeed;
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
        if (PlayerStatus == PlayerStatus.Aiming)
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
        else
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

