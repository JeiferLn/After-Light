using UnityEngine;

public class FlashlightFollow : MonoBehaviour
{
    [SerializeField] private PlayerLookTargetProvider lookTargetProvider;
    [SerializeField] private PlayerController playerController;

    [Tooltip("Velocidad de rotación en idle.")]
    [SerializeField] private float rotationSpeedIdle = 10f;
    [Tooltip("Velocidad de rotación cuando el jugador no está en idle.")]
    [SerializeField] private float rotationSpeedActive = 40f;
    [Tooltip("Velocidad de rotación durante la transición de interactuable (más bajo = más suavizado).")]
    [SerializeField] private float rotationSpeedInteractableTransition = 8f;
    [Tooltip("Ángulo de rotación (grados) de la linterna.")]
    [SerializeField] private float flashlightRotationAngle = -100f;

    // Al activarse el GameObject (p.ej. al encender la linterna por primera vez), la rotación
    // serializada queda obsoleta. Hacemos snap inmediato al target para evitar el "barrido"
    // de la rotación inicial hacia la correcta en los primeros frames.
    void OnEnable()
    {
        if (TryComputeTargetRotation(out Quaternion targetRot))
            transform.rotation = targetRot;
    }

    void LateUpdate()
    {
        if (!TryComputeTargetRotation(out Quaternion targetRot)) return;

        bool isIdle = playerController == null || PlayerStatusHelpers.IsIdle(playerController.PlayerStatus);

        float speed;
        if (lookTargetProvider.IsInteractableSmoothingActive)
            speed = rotationSpeedInteractableTransition;
        else
            speed = isIdle ? rotationSpeedIdle : rotationSpeedActive;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * speed
        );
    }

    private bool TryComputeTargetRotation(out Quaternion targetRot)
    {
        targetRot = Quaternion.identity;
        if (lookTargetProvider == null) return false;

        // Defensivo: garantiza valores frescos por si nadie computó este frame.
        lookTargetProvider.UpdateFrame();

        Vector3 dir = lookTargetProvider.CurrentLookPosition - transform.position;
        if (dir.sqrMagnitude < 1e-6f) return false;
        dir.Normalize();

        targetRot = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, flashlightRotationAngle, 0f);
        return true;
    }
}
