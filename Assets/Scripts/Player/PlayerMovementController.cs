using System.Collections;
using Prime31;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    // ---------- COMPONENTS ----------
    private CharacterController2D controller;

    // ---------- REFERENCES ----------
    [Header("References")]
    [SerializeField]
    private CameraLookDirection cameraLookDirection;

    // ---------- MOVEMENT ----------
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 6f;

    [SerializeField]
    private float gravity = -20f;

    // ---------- STATE ----------
    private Vector2 moveDirection;
    private Vector3 velocity;
    private PlayerState currentState = PlayerState.Idle;

    private float originalGravityScale;
    private bool isExternallyMoving;

    public bool CanMove => currentState == PlayerState.Idle || currentState == PlayerState.Walking;

    public PlayerState CurrentState
    {
        get => currentState;
        set => currentState = value;
    }

    public Vector3 Velocity
    {
        get => velocity;
        set => velocity = value;
    }

    public bool IsExternallyMoving
    {
        get => isExternallyMoving;
        set => isExternallyMoving = value;
    }

    public float Gravity => gravity;

    public float OriginalGravityScale => originalGravityScale;

    public CharacterController2D Controller => controller;

    // ---------- UNITY ----------
    private void Awake()
    {
        controller = GetComponent<CharacterController2D>();
        originalGravityScale = controller.rigidBody2D.gravityScale;
    }

    private void Update()
    {
        if (currentState == PlayerState.Hanging)
            return;

        HandleFlip();
    }

    private void FixedUpdate()
    {
        if (isExternallyMoving)
            return;

        HandleMovement();
    }

    // ---------- INPUT ----------
    public void Move(Vector2 direction)
    {
        moveDirection = direction;
    }

    // ---------- MOVEMENT ----------
    private void HandleMovement()
    {
        if (!CanMove)
            return;

        velocity.x = moveDirection.x * moveSpeed;

        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = 0;

            currentState =
                Mathf.Abs(moveDirection.x) > 0.01f ? PlayerState.Walking : PlayerState.Idle;
        }
        else
        {
            velocity.y += gravity * Time.fixedDeltaTime;
        }

        controller.move(velocity * Time.fixedDeltaTime);
    }

    private void HandleFlip()
    {
        if (moveDirection.x == 0)
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(moveDirection.x) * Mathf.Abs(scale.x);
        transform.localScale = scale;

        if (cameraLookDirection != null)
            cameraLookDirection.LookDirection(scale.x > 0);
    }

    // ---------- MOVEMENT HELPERS ----------
    public IEnumerator EnsureGrounded()
    {
        while (!controller.isGrounded)
        {
            velocity.y += gravity * Time.fixedDeltaTime;
            controller.move(velocity * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        velocity.y = 0;
    }
}
