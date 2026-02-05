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

    public void TryClimb(ClimbableObstacle obstacle)
    {
        if (climbController == null || obstacle == null)
            return;

        climbController.TryClimb(obstacle);
    }

    public void TryDrop(ClimbableObstacle obstacle)
    {
        if (climbController == null || obstacle == null)
            return;

        climbController.TryDrop(obstacle);
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
