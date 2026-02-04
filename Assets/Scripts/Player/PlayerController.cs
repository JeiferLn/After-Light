using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ---------- COMPONENTS ----------
    private PlayerMovementController movementController;
    private PlayerClimbAndDropController climbController;

    public bool CanMove => movementController != null && movementController.CanMove;

    // ---------- UNITY ----------
    private void Start()
    {
        movementController = GetComponent<PlayerMovementController>();
        climbController = GetComponent<PlayerClimbAndDropController>();
    }

    // ---------- INPUT FACADE ----------
    public void Move(Vector2 moveDirection)
    {
        if (movementController == null)
            return;

        movementController.Move(moveDirection);
    }

    public void TryClimb()
    {
        if (climbController == null)
            return;

        climbController.TryClimb();
    }

    public void TryDrop()
    {
        if (climbController == null)
            return;

        climbController.TryDrop();
    }

    public void HangClimbUp()
    {
        if (climbController == null)
            return;

        climbController.HangClimbUp();
    }

    public void HangDropDown()
    {
        if (climbController == null)
            return;

        climbController.HangDropDown();
    }
}
