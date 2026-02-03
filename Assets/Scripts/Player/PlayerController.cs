using System.Collections;
using Prime31;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ---------- COMPONENTS ----------
    private CharacterController2D controller;

    // ---------- MOVEMENT ----------
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 6f;

    [SerializeField]
    private float gravity = -30f;

    // ---------- OBSTACLE DETECTION ----------
    [Header("Obstacle Detection")]
    [SerializeField]
    private float frontCheckDistance = 0.6f;

    [SerializeField]
    private LayerMask obstacleLayer;

    // ---------- STATE ----------
    private Vector2 moveDirection;
    private Vector3 velocity;
    private PlayerState currentState = PlayerState.Idle;

    public bool CanMove => currentState == PlayerState.Idle || currentState == PlayerState.Walking;

    // ---------- UNITY ----------
    private void Awake()
    {
        controller = GetComponent<CharacterController2D>();
    }

    private void Update()
    {
        HandleMovement();
        HandleFlip();
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
            velocity.y += gravity * Time.deltaTime;
        }

        controller.move(velocity * Time.deltaTime);
    }

    private void HandleFlip()
    {
        if (moveDirection.x == 0)
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(moveDirection.x) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    // ---------- CLIMB ----------
    public void TryClimb()
    {
        if (!CanMove)
            return;

        Vector2 direction = Vector2.right * Mathf.Sign(transform.localScale.x);
        Vector2 origin = transform.position + Vector3.up * controller.skinWidth;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, frontCheckDistance, obstacleLayer);

        if (!hit)
            return;

        ClimbableObstacle obstacle = hit.collider.GetComponent<ClimbableObstacle>();
        if (obstacle == null)
            return;

        StartCoroutine(ClimbObstacle(obstacle));
    }

    // ---------- CLIMB COROUTINE ----------
    private IEnumerator ClimbObstacle(ClimbableObstacle obstacle)
    {
        currentState = PlayerState.Climbing;
        velocity = Vector3.zero;

        if (obstacle.startPoint != null)
            yield return MoveToPosition(obstacle.startPoint.position);

        if (obstacle.endPoint != null)
            yield return MoveToPosition(obstacle.endPoint.position);

        currentState = PlayerState.Idle;
    }

    // ---------- MOVE TO POSITION ----------
    private IEnumerator MoveToPosition(Vector3 target)
    {
        float duration = 0.35f;
        float elapsed = 0f;
        Vector3 start = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 pos = Vector3.Lerp(start, target, t);
            controller.move(pos - transform.position);

            yield return null;
        }
    }
}
