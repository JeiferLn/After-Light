using System.Collections;
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
    private RoomDarkness room;

    [SerializeField]
    private VisionCone visionCone;

    // ------------- VARIABLES -------------
    private bool opened;

    // ------------- PUBLIC METHODS -------------
    public void Peek()
    {
        if (opened)
            return;

        if (visionCone != null && visionCone.IsVisible)
            visionCone.Hide();
        else if (visionCone != null)
            visionCone.ShowCone();
    }

    public void OpenOrCloseSlow()
    {
        if (opened)
            StartCoroutine(CloseRoutine(1.5f));
        else
            StartCoroutine(OpenRoutine(1.5f));
    }

    public void OpenOrCloseFast()
    {
        if (opened)
            StartCoroutine(CloseRoutine(0.3f));
        else
            StartCoroutine(OpenRoutine(0.3f));
    }

    // ------------- OPEN LOGIC -------------
    IEnumerator OpenRoutine(float time)
    {
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
        room.RevealRoom();
    }

    // ------------- CLOSE LOGIC --------------
    IEnumerator CloseRoutine(float time)
    {
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
        }
    }
}
