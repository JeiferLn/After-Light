using System;
using Prime31;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    private PlayerController playerController;
    private CharacterController2D characterController;

    [Header("Interaction (Entrance)")]
    [SerializeField]
    float interactEntranceRange = 1.5f;

    [SerializeField]
    LayerMask entranceLayer;

    [Header("Obstacle Detection (Climb/Drop)")]
    [SerializeField]
    float checkDistance = 0.6f;

    [SerializeField]
    LayerMask obstacleLayer;

    float ePressedTime;
    float lastTapTime;

    InteractableEntrance currentEntrance;

    // ---------- AWAKE ----------
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController2D>();
    }

    // ---------- PUBLIC METHODS ----------
    public void InputMovement(InputAction.CallbackContext ctx)
    {
        if (!playerController.CanMove)
            return;
        Vector2 moveDirection = ctx.canceled ? Vector2.zero : ctx.ReadValue<Vector2>();
        playerController.Move(moveDirection);

        if (moveDirection.sqrMagnitude > 0.01f)
            PlayerMovementEvents.NotifyPlayerMoved();
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
    public void InputInteract(InputAction.CallbackContext ctx)
    {
        DetectInteractableEntrance();

        if (currentEntrance == null)
            return;

        if (ctx.performed)
        {
            if (Time.time - lastTapTime < 0.25f)
            {
                currentEntrance.OpenOrCloseFast();
                return;
            }

            ePressedTime = Time.time;
            lastTapTime = Time.time;
        }

        if (ctx.canceled)
        {
            float heldTime = Time.time - ePressedTime;

            if (heldTime > 0.35f)
                currentEntrance.OpenOrCloseSlow();
            else
                currentEntrance.Peek();
        }
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

    void DetectInteractableEntrance()
    {
        Vector2 direction = Vector2.right * Mathf.Sign(transform.localScale.x);

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            interactEntranceRange,
            entranceLayer
        );

        Debug.DrawRay(transform.position, direction * interactEntranceRange, Color.green);

        if (hit && hit.collider.TryGetComponent(out InteractableEntrance entrance))
            currentEntrance = entrance;
        else
            currentEntrance = null;
    }
}
