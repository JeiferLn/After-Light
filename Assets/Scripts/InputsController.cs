using UnityEngine;
using UnityEngine.InputSystem;

public class InputsController : MonoBehaviour
{
    // [SerializeField] private CameraAimController cameraAimController;
    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (playerController == null) return;

        Vector2 input = context.ReadValue<Vector2>();
        playerController.SetMovement(input);
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        // if (cameraAimController == null) return;

        // Vector2 input = context.ReadValue<Vector2>();
        // cameraAimController.SetLookInput(input);
    }
}