using System.Collections;
using Prime31;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(CharacterController2D))]
public class PlayerClimbAndDropController : MonoBehaviour
{
    // ---------- COMPONENTS ----------
    private PlayerMovementController movement;
    private CharacterController2D controller;

    // ---------- OBSTACLE DETECTION ----------
    [Header("Obstacle Detection")]
    [SerializeField]
    private float checkDistance = 0.6f;

    [SerializeField]
    private LayerMask obstacleLayer;

    // ---------- STATE ----------
    private ClimbableObstacle currentObstacle;
    private bool canMakeHangDecision;

    // ---------- UNITY ----------
    private void Awake()
    {
        movement = GetComponent<PlayerMovementController>();
        controller = GetComponent<CharacterController2D>();

        if (movement == null || controller == null)
        {
            enabled = false;
        }
    }

    // ---------- CLIMB ----------
    public void TryClimb()
    {
        if (movement == null || controller == null)
            return;

        if (!movement.CanMove)
            return;

        Vector2 direction = Vector2.right * Mathf.Sign(transform.localScale.x);
        Vector2 origin = (Vector2)transform.position + Vector2.up * controller.skinWidth;

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
        currentObstacle = obstacle;

        movement.IsExternallyMoving = true;
        movement.CurrentState = PlayerState.Climbing;
        movement.Velocity = Vector3.zero;

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

        if (obstacle.exitPoint != null)
        {
            Vector3 targetY = new Vector3(
                transform.position.x,
                obstacle.exitPoint.position.y,
                transform.position.z
            );

            yield return MoveTo(targetY);

            Vector3 targetX = new Vector3(
                obstacle.exitPoint.position.x,
                transform.position.y,
                transform.position.z
            );

            yield return MoveTo(targetX);
        }

        controller.rigidBody2D.gravityScale = movement.OriginalGravityScale;

        yield return movement.EnsureGrounded();

        movement.CurrentState = PlayerState.Idle;
        movement.IsExternallyMoving = false;
    }

    // ---------- DROP DOWN LOGIC ----------
    public void TryDrop()
    {
        PlayerState state = movement.CurrentState;
        if (state != PlayerState.Idle && state != PlayerState.Walking)
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

        StartCoroutine(ExecuteHang(obstacle));
    }

    public void HangClimbUp()
    {
        if (movement.CurrentState != PlayerState.Hanging || !canMakeHangDecision)
            return;

        StartCoroutine(ClimbBackUp(currentObstacle));
    }

    public void HangDropDown()
    {
        if (movement.CurrentState != PlayerState.Hanging || !canMakeHangDecision)
            return;

        StartCoroutine(DropDown(currentObstacle));
    }

    // ---------- DROP DOWN HELPER ----------
    private IEnumerator ExecuteHang(ClimbableObstacle obstacle)
    {
        currentObstacle = obstacle;

        movement.IsExternallyMoving = true;
        canMakeHangDecision = false;
        movement.CurrentState = PlayerState.Hanging;

        movement.Velocity = Vector3.zero;

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
        movement.CurrentState = PlayerState.Droping;

        controller.rigidBody2D.gravityScale = movement.OriginalGravityScale;
        movement.IsExternallyMoving = false;

        if (obstacle.dropPoint != null)
            yield return MoveTo(obstacle.dropPoint.position);

        yield return movement.EnsureGrounded();

        movement.CurrentState = PlayerState.Idle;
    }

    private IEnumerator ClimbBackUp(ClimbableObstacle obstacle)
    {
        canMakeHangDecision = false;
        movement.CurrentState = PlayerState.Climbing;

        controller.rigidBody2D.gravityScale = movement.OriginalGravityScale;

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

        movement.IsExternallyMoving = false;
        movement.CurrentState = PlayerState.Idle;
    }

    // ---------- MOVEMENT HELPERS ----------
    private IEnumerator MoveTo(Vector3 target)
    {
        float duration = currentObstacle.traversalDuration;
        float elapsed = 0f;

        Vector3 start = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            Vector3 next = Vector3.Lerp(start, target, elapsed / duration);
            Vector3 delta = next - transform.position;

            controller.move(delta);

            yield return null;
        }
    }
}
