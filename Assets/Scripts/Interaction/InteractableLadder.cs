using System.Collections;
using System.Linq;
using Prime31;
using UnityEngine;

public class InteractableLadder : MonoBehaviour
{
    // ---------- STATIC ----------
    public static Vector2 LadderMovementInput { get; set; }

    // ---------- COMPONENTS ----------
    private PlayerMovementController playerMovementController;
    private CharacterController2D controller;

    // ---------- STATE ----------
    private LadderObstacle currentLadder;

    // ---------- AWAKE ----------
    private void Awake()
    {
        playerMovementController = GetComponent<PlayerMovementController>();
        controller = GetComponent<CharacterController2D>();
    }

    // ---------- UPDATE ----------
    private void Update()
    {
        if (
            LadderMovementInput != Vector2.zero
            && currentLadder != null
            && playerMovementController.CurrentState == PlayerState.Climbing
        )
        {
            MovePlayerLadder(LadderMovementInput);
        }
    }

    // ---------- TRY CLIMB ----------
    public IEnumerator TryClimb(
        PlayerMovementController playerMovementController,
        LadderObstacle ladder
    )
    {
        this.playerMovementController = playerMovementController;
        currentLadder = ladder;

        if (this.playerMovementController == null || currentLadder == null)
            yield break;

        this.playerMovementController.CurrentState = PlayerState.Climbing;

        if (ladder.startPoints != null)
        {
            // Select the nearest start point to the player
            Transform startPoint = ladder
                .startPoints.OrderBy(point =>
                    Vector2.Distance(playerMovementController.transform.position, point.position)
                )
                .FirstOrDefault();

            // Player position is the same for both moves
            Vector3 playerPosition = playerMovementController.transform.position;

            // Move to the start point in X axis
            yield return MoveTo(
                new Vector3(startPoint.position.x, playerPosition.y, playerPosition.z)
            );

            // Move to the start point in Y axis
            yield return MoveTo(
                new Vector3(playerPosition.x, startPoint.transform.position.y, playerPosition.z)
            );
        }
    }

    // ---------- MOVE PLAYER LADDER ----------
    private void MovePlayerLadder(Vector2 movementInput)
    {
        Vector3 movement = new Vector3(0, movementInput.y, 0);
        controller.move(movement * Time.deltaTime);
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        yield return MovementHelper.MoveTo(
            playerMovementController.transform,
            delta => controller.move(delta),
            target,
            currentLadder.traversalDuration
        );
    }
}
