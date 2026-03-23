using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class InputsController : MonoBehaviour
{
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

    public void OnAimInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerController.PlayerStatus = PlayerStatus.Aiming;
        }
        else if (context.canceled)
        {
            playerController.PlayerStatus = PlayerStatus.Idle;
        }
    }
}