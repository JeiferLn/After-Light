using Prime31;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    // ---------- COMPONENTS ----------
    private PlayerController playerController;
    private PlayerInteractor playerInteractor;
    private CharacterController2D characterController;

    // ---------- OBSTACLE DETECTION (CLIMB/DROP) ----------
    [Header("Obstacle Detection (Climb/Drop)")]
    [SerializeField]
    private float checkDistance = 0.6f;

    [SerializeField]
    private LayerMask obstacleLayer;

    // ---------- AWAKE ----------
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerInteractor = GetComponent<PlayerInteractor>();
        characterController = GetComponent<CharacterController2D>();
    }

    // ---------- PUBLIC METHODS ----------
    public void InputMovement(InputAction.CallbackContext ctx)
    {
        if (!playerController.CanMove)
            return;

        Vector2 moveDirection = ctx.canceled ? Vector2.zero : ctx.ReadValue<Vector2>();
        playerController.Move(moveDirection);
    }

    public void InputClimb(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        ClimbableObstacle obstacle = RaycastClimbObstacle();
        if (obstacle != null)
            playerController.TryClimb(obstacle);
    }

    public void InputDrop(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        ClimbableObstacle obstacle = RaycastDropObstacle();
        if (obstacle != null)
            playerController.TryDrop(obstacle);
    }

    public void InputHangDecision(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        bool isClimbUp = ctx.control != null && ctx.control.name.ToLower().Contains("w");
        if (isClimbUp)
            playerController.HangClimbUp();
        else
            playerController.HangDropDown();
    }

    // ---------- INTERACTION ----------
    public void InputInteraction(InputAction.CallbackContext ctx)
    {
        if (playerInteractor == null)
            return;

        if (ctx.started)
        {
            playerInteractor.OnInteractionStarted();
        }
        else if (ctx.canceled)
        {
            playerInteractor.OnInteractionCanceled();
        }
    }

    // ---------- PEEK LOOK ----------
    public void InputPeekLook(InputAction.CallbackContext ctx)
    {
        VisionCone.PeekLookInput = ctx.ReadValue<Vector2>();
    }

    // ---------- RAYCAST ----------
    ClimbableObstacle RaycastClimbObstacle()
    {
        Vector2 direction = Vector2.right * Mathf.Sign(transform.localScale.x);
        float skin = characterController != null ? characterController.skinWidth : 0.02f;
        Vector2 origin = (Vector2)transform.position + Vector2.up * skin;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, checkDistance, obstacleLayer);
        if (!hit)
            return null;

        return hit.collider.GetComponent<ClimbableObstacle>();
    }

    ClimbableObstacle RaycastDropObstacle()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            checkDistance,
            obstacleLayer
        );

        if (!hit)
            return null;

        ClimbableObstacle obstacle = hit.collider.GetComponent<ClimbableObstacle>();
        if (obstacle == null || obstacle.hangPoint == null)
            return null;

        return obstacle;
    }
}
