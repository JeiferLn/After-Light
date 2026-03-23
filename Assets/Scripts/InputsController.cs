using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class InputsController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera normalCamera;
    [SerializeField] private CinemachineCamera aimCamera;
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
        if (playerController == null) return;

        if (context.performed)
        {
            playerController.PlayerStatus = PlayerStatus.Aiming;
            aimCamera.Priority = 20;
            normalCamera.Priority = 10;
        }
        else if (context.canceled)
        {
            playerController.PlayerStatus = PlayerStatus.Idle;
            aimCamera.Priority = 10;
            normalCamera.Priority = 20;
        }
    }
}