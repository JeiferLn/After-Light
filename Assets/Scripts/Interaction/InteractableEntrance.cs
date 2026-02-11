using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableEntrance : MonoBehaviour
{
    // ------------- VISUALS -------------
    [Header("Entrance Visual")]
    [SerializeField]
    private Transform entranceTransform;

    // ------------- ROOM -------------
    [Header("Room")]
    [SerializeField]
    private List<RoomDarkness> rooms;

    [SerializeField]
    private VisionCone visionCone;

    // ------------- VARIABLES -------------
    private bool opened;
    private bool allRoomsRevealed;

    // ------------- PUBLIC METHODS -------------
    public void Peek(PlayerMovementController playerState)
    {
        if (playerState == null || opened || allRoomsRevealed)
            return;

        if (visionCone != null && visionCone.IsVisible)
        {
            playerState.CurrentState = PlayerState.Idle;
            visionCone.Hide();
        }
        else if (visionCone != null)
        {
            playerState.CurrentState = PlayerState.Peeking;
            visionCone.ShowCone();
        }
    }

    public void ClosePeek(PlayerMovementController playerState)
    {
        if (playerState == null || visionCone == null)
            return;

        if (visionCone.IsVisible)
        {
            playerState.CurrentState = PlayerState.Idle;
            visionCone.Hide();
        }
    }

    public void OpenOrCloseSlow(PlayerMovementController playerState)
    {
        if (playerState == null)
            return;
        if (opened)
            StartCoroutine(CloseRoutine(1.5f, playerState));
        else
            StartCoroutine(OpenRoutine(1.5f, playerState));
    }

    public void OpenOrCloseFast(PlayerMovementController playerState)
    {
        if (playerState == null)
            return;
        if (opened)
            StartCoroutine(CloseRoutine(0.3f, playerState));
        else
            StartCoroutine(OpenRoutine(0.3f, playerState));
    }

    // ------------- OPEN LOGIC -------------
    IEnumerator OpenRoutine(float time, PlayerMovementController playerState)
    {
        playerState.CurrentState = PlayerState.Interacting;
        if (!allRoomsRevealed)
        {
            foreach (RoomDarkness room in rooms)
            {
                if (!room.isRevealed)
                {
                    room.RevealRoom();
                    allRoomsRevealed = false;
                }
            }
            allRoomsRevealed = true;
        }

        opened = true;
        float t = 0;

        Vector3 startPosition = entranceTransform.position;
        Vector3 endPosition = new Vector3(startPosition.x + 0.5f, startPosition.y, startPosition.z);

        Vector3 startScale = entranceTransform.localScale;
        Vector3 endScale = new Vector3(4f, startScale.y, 1);

        while (t < time)
        {
            entranceTransform.position = Vector3.Lerp(startPosition, endPosition, t / time);
            entranceTransform.localScale = Vector3.Lerp(startScale, endScale, t / time);
            t += Time.deltaTime;
            yield return null;
        }

        entranceTransform.position = endPosition;
        entranceTransform.localScale = endScale;

        gameObject.layer = LayerMask.NameToLayer("EntranceOpened");
        playerState.CurrentState = PlayerState.Idle;
    }

    // ------------- CLOSE LOGIC --------------
    IEnumerator CloseRoutine(float time, PlayerMovementController playerState)
    {
        playerState.CurrentState = PlayerState.Interacting;
        {
            opened = false;
            float t = 0;

            Vector3 startPosition = entranceTransform.position;
            Vector3 endPosition = new Vector3(
                startPosition.x - 0.5f,
                startPosition.y,
                startPosition.z
            );

            Vector3 startScale = entranceTransform.localScale;
            Vector3 endScale = new Vector3(1, 1, 1);

            while (t < time)
            {
                entranceTransform.position = Vector3.Lerp(startPosition, endPosition, t / time);
                entranceTransform.localScale = Vector3.Lerp(startScale, endScale, t / time);
                t += Time.deltaTime;
                yield return null;
            }

            entranceTransform.position = endPosition;
            entranceTransform.localScale = endScale;

            gameObject.layer = LayerMask.NameToLayer("EntranceClosed");
            playerState.CurrentState = PlayerState.Idle;
        }
    }
}
