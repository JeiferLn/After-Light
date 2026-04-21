using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class InputsController : MonoBehaviour
{
    [Header("Suavizado — movimiento")]
    [Tooltip("Más alto = menos tirón al alternar dirección, pero responde un poco más tarde.")]
    [SerializeField][Range(0.02f, 0.35f)] private float moveSmoothTime = 0.1f;
    [Tooltip("Debajo de este módulo, el input se trata como cero.")]
    [SerializeField][Range(0f, 0.25f)] private float moveDeadZone = 0.08f;

    [Header("Suavizado — mirada (cámara)")]
    [Tooltip("0 = sin suavizar (suele ir mejor con ratón). Subir un poco con mando.")]
    [SerializeField][Range(0f, 0.2f)] private float lookSmoothTime = 0f;
    [SerializeField][Range(0f, 0.05f)] private float lookDeadZone = 0f;

    [Header("Suavizado — sprint")]
    [Tooltip("0 = encendido/apagado instantáneo.")]
    [SerializeField][Range(0f, 0.15f)] private float sprintSmoothTime = 0.04f;

    private PlayerController playerController;
    private CameraController cameraController;

    private Vector2 rawMove;
    private Vector2 smoothedMove;
    private Vector2 moveSmoothVelocity;

    private Vector2 rawLook;
    private Vector2 smoothedLook;
    private Vector2 lookSmoothVelocity;

    private float rawSprint;
    private float smoothedSprint;
    private float sprintSmoothVelocity;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        cameraController = GetComponentInChildren<CameraController>();
    }

    void Update()
    {
        if (playerController != null)
        {
            Vector2 moveTarget = rawMove;
            if (moveTarget.sqrMagnitude < moveDeadZone * moveDeadZone)
                moveTarget = Vector2.zero;

            float mt = Mathf.Max(0.0001f, moveSmoothTime);
            smoothedMove = Vector2.SmoothDamp(smoothedMove, moveTarget, ref moveSmoothVelocity, mt);
            if (smoothedMove.sqrMagnitude < moveDeadZone * moveDeadZone * 0.25f)
                smoothedMove = Vector2.zero;

            playerController.SetMovementInput(smoothedMove);

            if (sprintSmoothTime <= 0f)
            {
                smoothedSprint = rawSprint;
                sprintSmoothVelocity = 0f;
            }
            else
            {
                float st = Mathf.Max(0.0001f, sprintSmoothTime);
                smoothedSprint = Mathf.SmoothDamp(smoothedSprint, rawSprint, ref sprintSmoothVelocity, st);
            }

            playerController.SetSprint(smoothedSprint > 0.5f);
        }

        if (cameraController != null)
        {
            Vector2 lookTarget = rawLook;
            if (lookDeadZone > 0f && lookTarget.sqrMagnitude < lookDeadZone * lookDeadZone)
                lookTarget = Vector2.zero;

            bool aiming = playerController != null && PlayerStatusHelpers.IsAimingStatus(playerController.PlayerStatus);

            if (aiming)
            {
                smoothedLook = lookTarget;
                lookSmoothVelocity = Vector2.zero;
            }
            else if (lookSmoothTime <= 0f)
            {
                smoothedLook = lookTarget;
                lookSmoothVelocity = Vector2.zero;
            }
            else
            {
                float lt = Mathf.Max(0.0001f, lookSmoothTime);
                smoothedLook = Vector2.SmoothDamp(smoothedLook, lookTarget, ref lookSmoothVelocity, lt);
            }

            if (lookDeadZone > 0f && smoothedLook.sqrMagnitude < lookDeadZone * lookDeadZone * 0.25f)
                smoothedLook = Vector2.zero;

            cameraController.SetLook(smoothedLook);
        }
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        rawMove = context.ReadValue<Vector2>();
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        rawLook = context.ReadValue<Vector2>();
    }

    public void OnCrounchInput(InputAction.CallbackContext context)
    {
        if (playerController == null || !context.performed)
            return;

        if (playerController.PlayerStatus != PlayerStatus.Crounched)
            playerController.PlayerStatus = PlayerStatus.Crounched;
        else
            playerController.PlayerStatus = PlayerStatus.Idle;
    }

    public void OnRunInput(InputAction.CallbackContext context)
    {
        rawSprint = context.ReadValueAsButton() ? 1f : 0f;
    }

    public void OnAimInput(InputAction.CallbackContext context)
    {
        if (playerController == null)
            return;

        PlayerStatus current = playerController.PlayerStatus;

        if (context.performed)
        {
            playerController.PlayerStatus = current == PlayerStatus.Crounched
                ? PlayerStatus.CrounchAiming
                : PlayerStatus.Aiming;
            return;
        }

        if (context.canceled)
        {
            playerController.PlayerStatus = current == PlayerStatus.CrounchAiming
                ? PlayerStatus.Crounched
                : PlayerStatus.Idle;
        }
    }

    public void OnInventoryInput(InputAction.CallbackContext context)
    {
        if (playerController == null) return;

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
