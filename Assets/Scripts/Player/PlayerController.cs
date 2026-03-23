using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;

    private Vector2 currentInput;
    private Vector3 movement;
    private float gravity = -9.81f;
    private float yVelocity;
    private PlayerStatus playerStatus;

    private const float GroundedStickForce = -2f;
    private const float MinMoveSqrMagnitude = 0.01f;
    private const float RotationMultiplier = 100f;

    public PlayerStatus PlayerStatus { get { return playerStatus; } set { playerStatus = value; } }

    public void SetMovement(Vector2 input)
    {
        currentInput = input;
        PlayerStatus = PlayerStatus.Walking;
    }

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        CalculateMovement();
        ApplyMovement();
    }

    void CalculateMovement()
    {
        GetCameraPlanarAxes(out Vector3 forward, out Vector3 right);
        movement = forward * currentInput.y + right * currentInput.x;
        movement = Vector3.ClampMagnitude(movement, 1f);
    }

    void ApplyMovement()
    {
        ApplyGravity();
        HandleRotation();

        Vector3 finalMove = movement * moveSpeed;
        finalMove.y = yVelocity;
        characterController.Move(finalMove * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (characterController.isGrounded && yVelocity < 0)
        {
            yVelocity = GroundedStickForce;
        }

        yVelocity += gravity * Time.deltaTime;
    }

    void HandleRotation()
    {
        if (PlayerStatus == PlayerStatus.Aiming)
        {
            HandleAimingRotation();
            return;
        }

        HandleDefaultRotation();
    }

    void HandleAimingRotation()
    {
        Vector3 aimDirection = cameraTransform.forward;
        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude < MinMoveSqrMagnitude)
            return;

        RotateTowards(aimDirection);
    }

    void HandleDefaultRotation()
    {
        if (movement.sqrMagnitude > MinMoveSqrMagnitude)
        {
            GetCameraPlanarAxes(out Vector3 forward, out Vector3 right);

            float forwardInput = Mathf.Max(0f, currentInput.y);

            Vector3 lookDirection = forward * forwardInput + right * currentInput.x;

            if (lookDirection.sqrMagnitude > MinMoveSqrMagnitude)
            {
                RotateTowards(lookDirection);
            }
        }
        else
        {
            PlayerStatus = PlayerStatus.Idle;
        }
    }

    void GetCameraPlanarAxes(out Vector3 forward, out Vector3 right)
    {
        forward = cameraTransform.forward;
        right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();
    }

    void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * RotationMultiplier * Time.deltaTime
        );
    }
}