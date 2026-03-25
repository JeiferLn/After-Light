using System;
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


    public void OnAimInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerController.PlayerStatus = PlayerStatus.Aiming;
            return;
        }

        playerController.PlayerStatus = PlayerStatus.Idle;
    }

    public void OnInventoryInput(InputAction.CallbackContext context)
    {
        if (context.performed && playerController.PlayerStatus != PlayerStatus.Inventory)
        {
            playerController.PlayerStatus = PlayerStatus.Inventory;
            return;
        }

        playerController.PlayerStatus = PlayerStatus.Idle;
    }

    public void OnMoveSectionInventoryInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Logica para moverse en la seccion de inventario
        }
    }
}