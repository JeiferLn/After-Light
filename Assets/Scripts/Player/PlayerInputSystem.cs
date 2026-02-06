using System;
using System.Collections;
using Prime31;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    // ---------- PLAYER CONTROLLER ----------
    private PlayerController playerController;
    private PlayerMovementController playerMovementController;
    private CharacterController2D characterController;

    // ---------- PLAYER MOVEMENT CONTROLLER ----------
    [Header("Interaction (Entrance)")]
    [SerializeField]
    float interactEntranceRange = 1.5f;

    [SerializeField]
    LayerMask entranceLayer;

    // ---------- OBSTACLE DETECTION (CLIMB/DROP) ----------
    [Header("Obstacle Detection (Climb/Drop)")]
    [SerializeField]
    float checkDistance = 0.6f;

    [SerializeField]
    LayerMask obstacleLayer;

    // ---------- INTERACTION (ENTRANCE) ----------
    float lastTapTime;
    bool peekTriggeredThisHold;
    Coroutine peekHoldRoutine;

    // ---------- CURRENT ENTRANCE ----------
    InteractableEntrance currentEntrance;

    // ---------- AWAKE ----------
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerMovementController = GetComponent<PlayerMovementController>();
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

        if (ctx.started)
        {
            peekTriggeredThisHold = false;
            if (peekHoldRoutine != null)
                StopCoroutine(peekHoldRoutine);
            peekHoldRoutine = StartCoroutine(PeekAfterHold());
            return;
        }

        if (ctx.canceled)
        {
            if (peekHoldRoutine != null)
            {
                StopCoroutine(peekHoldRoutine);
                peekHoldRoutine = null;
            }

            if (peekTriggeredThisHold)
                return;

            float timeSinceLastTap = Time.time - lastTapTime;
            if (
                playerMovementController != null
                && playerMovementController.CurrentState == PlayerState.Peeking
            )
            {
                currentEntrance.Peek(playerMovementController);
                return;
            }
            if (timeSinceLastTap < 0.5f)
            {
                Debug.Log("OpenOrCloseFast");
                currentEntrance.OpenOrCloseFast(playerMovementController);
            }
            else
            {
                Debug.Log("OpenOrCloseSlow");
                currentEntrance.OpenOrCloseSlow(playerMovementController);
            }

            lastTapTime = Time.time;
        }
    }

    // ---------- PEEK AFTER HOLD ----------
    IEnumerator PeekAfterHold()
    {
        yield return new WaitForSeconds(1f);
        peekHoldRoutine = null;
        DetectInteractableEntrance();
        if (currentEntrance != null && playerMovementController != null)
        {
            currentEntrance.Peek(playerMovementController);
            peekTriggeredThisHold = true;
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
