using UnityEngine;
using UnityEngine.InputSystem;

public class InputsController : MonoBehaviour
{
    private PlayerController playerController;
    private CameraController cameraController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        cameraController = GetComponentInChildren<CameraController>();
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (playerController == null) return;

        Vector2 input = context.ReadValue<Vector2>();
        playerController.SetMovement(input);
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        if (cameraController == null) return;

        Vector2 input = context.ReadValue<Vector2>();
        cameraController.SetLook(input);
    }
}