using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSharpness = 10f;

    private Vector2 currentInput;
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
        PlayerStatus = input.sqrMagnitude > MinMoveSqrMagnitude ? PlayerStatus.Walking : PlayerStatus.Idle;
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
        Vector3 aimDirection = cameraTransform.forward;
        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude < MinMoveSqrMagnitude)
            return;

        RotateTowards(aimDirection);
    }

    private void HandleDefaultRotation()
    {
        if (movement.sqrMagnitude > MinMoveSqrMagnitude)
        {
            Vector3 lookDirection = movement;

            RotateTowards(lookDirection);
        }
        else
        {
            PlayerStatus = PlayerStatus.Idle;
        }
    }

    private void GetCameraPlanarAxes(out Vector3 forward, out Vector3 right)
    {
        forward = cameraTransform.forward;
        right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();
    }

    private void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            1 - Mathf.Exp(-rotationSharpness * Time.deltaTime)
        );
    }
}