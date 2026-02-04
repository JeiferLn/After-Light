using System.Collections;
using Prime31;
using UnityEngine;

public class PlayerController : MonoBehaviour
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

    // ---------- OBSTACLE DETECTION ----------
    [Header("Obstacle Detection")]
    [SerializeField]
    private float checkDistance = 0.6f;

    [SerializeField]
    private LayerMask obstacleLayer;

    // ---------- STATE ----------
    private Vector2 moveDirection;
    private Vector3 velocity;
    private PlayerState currentState = PlayerState.Idle;

    public bool CanMove => currentState == PlayerState.Idle || currentState == PlayerState.Walking;

    private ClimbableObstacle currentObstacle;

    private float originalGravityScale;
    private bool canMakeHangDecision;

    private bool isExternallyMoving;

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
    public void Move(Vector2 moveDirection)
    {
        this.moveDirection = moveDirection;
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

    // ---------- CLIMB ----------
    public void TryClimb()
    {
        if (!CanMove)
            return;

        Vector2 direction = Vector2.right * Mathf.Sign(transform.localScale.x);
        Vector2 origin = transform.position + Vector3.up * controller.skinWidth;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, checkDistance, obstacleLayer);

        if (!hit)
            return;

        ClimbableObstacle obstacle = hit.collider.GetComponent<ClimbableObstacle>();
        if (obstacle == null)
            return;

        StartCoroutine(ExecuteClimb(obstacle));
    }

    // ---------- CLIMB LOGIC ----------
    private IEnumerator ExecuteClimb(ClimbableObstacle obstacle)
    {
        isExternallyMoving = true;
        currentState = PlayerState.Climbing;
        velocity = Vector3.zero;

        controller.rigidBody2D.gravityScale = 0f;

        if (obstacle.alignPoint != null)
        {
            Vector3 targetX = new Vector3(
                obstacle.alignPoint.position.x,
                transform.position.y,
                transform.position.z
            );

            yield return MoveTo(targetX);
        }

        if (obstacle.exitPoint != null)
        {
            Vector3 targetY = new Vector3(
                transform.position.x,
                obstacle.exitPoint.position.y,
                transform.position.z
            );

            yield return MoveTo(targetY);
        }

        float forwardOffset = 0.6f * Mathf.Sign(transform.localScale.x);
        Vector3 forwardTarget = transform.position + Vector3.right * forwardOffset;
        yield return MoveTo(forwardTarget);

        controller.rigidBody2D.gravityScale = originalGravityScale;

        yield return EnsureGrounded();

        currentState = PlayerState.Idle;
        isExternallyMoving = false;
    }

    // ---------- DROP DOWN LOGIC ----------
    public void TryDrop()
    {
        if (currentState != PlayerState.Idle && currentState != PlayerState.Walking)
            return;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            checkDistance,
            obstacleLayer
        );

        if (!hit)
            return;

        ClimbableObstacle obstacle = hit.collider.GetComponent<ClimbableObstacle>();
        if (obstacle == null || obstacle.hangPoint == null)
            return;

        currentObstacle = obstacle;
        StartCoroutine(ExecuteHang(obstacle));
    }

    public void HangClimbUp()
    {
        if (currentState != PlayerState.Hanging || !canMakeHangDecision)
            return;

        StartCoroutine(ClimbBackUp(currentObstacle));
    }

    public void HangDropDown()
    {
        if (currentState != PlayerState.Hanging || !canMakeHangDecision)
            return;

        StartCoroutine(DropDown(currentObstacle));
    }

    // ---------- DROP DOWN HELPER ----------
    private IEnumerator ExecuteHang(ClimbableObstacle obstacle)
    {
        isExternallyMoving = true;
        canMakeHangDecision = false;
        currentState = PlayerState.Hanging;

        velocity = Vector3.zero;

        transform.localScale = new Vector3(
            -transform.localScale.x,
            transform.localScale.y,
            transform.localScale.z
        );

        controller.rigidBody2D.gravityScale = 0f;
        controller.rigidBody2D.linearVelocity = Vector2.zero;

        Vector3 targetX = new Vector3(
            obstacle.hangPoint.position.x,
            transform.position.y,
            transform.position.z
        );
        yield return MoveTo(targetX);

        Vector3 targetY = new Vector3(
            transform.position.x,
            obstacle.hangPoint.position.y,
            transform.position.z
        );
        yield return MoveTo(targetY);

        canMakeHangDecision = true;
    }

    private IEnumerator DropDown(ClimbableObstacle obstacle)
    {
        canMakeHangDecision = false;
        currentState = PlayerState.Droping;

        controller.rigidBody2D.gravityScale = originalGravityScale;
        isExternallyMoving = false;

        if (obstacle.dropPoint != null)
            yield return MoveTo(obstacle.dropPoint.position);

        yield return EnsureGrounded();

        currentState = PlayerState.Idle;
    }

    private IEnumerator ClimbBackUp(ClimbableObstacle obstacle)
    {
        canMakeHangDecision = false;
        currentState = PlayerState.Climbing;

        controller.rigidBody2D.gravityScale = originalGravityScale;

        if (obstacle.topPoint != null)
        {
            Vector3 targetY = new Vector3(
                transform.position.x,
                obstacle.topPoint.position.y,
                transform.position.z
            );
            yield return MoveTo(targetY);

            Vector3 targetX = new Vector3(
                obstacle.topPoint.position.x,
                transform.position.y,
                transform.position.z
            );
            yield return MoveTo(targetX);
        }

        isExternallyMoving = false;
        currentState = PlayerState.Idle;
    }

    // ---------- MOVEMENT HELPERS ----------
    private IEnumerator MoveTo(Vector3 target)
    {
        float duration = 0.25f;
        float elapsed = 0f;
        Vector3 start = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        transform.position = target;
    }

    private IEnumerator EnsureGrounded()
    {
        while (!controller.isGrounded)
        {
            velocity.y += gravity * Time.fixedDeltaTime;
            controller.move(velocity * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        velocity.y = 0;
    }

    // ---------- DEBUG ----------
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.yellow;

        Vector2 direction = Vector2.right * Mathf.Sign(transform.localScale.x);
        Vector2 origin = transform.position;

        Gizmos.DrawLine(origin, origin + direction * checkDistance);
    }
}
