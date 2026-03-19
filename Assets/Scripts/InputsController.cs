using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class InputsController : MonoBehaviour
{
    [SerializeField]
    private GameObject targetCamera;
    private CameraAimController cameraAimController;
    private PlayerMovement playerMovement;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        cameraAimController = targetCamera.GetComponent<CameraAimController>();
    }
    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (playerMovement == null) return;
        Vector2 input = context.ReadValue<Vector2>();
        playerMovement.SetMovement(input);
    }

    public void OnAimInput(InputAction.CallbackContext context)
    {
        if (cameraAimController == null) return;

        bool isAiming;

        if (context.performed)
        {
            isAiming = true;
        }
        else
        {
            isAiming = false;
        }
        cameraAimController.SetTargetPosition(isAiming);
    }
}