using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    // ---------- PRIVATE VARIABLES ----------
    private PlayerController playerController;

    // ---------- AWAKE ----------
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
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
        playerController.TryClimb();
    }

    public void InputDrop(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        playerController.TryDrop();
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
}
