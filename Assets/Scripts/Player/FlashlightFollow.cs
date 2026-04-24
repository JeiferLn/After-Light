using UnityEngine;

public class FlashlightFollow : MonoBehaviour
{
    [SerializeField] private LookController lookController;
    [SerializeField] private PlayerController playerController;

    [Tooltip("Velocidad de rotación en idle.")]
    [SerializeField] private float rotationSpeedIdle = 10f;
    [Tooltip("Velocidad de rotación cuando el jugador no está en idle.")]
    [SerializeField] private float rotationSpeedActive = 40f;
    [Tooltip("Ángulo de rotación (grados) de la linterna.")]
    [SerializeField] private float flashlightRotationAngle = -100f;

    void LateUpdate()
    {
        if (lookController == null) return;

        Vector3 dir = (lookController.CurrentLookPosition - transform.position).normalized;

        Quaternion targetRot = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, flashlightRotationAngle, 0f);

        bool isIdle = playerController == null || playerController.PlayerStatus == PlayerStatus.Idle;
        float speed = isIdle ? rotationSpeedIdle : rotationSpeedActive;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * speed
        );
    }
}